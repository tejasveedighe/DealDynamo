using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DealDynamo.Models.ProductViewModels;
using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models.UserViewModels;
using DealDynamo.Models;
using System.Reflection.Emit;

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

        builder.Entity<Order>()
            .HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payments>(p => p.OrderId);

        builder.Entity<ProductReview>()
          .HasOne(pr => pr.Product)
          .WithMany(p => p.ProductReviews)
          .HasForeignKey(pr => pr.ProductID);

        builder.Entity<ProductReview>()
            .HasOne(pr => pr.User)
            .WithMany(u => u.ProductReviews)
            .HasForeignKey(pr => pr.UserId);

    }

    public DbSet<DealDynamo.Models.Category>? Category { get; set; }
    public DbSet<DealDynamo.Models.Product>? Product { get; set; }
    public DbSet<DealDynamo.Models.OrderItems>? OrderItems { get; set; }
    public DbSet<DealDynamo.Models.Order>? Orders { get; set; }
    public DbSet<DealDynamo.Models.Payments>? Payments { get; set; }
    public DbSet<DealDynamo.Models.CartItem>? CartItems { get; set; }
    public DbSet<DealDynamo.Models.Address>? Addresses { get; set; }
    public DbSet<DealDynamo.Models.ProductReview>? ProductReviews { get; set; }
}
