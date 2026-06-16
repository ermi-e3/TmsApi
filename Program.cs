using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;

using TmsApi.Services;
using TmsApi.Models;


var builder = WebApplication.CreateBuilder(args);


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

builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training",
        null);

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

app.MapGet("/api/error", () =>
{
    throw new InvalidOperationException("This is a test exception");
});


app.MapControllers();

app.Run();