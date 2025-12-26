using UnoGame.Core.Entities;

namespace UnoGame.Core.Interfaces;

public interface IUserRepository
{
    public Task<User> CreateUser(User user);
    public Task<User?> GetUserById(int id);
    public Task<User?> GetUserByName(string name);
    public Task<List<User>> GetAllUsers();
    public Task<int> CountUsersRegisteredToday();
}