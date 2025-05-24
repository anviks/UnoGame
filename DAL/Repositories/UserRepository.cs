using DAL.Context;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class UserRepository(UnoDbContext db)
{
    public async Task CreateUser(UserEntity userEntity)
    {
        await db.Users.AddAsync(userEntity);
        await db.SaveChangesAsync();
    }

    public async Task<UserEntity?> GetUserById(int id)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserEntity?> GetUserByToken(Guid token)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Token == token);
    }

    public async Task<UserEntity?> GetUserByEmail(string email)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<UserEntity?> GetUserByName(string name)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Name == name);
    }

    public async Task<List<UserEntity>> GetAllUsers()
    {
        return await db.Users.ToListAsync();
    }

    public async Task<int> CountUsersRegisteredToday()
    {
        return await db.Users.CountAsync(u => u.CreatedAt.Date == DateTime.UtcNow.Date);
    }
}