using AdvancedEfCore.Api.Models.DTOs;

namespace AdvancedEfCore.Api.Services;

public interface IReportService
{
    Task<UserReportDto?> GetUserReportAsync(int userId);
    Task<IEnumerable<UserReportDto>> GetTopCustomersAsync(int limit = 10);
    Task<IEnumerable<DepartmentSummaryDto>> GetDepartmentSummaryAsync();
    Task<StoredProcedureResult> BulkUpdateDepartmentStatusAsync(string department, bool isActive);
}
