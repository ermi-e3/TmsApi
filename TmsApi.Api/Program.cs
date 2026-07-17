using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Api.Filters;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Services;
// using TmsApi.Data;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Persistence;

// using TmsApi.Models;
// using TmsApi.Persistence;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddDbContext<TmsDbContext>(options =>
//     options
//         .UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
//         .LogTo(Console.WriteLine, LogLevel.Information)
//         .EnableSensitiveDataLogging()
// );

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

builder.Services.AddProblemDetails();

/// SWager
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

// builder.Services.AddSingleton<EnrollmentWorker>();

//BUG: make it work with scoped
// builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();
// FIX:
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<IStudentService, StudentService>();

// builder.Services.AddSingleton<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder
//     .Services.AddOptions<PaymentOptions>()
//     .BindConfiguration("Payments")
//     .ValidateDataAnnotations()
//     .ValidateOnStart();

builder
    .Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Swagger
    app.UseSwagger();
    app.UseSwaggerUI();

    // Scalar
    app.MapOpenApi();
    app.MapScalarApiReference();

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

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
