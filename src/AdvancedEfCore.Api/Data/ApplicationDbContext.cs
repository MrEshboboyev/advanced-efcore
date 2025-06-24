using AdvancedEfCore.Api.Models;
using AdvancedEfCore.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEfCore.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }

    // DbSets for stored procedure results
    public DbSet<UserReportDto> UserReports { get; set; }
    public DbSet<DepartmentSummaryDto> DepartmentSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities
        ConfigureEntities(modelBuilder);

        // Configure stored procedures and functions
        ConfigureStoredProcedures(modelBuilder);
        ConfigureFunctions(modelBuilder);
    }

    private void ConfigureEntities(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.User)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.UserId);
        });

        // Configure keyless entities for stored procedure results
        modelBuilder.Entity<UserReportDto>().HasNoKey();
        modelBuilder.Entity<DepartmentSummaryDto>().HasNoKey();
        modelBuilder.Entity<StoredProcedureResult>().HasNoKey();
    }

    private void ConfigureStoredProcedures(ModelBuilder modelBuilder)
    {
        // Create stored procedures using raw SQL in migrations
        // This approach ensures they're created automatically

        // Stored procedure for user report
        var createUserReportProc = """
                CREATE OR REPLACE FUNCTION GetUserReport(user_id_param INTEGER)
                RETURNS TABLE(
                    UserId INTEGER,
                    FullName VARCHAR(201),
                    Email VARCHAR(255),
                    OrderCount INTEGER,
                    TotalSpent DECIMAL(18,2),
                    LastOrderDate TIMESTAMP
                ) AS $$
                BEGIN
                    RETURN QUERY
                    SELECT 
                        u."Id" as UserId,
                        CONCAT(u."FirstName", ' ', u."LastName") as FullName,
                        u."Email" as Email,
                        COALESCE(COUNT(o."Id")::INTEGER, 0) as OrderCount,
                        COALESCE(SUM(o."Amount"), 0::DECIMAL(18,2)) as TotalSpent,
                        COALESCE(MAX(o."OrderDate"), u."CreatedAt") as LastOrderDate
                    FROM "Users" u
                    LEFT JOIN "Orders" o ON u."Id" = o."UserId"
                    WHERE u."Id" = user_id_param AND u."IsActive" = true
                    GROUP BY u."Id", u."FirstName", u."LastName", u."Email", u."CreatedAt";
                END;
                $$ LANGUAGE plpgsql;
                """;

        // Stored procedure for bulk operations
        var createBulkUpdateProc = """
                CREATE OR REPLACE FUNCTION BulkUpdateUserStatus(
                    department_param VARCHAR(100),
                    new_status BOOLEAN
                )
                RETURNS TABLE(
                    Message VARCHAR(255),
                    AffectedRows INTEGER
                ) AS $$
                DECLARE
                    rows_affected INTEGER;
                BEGIN
                    UPDATE "Users" 
                    SET "IsActive" = new_status
                    WHERE "Department" = department_param;
                    
                    GET DIAGNOSTICS rows_affected = ROW_COUNT;
                    
                    RETURN QUERY
                    SELECT 
                        ('Updated ' || rows_affected || ' users in department: ' || department_param)::VARCHAR(255) as Message,
                        rows_affected as AffectedRows;
                END;
                $$ LANGUAGE plpgsql;
                """;

        // Store SQL for execution during migration
        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(GetUserReportFunction))!)
            .HasName("GetUserReport");

        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(BulkUpdateUserStatusFunction))!)
            .HasName("BulkUpdateUserStatus");
    }

    private void ConfigureFunctions(ModelBuilder modelBuilder)
    {
        // Scalar function configuration
        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(CalculateUserAge))!)
            .HasName("calculate_user_age");

        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(GetFullName))!)
            .HasName("get_full_name");

        // Table-valued function configuration
        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(GetTopCustomers))!)
            .HasName("get_top_customers");
    }

    // Function method signatures for EF Core mapping
    [DbFunction]
    public static int CalculateUserAge(DateTime birthDate)
        => throw new InvalidOperationException("This method should only be called within LINQ-to-Entities queries.");

    [DbFunction]
    public static string GetFullName(string firstName, string lastName)
        => throw new InvalidOperationException("This method should only be called within LINQ-to-Entities queries.");

    [DbFunction]
    public IQueryable<UserReportDto> GetTopCustomers(int limit)
        => FromExpression(() => GetTopCustomers(limit));

    // Stored procedure method signatures
    public IQueryable<UserReportDto> GetUserReportFunction(int userId)
        => FromExpression(() => GetUserReportFunction(userId));

    public IQueryable<StoredProcedureResult> BulkUpdateUserStatusFunction(string department, bool isActive)
        => FromExpression(() => BulkUpdateUserStatusFunction(department, isActive));

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable sensitive data logging in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
