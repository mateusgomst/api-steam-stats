using APISTEAMSTATS.models;
using Microsoft.EntityFrameworkCore;

namespace APISTEAMSTATS.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Game> games { get; set; }
        public DbSet<User> users {get; set;}
        
        public DbSet<WishGame> wishlists {get; set;}

    }
}