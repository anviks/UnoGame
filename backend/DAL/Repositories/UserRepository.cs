using DAL.Context;
using Microsoft.EntityFrameworkCore;
using UnoGame.Core.Entities;
using UnoGame.Core.Interfaces;

namespace DAL.Repositories;

public class UserRepository(UnoDbContext db) : IUserRepository
{
    public async Task CreateUser(User user)
    {
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
    }

    public async Task<User?> GetUserById(int id)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByToken(Guid token)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Token == token);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByName(string name)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Name == name);
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