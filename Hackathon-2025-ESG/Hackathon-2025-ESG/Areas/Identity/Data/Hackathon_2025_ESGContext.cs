using Hackathon_2025_ESG.Areas.Identity.Data;
using Hackathon_2025_ESG.Models;
using Hackathon_2025_ESG.Models.ERPSample;
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
    public DbSet<Company> Company { get; set; }
    public DbSet<Department> Department { get; set; }
    public DbSet<Employee> Employee { get; set; }
    public DbSet<Vendor> Vendor { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<OrderDetail> OrderDetail { get; set; }
    public DbSet<EnvironmentalMetric> EnvironmentalMetric { get; set; }
    public DbSet<Compliance> Compliance { get; set; }
}
