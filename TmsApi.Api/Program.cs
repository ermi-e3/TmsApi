using System.Threading.Channels;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Scalar.AspNetCore;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Api.Filters;
using TmsApi.Api.Hubs;
using TmsApi.Api.Middlewares;
using TmsApi.Api.Notifications;
using TmsApi.Api.RateLimiting;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Hubs;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Notifications;
using TmsApi.Application.Services;
using TmsApi.Application.TranscriptJobModel;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Persistence.Repositories;
using TmsApi.Infrastructure.Repositories;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Infrastructure.Workers;
using TmsApi.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITranscriptNotificationService, SignalRTranscriptNotificationService>();

builder.Services.AddSignalR();

builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();

builder.Services.AddSingleton(
    Channel.CreateBounded<TranscriptRequest>(
        new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait }
    )
);

builder.Services.AddHostedService<TranscriptWorker>();

// NOTE: RateLimiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);

        return tier switch
        {
            ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"paid:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 200,
                    TokensPerPeriod = 100,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }
            ),

            ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"free:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 30,
                    TokensPerPeriod = 10,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }
            ),

            _ => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"anon:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 10,
                    TokensPerPeriod = 5,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }
            ),
        };
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ts))
        {
            retryAfter = ((int)ts.TotalSeconds).ToString();
        }

        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        context.HttpContext.Response.ContentType = "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = "Rate limit exceeded",
                Detail = $"Too many requests. Retry after {retryAfter} seconds.",
                Status = StatusCodes.Status429TooManyRequests,
                Type = "https://tms.local/errors/rate_limit_exceeded",
            },
            ct
        );
    };

    options.AddConcurrencyLimiter(
        "transcripts",
        opt =>
        {
            opt.PermitLimit = 3;
            opt.QueueLimit = 20;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        }
    );

    options.AddTokenBucketLimiter(
        "search",
        opt =>
        {
            opt.TokenLimit = 10;
            opt.TokensPerPeriod = 5;
            opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            opt.QueueLimit = 2;
        }
    );
});

// NOTE: note
// builder.Services.AddRateLimiter(options =>
// {
//     options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
//     {
//         var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);
//         return tier switch
//         {
//             ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(
//                 partitionKey: $"paid:{partitionKey}",
//                 factory: _ => new TokenBucketRateLimiterOptions
//                 {
//                     TokenLimit = 200,
//                     TokensPerPeriod = 100,
//                     ReplenishmentPeriod = TimeSpan.FromSeconds(10),
//                     QueueLimit = 0,
//                     AutoReplenishment = true,
//                 }
//             ),
//             ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(
//                 partitionKey: $"free:{partitionKey}",
//                 factory: _ => new TokenBucketRateLimiterOptions
//                 {
//                     TokenLimit = 30,
//                     TokensPerPeriod = 10,
//                     ReplenishmentPeriod = TimeSpan.FromSeconds(10),
//                     QueueLimit = 0,
//                     AutoReplenishment = true,
//                 }
//             ),
//             _ => RateLimitPartition.GetTokenBucketLimiter(
//                 partitionKey: $"anon:{partitionKey}",
//                 factory: _ => new TokenBucketRateLimiterOptions
//                 {
//                     TokenLimit = 10,
//                     TokensPerPeriod = 5,
//                     ReplenishmentPeriod = TimeSpan.FromSeconds(10),
//                     QueueLimit = 0,
//                     AutoReplenishment = true,
//                 }
//             ),
//         };
//     });

//     options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
//     options.OnRejected = async (context, ct) =>
//     {
//         var retryAfter = "10";
//         if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ts))
//             retryAfter = ((int)ts.TotalSeconds).ToString();
//         context.HttpContext.Response.Headers.RetryAfter = retryAfter;
//         context.HttpContext.Response.ContentType = "application/problem+json";
//         await context.HttpContext.Response.WriteAsJsonAsync(
//             new ProblemDetails
//             {
//                 Title = "Rate limit exceeded",
//                 Detail = $"Too many requests. Retry after {retryAfter} seconds.",
//                 Status = StatusCodes.Status429TooManyRequests,
//                 Type = "https://tms.local/errors/rate_limit_exceeded",
//             },
//             ct
//         );
//     };

//     // options.AddConcurrencyLimiter("transcripts", opt =>
//     options.AddConcurrencyLimiter(
//         "transcripts",
//         opt =>
//         {
//             opt.QueueLimit = 20;
//             opt.QueueLimit = 20;
//             opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//         }
//     );
//     options.AddTokenBucketLimiter(
//         "search",
//         opt =>
//         {
//             opt.TokenLimit = 10;
//             opt.TokensPerPeriod = 5;
//             opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
//             opt.QueueLimit = 2;
//         }
//     );
// });

// NOTE: transcripts RateLimiting
// builder.Services.AddRateLimiter(options =>
// {
// options.AddFixedWindowLimiter(
//     "transcripts",
//     limiterOptions =>
//     {
//         limiterOptions.PermitLimit = 5;
//         limiterOptions.Window = TimeSpan.FromMinutes(1);
//         limiterOptions.QueueLimit = 0;
//     }
// );

//     options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
// });

builder.Services.AddSingleton<ITranscriptQueue, TranscriptQueue>();
builder.Services.AddSingleton<ITranscriptJobStore, TranscriptJobStore>();
builder.Services.AddHealthChecks();

// NOTE: Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAngular",
        policy => policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()
    );
});

// NOTE: caching
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2),
    };
});

builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();

/*
//NOTE: Production-only leave commented in lab
builder.Services.AddStackExchangeRedisCache(options =>
{

options.Configuration = builder.Configuration.GetConnectionStrin
g("Redis");
//
options.InstanceName = "tms:";
});
builder.Services.AddHybridCache();
*/

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
);

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<TmsDbContext>()
);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

// builder.Services.AddControllers();

builder.Services.AddOpenApi(); // Required before MapOpenApi() will work

builder.Services.AddAuthorization();

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<ICourseRepository, CourseRepository>();

builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

builder
    .Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

// NOTE: Versioning
builder.Services.AddOpenApi(
    "v1",
    options =>
    {
        options.ShouldInclude = description => description.GroupName == "v1";
    }
);
builder.Services.AddOpenApi(
    "v2",
    options =>
    {
        options.ShouldInclude = description => description.GroupName == "v2";
    }
);

builder
    .Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = false;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly)
);
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);

// LoggingBehavior FIRST—it must wrap ValidationBehavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.MapHub<TmsHub>("/hubs/tms");
app.UseCors("AllowAngular");

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Scalar
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        // Tell Scalar to pull both documents into its sidebar dropdown
        options.AddDocument("v1", "API Version 1.0").AddDocument("v2", "API Version 2.0");
    });

    // Data seeder
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();

    await DataSeeder.SeedAsync(context);
}
else
{
    app.UseExceptionHandler();
}

// Required order

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<V1DeprecationMiddleware>();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseRouting();

app.UseRateLimiter();

app.MapHealthChecks("/health/live").DisableRateLimiting();
app.MapHealthChecks("/health/ready").DisableRateLimiting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
