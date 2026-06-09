

/////////////////////////////////////////
// using JWt
// var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddControllers();

// // builder.Services.AddAuthentication();
// builder.Services.AddAuthentication("Bearer").AddJwtBearer();

// // builder.Services.AddAuthentication();

// builder.Services.AddAuthorization();

// var app = builder.Build();

// app.UseRouting();

// app.UseAuthentication();
// app.UseAuthorization();

// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//   courseCode = "CS-101",
//   studentId = "S-001",
//   letterGrade = "A"
// })).RequireAuthorization() ;

// app.Run();





/////////////////////////////////////////////////////////////////
// ##################################################
// using Microsoft.AspNetCore.Authentication.Cookies;

// var builder = WebApplication.CreateBuilder(args);

// builder.Services
//     .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(options =>
//     {
//         // Return 401 instead of redirecting
//         options.Events.OnRedirectToLogin = context =>
//         {
//             context.Response.StatusCode = 401;
//             return Task.CompletedTask;
//         };
//     });

// builder.Services.AddAuthorization();

// var app = builder.Build();

// app.UseRouting();

// app.UseAuthentication();
// app.UseAuthorization();

// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// }))
// .RequireAuthorization();

// app.Run();


//////////////////////////////////////////////////////////////////////////////
// using Microsoft.AspNetCore.Authentication.Cookies;

// var builder = WebApplication.CreateBuilder(args);

// // Authentication
// builder.Services
//     .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(options =>
//     {
//         options.Events.OnRedirectToLogin = context =>
//         {
//             context.Response.StatusCode = 401;
//             return Task.CompletedTask;
//         };
//     });

// // Authorization
// builder.Services.AddAuthorization();

// var app = builder.Build();


// // 1. Custom middleware FIRST
// app.UseMiddleware<RequestLoggingMiddleware>();

// // 2. Exception handler
// app.UseExceptionHandler("/error");

// // 3. HTTPS redirection
// app.UseHttpsRedirection();


// app.UseRouting();
// app.UseAuthentication();
// app.UseAuthorization();

// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// }))
// .RequireAuthorization();

// app.Run();


//////////////////////////
/// 
/// 
/// 

// ####################################

// using Microsoft.AspNetCore.Authentication;

// var builder = WebApplication.CreateBuilder(args);

// builder.Services
//     .AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
//         "Training",
//         null);

// builder.Services.AddAuthorization();

// var app = builder.Build();

// app.UseRouting();

// app.UseAuthentication();
// app.UseAuthorization();

// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// }))
// .RequireAuthorization();

// app.Run();

////////////////////////////
/// 
/// 

using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training",
        null);

builder.Services.AddAuthorization();

var app = builder.Build();

// Required order

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler("/error");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}))
.RequireAuthorization();

app.Run();

