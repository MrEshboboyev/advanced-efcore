namespace AdvancedEfCore.Api.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public decimal? Salary { get; set; }
    public string? Department { get; set; }

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = [];
}
