
///////////////////////////////////////////
/// after installing rest client
/// 

// var builder = WebApplication.CreateBuilder(args);

// // Register authorization services
// builder.Services.AddAuthorization();

// var app = builder.Build();


// // TODO1: Register routing
// app.UseRouting();

// // TODO2: Register authentication and authorization
// app.UseAuthorization();
// app.UseAuthentication();




// // TODO3: Protected minimal API route
// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// }))
// .RequireAuthorization();

// app.Run();
//////////////////////////////////////////////////////////////////
/// 
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/assessments/results", () =>
{
    return Results.Ok(new
    {
        courseCode = "CS-101",
        studentId = "S-001",
        letterGrade = "A"
    });
})
.RequireAuthorization();

app.Run();









/////////////////////////////////////////////////////////////////

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


// // 4. Routing
// app.UseRouting();

// // 5. Authentication
// app.UseAuthentication();

// // 6. Authorization
// app.UseAuthorization();


// // Protected endpoint LAST
// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// }))
// .RequireAuthorization();

// app.Run();