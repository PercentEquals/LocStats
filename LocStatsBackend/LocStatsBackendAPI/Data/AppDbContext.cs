using LocStatsBackendAPI.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LocStatsBackendAPI.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        // public virtual DbSet<Model> Table { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
