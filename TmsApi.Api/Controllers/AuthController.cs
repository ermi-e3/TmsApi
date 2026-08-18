// using Microsoft.AspNetCore.Mvc;

// namespace TmsApi.Api.Controllers;

// [ApiController]
// [Route("api/{version:apiVersion}/auth")]
// public class AuthController : ControllerBase
// {
//     [HttpPost("login")]
//     public IActionResult Login(
//         [FromBody] LoginRequest request,
//         [FromServices] IWebHostEnvironment env
//     )
//     {
//         // Validate credentials (demo account for M10 transport testing)
//         if (request.Username == "admin" && request.Password == "Password123!")
//         {
//             var dummyJwt = "header.payload.signature-demo-token";
//             // Append HttpOnly authentication cookie — JavaScript CANNOT read this token
//             Response.Cookies.Append(
//                 "tms_auth",
//                 dummyJwt,
//                 new CookieOptions
//                 {
//                     HttpOnly = true,
//                     Secure = !env.IsDevelopment(), // HTTPS in prod; HTTP permitted locally over dev
//                     SameSite = SameSiteMode.Strict,
//                     Expires = DateTimeOffset.UtcNow.AddHours(2),
//                 }
//             );
//             return Ok(new UserProfileDto("System Admin", "Admin"));
//         }
//         return Unauthorized(new { detail = "Invalid username orpassword." });
//     }

//     [HttpGet("me")]
//     public IActionResult GetCurrentUser()
//     {
//         // Inspect cookie attached automatically by the browser on cross-origin requests
//         if (Request.Cookies.TryGetValue("tms_auth", out _))
//         {
//             return Ok(new UserProfileDto("System Admin", "Admin"));
//         }
//         return Unauthorized(new { detail = "Session expired or missing authentication cookie." });
//     }
// }


using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IAntiforgery _antiforgery;

    public AuthController(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    // GET: /api/v2/auth/xsrf
    [HttpGet("xsrf")]
    public IActionResult GetXsrfToken()
    {
        _antiforgery.GetAndStoreTokens(HttpContext);

        return NoContent();
    }

    // POST: /api/v2/auth/login
    [HttpPost("login")]
    public IActionResult Login(
        [FromBody] LoginRequest request,
        [FromServices] IWebHostEnvironment env)
    {
        // Demo credentials
        if (request.Username == "admin" &&
            request.Password == "Password123!")
        {
            var dummyJwt = "header.payload.signature-demo-token";

            Response.Cookies.Append(
                "tms_auth",
                dummyJwt,
                new CookieOptions
                {
                    HttpOnly = true,

                    // HTTP is allowed during local development.
                    Secure = !env.IsDevelopment(),

                    // Works for same-site Angular/API development.
                    SameSite = SameSiteMode.Lax,

                    Expires = DateTimeOffset.UtcNow.AddHours(2),

                    Path = "/"
                }
            );

            return Ok(
                new UserProfileDto(
                    "System Admin",
                    "Admin"
                )
            );
        }

        return Unauthorized(
            new
            {
                detail = "Invalid username or password."
            }
        );
    }

    // GET: /api/v2/auth/me
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        if (Request.Cookies.TryGetValue("tms_auth", out var token)
            && !string.IsNullOrWhiteSpace(token))
        {
            return Ok(
                new UserProfileDto(
                    "System Admin",
                    "Admin"
                )
            );
        }

        return Unauthorized(
            new
            {
                detail = "Session expired or missing authentication cookie."
            }
        );
    }

    // POST: /api/v2/auth/logout
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(
            "tms_auth",
            new CookieOptions
            {
                Path = "/"
            }
        );

        return NoContent();
    }
}