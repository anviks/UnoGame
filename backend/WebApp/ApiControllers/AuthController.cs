using System.Security.Claims;
using DAL.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UnoGame.Core.Config;
using UnoGame.Core.Entities;
using UnoGame.Core.Services;
using WebApp.DTO;

namespace WebApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UnoDbContext db,
    UserService userService,
    IOptions<UserLimitsConfig> limitOptions,
    IWebHostEnvironment env)
    : ControllerBase
{
    private readonly UserLimitsConfig _limitsConfig = limitOptions.Value;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest request)
    {
        if (await userService.GetUserByName(request.Username) != null)
            return BadRequest("Username already taken");

        if (await userService.CountUsersRegisteredToday() >= _limitsConfig.MaxUsersPerDay)
            return StatusCode(503, "User registration limit reached for today.");

        User user = await userService.CreateUser(request.Username, request.Password);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new("UserId", user.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        if (!await userService.VerifyLogin(request.Username, request.Password))
            return Unauthorized("Invalid username or password");

        User? user = await userService.GetUserByName(request.Username);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user!.Username),
            new("UserId", user.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [HttpGet("is-username-available")]
    public async Task<IActionResult> IsAvailable([FromQuery] string username)
    {
        if (string.IsNullOrEmpty(username)) return Ok(false);

        User? user = await userService.GetUserByName(username);

        return Ok(user == null);
    }

    [HttpGet("whoami")]
    [Authorize]
    public async Task<IActionResult> WhoAmI()
    {
        User? user = await userService.GetCurrentUser();
        if (user == null) return NotFound("User not found");

        return Ok(new
        {
            user.Id,
            user.Username
        });
    }
}