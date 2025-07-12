using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UnoGame.Core.DTO;
using UnoGame.Core.Entities;
using UnoGame.Core.Interfaces;

namespace UnoGame.Core.Services;

public class UserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
{
    public async Task CreateUser(User user)
    {
        await userRepository.CreateUser(user);
    }

    public async Task<User?> GetUserById(int id)
    {
        return await userRepository.GetUserById(id);
    }

    public async Task<User?> GetUserByTokenAsync(Guid token)
    {
        return await userRepository.GetUserByToken(token);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await userRepository.GetUserByEmail(email);
    }

    public async Task<User?> GetUserByName(string name)
    {
        return await userRepository.GetUserByName(name);
    }

    public async Task<List<UserDto>> GetAllUserDtos()
    {
        var allUsers = await userRepository.GetAllUsers();
        return allUsers.Select(u => new UserDto { Id = u.Id, Name = u.Name }).ToList();
    }

    public async Task<int> CountUsersRegisteredToday()
    {
        return await userRepository.CountUsersRegisteredToday();
    }

    public async Task<User?> GetCurrentUser()
    {
        ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
        var id = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(id, out var userId))
        {
            return await userRepository.GetUserById(userId);
        }

        return null;
    }
}