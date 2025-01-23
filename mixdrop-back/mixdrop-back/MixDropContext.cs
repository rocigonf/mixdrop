using Microsoft.EntityFrameworkCore;
using mixdrop_back.Models.Entities;

namespace mixdrop_back;

public class MixDropContext : DbContext
{
    private const string DATABASE_PATH = "mixdrop.db";
    
    public DbSet<Artist> Artists { get; set; }
    public DbSet<Battle> Battles { get; set; }
    public DbSet<BattleCart> BattlesCarts { get; set; }
    public DbSet<BattleResult> BattleResults { get; set; }
    public DbSet<BattleRole> BattleRoles { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartType> CartTypes { get; set; }
    public DbSet<Collection> Collections { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<State> States { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserBattle> UsersBattles { get; set; }
    //public DbSet<UserFriend> UsersFriends { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        optionsBuilder.UseSqlite($"DataSource={baseDir}{DATABASE_PATH}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Friendship>()
            .HasKey(f => new { f.User1Id, f.User2Id });

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.User1)
            .WithMany(u => u.Friendships)
            .HasForeignKey(f => f.User1Id);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.User2)
            .WithMany()
            .HasForeignKey(a => a.User2Id);
    }
}
