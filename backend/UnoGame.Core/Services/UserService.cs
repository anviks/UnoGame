using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using UnoGame.Core.DTO;
using UnoGame.Core.Entities;
using UnoGame.Core.Interfaces;

namespace UnoGame.Core.Services;

public class UserService(IMapper mapper, IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
{
    public async Task<User> CreateUser(string username, string password)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };
        return await userRepository.CreateUser(user);
    }

    public async Task<bool> VerifyLogin(string username, string password)
    {
        User? user = await userRepository.GetUserByName(username);

        return user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<User?> GetUserById(int id)
    {
        return await userRepository.GetUserById(id);
    }

    public async Task<User?> GetUserByName(string name)
    {
        return await userRepository.GetUserByName(name);
    }

    public async Task<List<UserDto>> GetAllUserDtos()
    {
        var allUsers = await userRepository.GetAllUsers();
        return mapper.Map<List<UserDto>>(allUsers);
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