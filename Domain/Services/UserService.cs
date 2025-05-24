using System.Security.Claims;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;

namespace Domain.Services;

public class UserService(IHttpContextAccessor httpContextAccessor, UserRepository userRepository)
{
    public async Task CreateUser(User user)
    {
        await userRepository.CreateUser(EntityMapper.ToEntity(user));
    }

    public async Task<User?> GetUserById(int id)
    {
        UserEntity? entity = await userRepository.GetUserById(id);
        return entity != null ? EntityMapper.ToDomain(entity) : null;
    }

    public async Task<User?> GetUserByTokenAsync(Guid token)
    {
        UserEntity? entity = await userRepository.GetUserByToken(token);
        return entity != null ? EntityMapper.ToDomain(entity) : null;
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        UserEntity? entity = await userRepository.GetUserByEmail(email);
        return entity != null ? EntityMapper.ToDomain(entity) : null;
    }

    public async Task<User?> GetUserByName(string name)
    {
        UserEntity? entity = await userRepository.GetUserByName(name);
        return entity != null ? EntityMapper.ToDomain(entity) : null;
    }

    public async Task<List<User>> GetAllUsers()
    {
        var entities = await userRepository.GetAllUsers();
        return entities.Select(EntityMapper.ToDomain).ToList();
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
            UserEntity? entity = await userRepository.GetUserById(userId);
            if (entity != null) return EntityMapper.ToDomain(entity);
        }

        return null;
    }
}