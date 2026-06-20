using monitoring_management.Models;

namespace monitoring_management.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        SeedAdmin(context);
    }

    private static void SeedAdmin(ApplicationDbContext context)
    {
        if (context.Admins.Any()) return;

        context.Admins.Add(new Admin
        {
            Username = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("admin123")
        });

        context.SaveChanges();
    }
}