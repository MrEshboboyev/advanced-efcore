namespace AdvancedEfCore.Api.Models.DTOs;

public class StoredProcedureResult
{
    public string Message { get; set; } = string.Empty;
    public int AffectedRows { get; set; }
}
