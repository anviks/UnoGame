using DAL.Context;
using DAL.Entities;
using Domain;
using Domain.Config;
using Domain.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApp.DTO;

namespace WebApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UnoDbContext db, UserService userService, IOptions<EmailConfig> emailOptions, IOptions<UserLimitsConfig> limitOptions, IWebHostEnvironment env)
    : ControllerBase
{
    private readonly UserLimitsConfig _limitsConfig = limitOptions.Value;
    private readonly EmailService _emailService = new(emailOptions.Value, Path.Combine(env.ContentRootPath, "Templates", "MagicLinkEmail.html"));

    [HttpPost("request-magic-link")]
    public async Task<IActionResult> RequestMagicLink([FromBody] MagicLinkRequest dto)
    {
        if (await userService.CountUsersRegisteredToday() >= _limitsConfig.MaxUsersPerDay)
            return StatusCode(503, "User registration limit reached for today.");

        if (await userService.GetUserByEmail(dto.Email) != null)
            return BadRequest("Email already registered.");

        // TODO: Invalidate other tokens for the same email

        var token = new MagicToken { Email = dto.Email };
        db.MagicTokens.Add(token);
        await db.SaveChangesAsync();

        var origin = Request.Headers.Origin.First();
        var link = $"{origin}/register?token={token.Token}";
        await _emailService.SendAsync(dto.Email, "Sign up for UNO Online", link);

        return Ok(new { message = "Magic link sent!" });
    }

    [HttpGet("magic-token/{id:guid}")]
    public async Task<IActionResult> GetMagicToken(Guid id)
    {
        MagicToken? magicToken = await db.MagicTokens.FindAsync(id);
        if (magicToken == null) return NotFound("Magic token not found");
        if (magicToken.Expiry < DateTime.UtcNow) return BadRequest("Magic token expired");
        if (magicToken.Used) return BadRequest("Magic token already used");
        return Ok(magicToken);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        MagicToken? magic = await db.MagicTokens.FirstOrDefaultAsync(t => t.Token == request.Token && !t.Used);
        if (magic == null || magic.Expiry < DateTime.UtcNow)
            return Unauthorized();

        if (await userService.GetUserByName(request.Username) != null)
            return BadRequest("Username already taken");

        if (await userService.GetUserByEmail(magic.Email) != null)
            return BadRequest("Email already registered");

        if (await userService.CountUsersRegisteredToday() >= _limitsConfig.MaxUsersPerDay)
            return StatusCode(503, "User registration limit reached for today.");

        magic.Used = true;

        var user = new UserEntity
        {
            Email = magic.Email,
            Name = request.Username,
        };
        db.Users.Add(user);

        await db.SaveChangesAsync();

        Response.Cookies.Append("uno_token", user.Token.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddYears(1),
        });

        return Ok(new
        {
            token = user.Token,
            name = user.Name
        });
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
            user.Name,
            user.Email
        });
    }
}