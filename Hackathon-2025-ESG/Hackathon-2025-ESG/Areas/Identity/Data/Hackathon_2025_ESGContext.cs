using Hackathon_2025_ESG.Areas.Identity.Data;
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
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
