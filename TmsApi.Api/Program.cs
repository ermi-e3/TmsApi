using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Api.Filters;
using TmsApi.Api.Middlewares;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Services;
// using TmsApi.Data;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Persistence.Repositories;
using TmsApi.Persistence;

;

// using TmsApi.Models;
// using TmsApi.Persistence;

var builder = WebApplication.CreateBuilder(args);

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

// /// SWager
// builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

// builder.Services.AddSingleton<EnrollmentWorker>();

//BUG: make it work with scoped
// builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();
// FIX:
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<IStudentService, StudentService>();

// builder.Services.AddSingleton<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<ICourseRepository, CourseRepository>();

builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// builder
//     .Services.AddOptions<PaymentOptions>()
//     .BindConfiguration("Payments")
//     .ValidateDataAnnotations()
//     .ValidateOnStart();

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
        options.AssumeDefaultVersionWhenUnspecified = true;
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

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Swagger
    // app.UseSwagger();
    // app.UseSwaggerUI();

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
