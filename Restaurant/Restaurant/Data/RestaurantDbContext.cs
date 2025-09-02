using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Data;

public partial class RestaurantDbContext : DbContext
{
    public RestaurantDbContext()
    {
    }

    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Allergen> Allergens { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Configuration> Configurations { get; set; }
    public virtual DbSet<Menu> Menus { get; set; }
    public virtual DbSet<MenuItem> MenuItems { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<OrderMenuItem> OrderMenuItems { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductPhoto> ProductPhotos { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public DbSet<LowStockWithStatsDto> LowStockWithStats { get; set; }


    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code.
        => optionsBuilder.UseSqlServer("Server=.;Database=RestaurantDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Allergen>(entity =>
        {
            entity.HasKey(e => e.AllergenId).HasName("PK__Allergen__158B939F7760CCC9");
            entity.ToTable("Allergen");
            entity.HasIndex(e => e.Name, "UQ__Allergen__737584F625AA4EB1").IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0B472EC793");
            entity.ToTable("Category");
            entity.HasIndex(e => e.Name, "UQ__Category__737584F6950E5513").IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PK__Configur__C41E02886D0FD036");
            entity.ToTable("Configuration");
            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(200);
        });
        modelBuilder.Entity<OrderWithDetailsDto>(eb =>
        {
            eb.HasNoKey();
            eb.ToView(null);
        });
        modelBuilder.Entity<LowStockWithStatsDto>(eb =>
        {
            eb.HasNoKey();
            eb.ToView(null);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menu__C99ED23073DDEA58");
            entity.ToTable("Menu");
            entity.HasIndex(e => e.CategoryId, "IX_Menu_CategoryId");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.HasOne(d => d.Category).WithMany(p => p.Menus)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Menu_Category");
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => new { e.MenuId, e.ProductId }).HasName("PK__MenuItem__02DE1E5C064C7188");
            entity.ToTable("MenuItem");
            entity.Property(e => e.QuantityInMenu).HasColumnType("decimal(10, 2)");
            entity.HasOne(d => d.Menu).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.MenuId)

                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuItem_Menu");
            entity.HasOne(d => d.Product).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_MenuItem_Product");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BCF1C7E6832");
            entity.ToTable("Order");
            entity.HasIndex(e => new { e.UserId, e.OrderDate }, "IX_Order_UserId").IsDescending(false, true);
            entity.Property(e => e.DeliveryEta).HasColumnName("DeliveryETA");
            entity.Property(e => e.DeliveryFee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderCode).HasDefaultValueSql("(newid())");
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.Status)
                      .WithMany(p => p.Orders)
                      .HasForeignKey(d => d.StatusId)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK_Order_OrderStatus");


            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_User");
        });
        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId);
            entity.ToTable("OrderStatus");
            entity.Property(e => e.Status)
                  .HasMaxLength(30)
                  .IsRequired();
        });
        

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ProductId }).HasName("PK__OrderIte__08D097A347C96EF6");
            entity.ToTable("OrderItem");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OI_Order");
            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OI_Product");
        });

        modelBuilder.Entity<OrderMenuItem>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.MenuId }).HasName("PK__OrderMen__DF09B6EC758F944F");
            entity.ToTable("OrderMenuItem");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
            entity.HasOne(d => d.Menu).WithMany(p => p.OrderMenuItems)
                .HasForeignKey(d => d.MenuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OMI_Menu");
            entity.HasOne(d => d.Order).WithMany(p => p.OrderMenuItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OMI_Order");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Product__B40CC6CD438B2661");
            entity.ToTable("Product");
            entity.HasIndex(e => e.Name, "IX_Product_Name");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PortionSize).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalQuantity).HasColumnType("decimal(10, 2)");
            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Category");
            entity.HasMany(d => d.Allergens).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductAllergen",
                    r => r.HasOne<Allergen>().WithMany()
                        .HasForeignKey("AllergenId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PA_Allergen"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PA_Product"),
                    j =>
                    {
                        j.HasKey("ProductId", "AllergenId").HasName("PK__ProductA__55547FF4DC32A248");
                        j.ToTable("ProductAllergen");
                        j.HasIndex(new[] { "AllergenId" }, "IX_ProductAllergen_AllergenId");
                    });
        });

        modelBuilder.Entity<ProductPhoto>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__ProductP__21B7B5E2C471ABD9");
            entity.ToTable("ProductPhoto");
            entity.HasIndex(e => e.ProductId, "IX_ProductPhoto_ProductId");
            entity.Property(e => e.PhotoUrl).HasMaxLength(300);
            entity.HasOne(d => d.Product).WithMany(p => p.ProductPhotos)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Photo_Product");
        });


        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4CE0BA9C46");
            entity.ToTable("User");
            entity.HasIndex(e => e.Email, "UQ__User__A9D1053499316085").IsUnique();
            entity.Property(e => e.DeliveryAddress).HasMaxLength(300);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
