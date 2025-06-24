namespace AdvancedEfCore.Api.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
