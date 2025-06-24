using AdvancedEfCore.Api.Data;
using AdvancedEfCore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEfCore.Api.Services;

public class UserService(ApplicationDbContext context) : IUserService
{
    public async Task<IEnumerable<User>> GetUsersWithFullNamesAsync()
    {
        // Example of using scalar function in LINQ query
        var users = await context.Users
            .Where(u => u.IsActive)
            .Select(u => new User
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive,
                Salary = u.Salary,
                Department = u.Department
            })
            .ToListAsync();

        return users;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await context.Users
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateUserAsync(int id, User user)
    {
        var existingUser = await context.Users.FindAsync(id);
        if (existingUser == null)
            return null;

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.IsActive = user.IsActive;
        existingUser.Salary = user.Salary;
        existingUser.Department = user.Department;

        await context.SaveChangesAsync();
        return existingUser;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null)
            return false;

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return true;
    }
}
