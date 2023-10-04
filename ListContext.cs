using Microsoft.EntityFrameworkCore;

/// <summary>
/// Class <c>ListContext</c> Entity framework Model
/// Holds "tables" of History, Favorites, and Options
/// </summary>
public class ListContext : DbContext
{
    // Init sets
    public DbSet<Url> History { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Option> Options { get; set; }

    // Init DB path
    public string DbPath { get; }

    public ListContext()
    {
        this.DbPath = "browser.db";
    }

    public ListContext(string path)
    {
        this.DbPath = path + ".db";
    }

    // Init sqlite db, gets called on creation
    /// <summary>
    /// Init sqlite db, gets called on creation
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}