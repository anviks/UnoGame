using DAL;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApp.Filters;

public class RequireFingerprintAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var fingerprint = context.HttpContext.Request.Headers["X-Fingerprint"].FirstOrDefault();
        var userRepository = context.HttpContext.RequestServices.GetService(typeof(UserRepository)) as UserRepository;

        if (string.IsNullOrWhiteSpace(fingerprint) || userRepository?.GetUserByEmail(fingerprint).Result == null)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}