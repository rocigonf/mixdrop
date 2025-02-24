using Microsoft.EntityFrameworkCore;
using mixdrop_back.Models.Entities;

namespace mixdrop_back;

public class MixDropContext : DbContext
{
    private const string DATABASE_PATH = "mixdrop.db";

    private readonly Settings _settings;
    public MixDropContext(Settings settings)
    {
        _settings = settings;
    }


    public DbSet<Artist> Artists { get; set; }
    public DbSet<Battle> Battles { get; set; }
    public DbSet<BattleState> BattleStates { get; set; }
    public DbSet<BattleResult> BattleResults { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<CardType> CardTypes { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<State> States { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserBattle> UsersBattles { get; set; }
    public DbSet<Tone> Tones { get; set; }
    //public DbSet<UserFriend> UsersFriends { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
# if DEBUG
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        optionsBuilder.UseSqlite($"DataSource={baseDir}{DATABASE_PATH}");
#else

            optionsBuilder.UseMySql(_settings.DatabaseConnection, ServerVersion.AutoDetect(_settings.DatabaseConnection));
#endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.SenderUser)
            .WithMany(u => u.Friendships)
            .HasForeignKey(f => f.SenderUserId);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.ReceiverUser)
            .WithMany()
            .HasForeignKey(f => f.ReceiverUserId);
    }
}
