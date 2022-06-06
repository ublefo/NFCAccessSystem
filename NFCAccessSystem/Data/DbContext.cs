using Microsoft.EntityFrameworkCore;

namespace NFCAccessSystem.Data;

// TagUid needs to be unique
[Index(nameof(TagUid), IsUnique = true)]
public class User
{
    public int UserId { get; set; }
    public int TagUid { get; set; }
    public string? Name { get; set; }
    public string TotpSecret { get; set; }
    public bool Authorized { get; set; }
    public bool Admin { get; set; }
    public bool OfflineAuth { get; set; }
}

public class AccessSystemContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public string DbPath { get; }

    public AccessSystemContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "acs.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    public DbSet<NFCAccessSystem.Models.ErrorViewModel>? ErrorViewModel { get; set; }
}