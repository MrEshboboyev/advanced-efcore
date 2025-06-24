using AdvancedEfCore.Api.Data;
using AdvancedEfCore.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEfCore.Api.Services;

public class ReportService(ApplicationDbContext context) : IReportService
{
    public async Task<UserReportDto?> GetUserReportAsync(int userId)
    {
        // Call stored procedure using EF Core
        var result = await context.GetUserReportFunction(userId)
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<IEnumerable<UserReportDto>> GetTopCustomersAsync(int limit = 10)
    {
        // Call table-valued function
        var results = await context.GetTopCustomers(limit)
            .ToListAsync();

        return results;
    }

    public async Task<IEnumerable<DepartmentSummaryDto>> GetDepartmentSummaryAsync()
    {
        // Complex query using scalar functions within LINQ
        var results = await context.Users
            .Where(u => u.IsActive)
            .GroupBy(u => u.Department)
            .Select(g => new DepartmentSummaryDto
            {
                Department = g.Key ?? "Unknown",
                UserCount = g.Count(),
                AverageSalary = g.Average(u => u.Salary ?? 0),
                TotalRevenue = g.SelectMany(u => u.Orders)
                               .Where(o => o.Status == "Completed")
                               .Sum(o => o.Amount)
            })
            .ToListAsync();

        return results;
    }

    public async Task<StoredProcedureResult> BulkUpdateDepartmentStatusAsync(string department, bool isActive)
    {
        // Call stored procedure for bulk operations
        var result = await context.BulkUpdateUserStatusFunction(department, isActive)
            .FirstOrDefaultAsync();

        return result ?? new StoredProcedureResult { Message = "No operation performed", AffectedRows = 0 };
    }
}
