using DAL.Repositories;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(UserService userService): ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok(await userService.GetAllUsers());
    }
}