using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class OpenLRLocation
{
    [Key]
    public uint Id { get; set; }
    public string Location { get; set; }
    public string Url { get; set; }
    public ulong GotCount { get; set; }
}

public sealed class ApplicationContext : DbContext
{
    public DbSet<OpenLRLocation> Locations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=EPUAKYIW0060;Database=tomtomopenlr-spike;Trusted_Connection=True;");
    }
}
