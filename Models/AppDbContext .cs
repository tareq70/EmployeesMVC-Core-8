
using Microsoft.EntityFrameworkCore;

namespace EmployeesMVC_Core_8.Models
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        { }

        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<DeviceToken> DeviceTokens { get; set; }

    }
}
