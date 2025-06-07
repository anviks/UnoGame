using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using UnoGame.Core.Entities;
using UnoGame.Core.Interfaces;

namespace WebApp.Handlers;

public class UnoTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUserRepository userRepository)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Request.Cookies["uno_token"];
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.Fail("No token");
        }

        User? user = await userRepository.GetUserByToken(Guid.Parse(token));
        if (user == null)
        {
            return AuthenticateResult.Fail("Invalid token");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}