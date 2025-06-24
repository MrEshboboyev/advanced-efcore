using AdvancedEfCore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEfCore.Api.Data;

// Extension methods for database initialization
public static class DbContextExtensions
{
    public static async Task SeedDataAsync(this ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
            return; // Data already seeded

        // Create functions and procedures
        await CreateDatabaseFunctions(context);

        // Seed initial data
        var users = new[]
        {
                new User { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", CreatedAt = DateTime.UtcNow, IsActive = true, Salary = 75000, Department = "Engineering" },
                new User { FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", CreatedAt = DateTime.UtcNow, IsActive = true, Salary = 85000, Department = "Engineering" },
                new User { FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@example.com", CreatedAt = DateTime.UtcNow, IsActive = true, Salary = 65000, Department = "Sales" },
                new User { FirstName = "Alice", LastName = "Williams", Email = "alice.williams@example.com", CreatedAt = DateTime.UtcNow, IsActive = false, Salary = 70000, Department = "Marketing" }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        var orders = new[]
        {
                new Order { UserId = 1, Amount = 150.50m, OrderDate = DateTime.UtcNow.AddDays(-10), Status = "Completed", Description = "Office supplies" },
                new Order { UserId = 1, Amount = 75.25m, OrderDate = DateTime.UtcNow.AddDays(-5), Status = "Completed", Description = "Software license" },
                new Order { UserId = 2, Amount = 200.00m, OrderDate = DateTime.UtcNow.AddDays(-3), Status = "Pending", Description = "Equipment" },
                new Order { UserId = 3, Amount = 50.00m, OrderDate = DateTime.UtcNow.AddDays(-1), Status = "Completed", Description = "Books" }
        };

        await context.Orders.AddRangeAsync(orders);
        await context.SaveChangesAsync();
    }

    private static async Task CreateDatabaseFunctions(ApplicationDbContext context)
    {
        // Create scalar functions
        var createAgeFunctionSql = """
                CREATE OR REPLACE FUNCTION calculate_user_age(birth_date TIMESTAMP)
                RETURNS INTEGER AS $$
                BEGIN
                    RETURN EXTRACT(YEAR FROM AGE(CURRENT_DATE, birth_date::DATE));
                END;
                $$ LANGUAGE plpgsql IMMUTABLE;
                """;

        var createFullNameFunctionSql = """
                CREATE OR REPLACE FUNCTION get_full_name(first_name VARCHAR, last_name VARCHAR)
                RETURNS VARCHAR AS $$
                BEGIN
                    RETURN CONCAT(first_name, ' ', last_name);
                END;
                $$ LANGUAGE plpgsql IMMUTABLE;
                """;

        // Create table-valued function
        var createTopCustomersFunctionSql = """
                CREATE OR REPLACE FUNCTION get_top_customers(customer_limit INTEGER)
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
                        get_full_name(u."FirstName", u."LastName") as FullName,
                        u."Email",
                        COUNT(o."Id")::INTEGER as OrderCount,
                        COALESCE(SUM(o."Amount"), 0::DECIMAL(18,2)) as TotalSpent,
                        MAX(o."OrderDate") as LastOrderDate
                    FROM "Users" u
                    LEFT JOIN "Orders" o ON u."Id" = o."UserId"
                    WHERE u."IsActive" = true
                    GROUP BY 
                        u."Id",
                        get_full_name(u."FirstName", u."LastName"),  -- << Expression repeat here!
                        u."Email"
                    HAVING COUNT(o."Id") > 0
                    ORDER BY TotalSpent DESC
                    LIMIT customer_limit;
                END;
                $$ LANGUAGE plpgsql;
                """;

        // Create user report stored procedure
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
                        get_full_name(u."FirstName", u."LastName") as FullName,
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

        // Create bulk update stored procedure
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

        // Execute all function creation scripts
        await context.Database.ExecuteSqlRawAsync(createAgeFunctionSql);
        await context.Database.ExecuteSqlRawAsync(createFullNameFunctionSql);
        await context.Database.ExecuteSqlRawAsync(createTopCustomersFunctionSql);
        await context.Database.ExecuteSqlRawAsync(createUserReportProc);
        await context.Database.ExecuteSqlRawAsync(createBulkUpdateProc);
    }
}
