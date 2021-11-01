using LocStatsBackendAPI.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LocStatsBackendAPI.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        // public virtual DbSet<Model> Table { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<GpsCoordinate> GpsCoordinates { get; set; } 

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
