using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Models;
using TmsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Register TmsDbContext scoped for incoming HTTP requests
// builder.Services.AddDbContext<TmsDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
// );

builder.Services.AddDbContext<TmsDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddControllers();

builder.Services.AddOpenApi(); // Required before MapOpenApi() will work

builder.Services.AddProblemDetails();

/// SWager
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

builder.Services.AddSingleton<EnrollmentWorker>();

//BUG: make it work with scoped
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();

builder.Services.AddSingleton<IStudentService, StudentService>();
builder.Services.AddSingleton<ICourseService, CourseService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

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

// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// }))
// .RequireAuthorization();

// app.MapPost("/api/enrollments",
//     async (IEnrollmentService service, EnrollmentRequest request) =>
// {
//     var enrollment = await service.EnrollAsync(
//         request.StudentId,
//         request.CourseCode);

//     return Results.Ok(enrollment);
// });

// app.MapGet(
//     "/api/error",
//     () =>
//     {
//         throw new InvalidOperationException("This is a test exception");
//     }
// );

// Seed test data at startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate(); // Applies any pending migrations; keeps migration history intact
    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new()
            {
                RegistrationNumber = "TMS-2026-0001",
                Name = "Alice Smith",
                GPA = 3.8m,
                IsActive = true,
            },
            new()
            {
                RegistrationNumber = "TMS-2026-0002",
                Name = "Bob Jones",
                GPA = 2.9m,
                IsActive = true,
            },
            new()
            {
                RegistrationNumber = "TMS-2026-0003",
                Name = "Charlie Brown",
                GPA = 3.4m,
                IsActive = false,
            },
            new()
            {
                RegistrationNumber = "TMS-2026-0004",
                Name = "DianaPrince",
                GPA = 3.9m,
                IsActive = true,
            },
            new()
            {
                RegistrationNumber = "TMS-2026-0005",
                Name = "EvanWright",
                GPA = 2.5m,
                IsActive = true,
            },
        };

        context.Students.AddRange(students);
        var courses = new List<Course>
        {
            new()
            {
                Code = "CS-101",
                Title = "Introduction to ComputerScience",
                Capacity = 30,
            },
            new()
            {
                Code = "CS-201",
                Title = "Data Structures and Algorithms",
                Capacity = 25,
            },
            new()
            {
                Code = "MAT-101",
                Title = "Calculus I",
                Capacity = 40,
            },
        };

        context.Courses.AddRange(courses);
        context.SaveChanges();
        var enrollments = new List<Enrollment>
        {
            new()
            {
                StudentId = students[0].Id,
                CourseId = courses[0].Id,
                Grade = 4.0m,
            },
            new()
            {
                StudentId = students[0].Id,
                CourseId = courses[1].Id,
                Grade = 3.6m,
            },
            new()
            {
                StudentId = students[1].Id,
                CourseId = courses[0].Id,
                Grade = 2.8m,
            },
            new()
            {
                StudentId = students[3].Id,
                CourseId = courses[1].Id,
                Grade = 3.9m,
            },
        };

        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }
}

app.MapControllers();

app.Run();
