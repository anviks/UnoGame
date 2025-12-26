using DAL.Context;
using Microsoft.EntityFrameworkCore;
using UnoGame.Core.Entities;
using UnoGame.Core.Interfaces;

namespace DAL.Repositories;

public class UserRepository(UnoDbContext db) : IUserRepository
{
    public async Task<User> CreateUser(User user)
    {
        var result = await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<User?> GetUserById(int id)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByName(string name)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Username == name);
    }

    public async Task<List<User>> GetAllUsers()
    {
        return await db.Users.ToListAsync();
    }

    public async Task<int> CountUsersRegisteredToday()
    {
        return await db.Users.CountAsync(u => u.CreatedAt.Date == DateTime.UtcNow.Date);
    }
}