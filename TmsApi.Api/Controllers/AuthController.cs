// using Asp.Versioning;
// using Microsoft.AspNetCore.Antiforgery;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace TmsApi.Api.Controllers;

// [ApiController]
// [Route("api/v{version:apiVersion}/auth")]
// // [Route("api/{version:apiVersion}/auth")]
// [ApiVersion("2.0")]
// public class AuthController : ControllerBase
// {
//     private readonly IAntiforgery _antiforgery;

//     public AuthController(IAntiforgery antiforgery)
//     {
//         _antiforgery = antiforgery;
//     }

//     // GET: /api/v2/auth/xsrf
//     [AllowAnonymous]
//     [HttpGet("xsrf")]
//     public IActionResult GetXsrfToken()
//     {
//         Console.WriteLine("🔥 AuthController.GetXsrfToken() WAS CALLED");
//         _antiforgery.GetAndStoreTokens(HttpContext);

//         return NoContent();
//     }

//     // POST: /api/v2/auth/login
//     [AllowAnonymous]
//     [HttpPost("login")]
//     public IActionResult Login(
//         [FromBody] LoginRequest request,
//         [FromServices] IWebHostEnvironment env
//     )
//     {
//         // Demo credentials
//         if (request.Username == "admin" && request.Password == "Password123!")
//         {
//             var dummyJwt = "header.payload.signature-demo-token";

//             Response.Cookies.Append(
//                 "tms_auth",
//                 dummyJwt,
//                 new CookieOptions
//                 {
//                     HttpOnly = true,

//                     // HTTP is allowed during local development.
//                     Secure = !env.IsDevelopment(),

//                     // Works for same-site Angular/API development.
//                     SameSite = SameSiteMode.Lax,

//                     Expires = DateTimeOffset.UtcNow.AddHours(2),

//                     Path = "/",
//                 }
//             );

//             return Ok(new UserProfileDto("System Admin", "Admin"));
//         }

//         return Unauthorized(new { detail = "Invalid username or password." });
//     }

//     // GET: /api/v2/auth/me
//     [HttpGet("me")]
//     public IActionResult GetCurrentUser()
//     {
//         if (
//             Request.Cookies.TryGetValue("tms_auth", out var token)
//             && !string.IsNullOrWhiteSpace(token)
//         )
//         {
//             return Ok(new UserProfileDto("System Admin", "Admin"));
//         }

//         return Unauthorized(new { detail = "Session expired or missing authentication cookie." });
//     }

//     // POST: /api/v2/auth/logout
//     [HttpPost("logout")]
//     public IActionResult Logout()
//     {
//         Response.Cookies.Delete("tms_auth", new CookieOptions { Path = "/" });

//         return NoContent();
//     }
// }

using Asp.Versioning;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Domain.Entities;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("2.0")]
public class AuthController : ControllerBase
{
    private readonly UserManager<TmsUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAntiforgery _antiforgery;

    public AuthController(
        UserManager<TmsUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IAntiforgery antiforgery
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _antiforgery = antiforgery;
    }

    public record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string Role
    );

    [AllowAnonymous]
    [HttpGet("xsrf")]
    public IActionResult GetXsrfToken()
    {
        Console.WriteLine("🔥 AuthController.GetXsrfToken() WAS CALLED");
        _antiforgery.GetAndStoreTokens(HttpContext);

        return NoContent();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            // Prevent account enumeration by returning a generic response
            return Ok(new { message = "Registration request received." });
        }
        var user = new TmsUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        // Ensure requested role exists
        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(request.Role));
        }

        await _userManager.AddToRoleAsync(user, request.Role);
        return Ok(new { message = "Registration successful." });
    }

    public record LoginRequest(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new { detail = "Invalid credentials." });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(
                423,
                new
                {
                    detail = "Account locked due to multiple failed login attempts. Try again in 15 minutes.",
                }
            );
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);
            return Unauthorized(new { detail = "Invalid credentials." });
        }
        // Reset failed attempt counter on successful login
        await _userManager.ResetAccessFailedCountAsync(user);
        return Ok(
            new
            {
                userId = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
            }
        );
    }
}
