using APISTEAMSTATS.models;
using Microsoft.EntityFrameworkCore;

namespace APISTEAMSTATS.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<GameList> games { get; set; }
        public DbSet<User> users {get; set;}
        
        public DbSet<WishList> wishlists {get; set;}

    }
}