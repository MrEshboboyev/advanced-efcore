using AdvancedEfCore.Api.Models;

namespace AdvancedEfCore.Api.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetUsersWithFullNamesAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(User user);
    Task<User?> UpdateUserAsync(int id, User user);
    Task<bool> DeleteUserAsync(int id);
}
