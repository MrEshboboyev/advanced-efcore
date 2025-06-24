namespace AdvancedEfCore.Api.Models.DTOs;

public class DepartmentSummaryDto
{
    public string Department { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal TotalRevenue { get; set; }
}
