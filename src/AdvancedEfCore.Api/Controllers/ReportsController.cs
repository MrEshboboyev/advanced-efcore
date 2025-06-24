using AdvancedEfCore.Api.Models.DTOs;
using AdvancedEfCore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedEfCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<UserReportDto>> GetUserReport(int userId)
    {
        var report = await reportService.GetUserReportAsync(userId);
        if (report == null)
            return NotFound($"No report found for user ID {userId}");

        return Ok(report);
    }

    [HttpGet("top-customers")]
    public async Task<ActionResult<IEnumerable<UserReportDto>>> GetTopCustomers([FromQuery] int limit = 10)
    {
        var customers = await reportService.GetTopCustomersAsync(limit);
        return Ok(customers);
    }

    [HttpGet("department-summary")]
    public async Task<ActionResult<IEnumerable<DepartmentSummaryDto>>> GetDepartmentSummary()
    {
        var summary = await reportService.GetDepartmentSummaryAsync();
        return Ok(summary);
    }

    [HttpPost("bulk-update-status")]
    public async Task<ActionResult<StoredProcedureResult>> BulkUpdateDepartmentStatus(
        [FromQuery] string department,
        [FromQuery] bool isActive)
    {
        var result = await reportService.BulkUpdateDepartmentStatusAsync(department, isActive);
        return Ok(result);
    }
}
