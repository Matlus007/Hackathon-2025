using Hackathon_2025_ESG.Areas.Identity.Data;
using Hackathon_2025_ESG.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hackathon_2025_ESG.Data;

public class Hackathon_2025_ESGContext : IdentityDbContext<Hackathon_2025_ESGUser>
{
    public Hackathon_2025_ESGContext(DbContextOptions<Hackathon_2025_ESGContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }

    public DbSet<EsgRawDocs> EsgRawDoc { get; set; }
    public DbSet<EsgReport> EsgReport { get; set; }
}
