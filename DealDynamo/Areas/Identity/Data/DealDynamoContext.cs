using DealDynamo.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DealDynamo.Models;
using DealDynamo.Models.ProductViewModels;

namespace DealDynamo.Data;

public class DealDynamoContext : IdentityDbContext<ApplicationUser>
{
    public DealDynamoContext(DbContextOptions<DealDynamoContext> options)
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

    public DbSet<DealDynamo.Models.Category>? Category { get; set; }
    public DbSet<DealDynamo.Models.Product>? Product { get; set; }
    public DbSet<DealDynamo.Models.ProductViewModels.ViewDetailProductViewModel>? ViewDetailProductViewModel { get; set; }
}
