using System;
using System.Threading;
using System.Threading.Tasks;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Common;
using MetGlobalCompany.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetGlobalCompany.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Contractor> Contractors => Set<Contractor>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<NomenclatureCategory> NomenclatureCategories => Set<NomenclatureCategory>();
    public DbSet<Nomenclature> Nomenclatures => Set<Nomenclature>();
    public DbSet<Unit> Units => Set<Unit>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceDetail> SalesInvoiceDetails => Set<SalesInvoiceDetail>();

    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceDetail> PurchaseInvoiceDetails => Set<PurchaseInvoiceDetail>();

    public DbSet<PaymentDocument> Payments => Set<PaymentDocument>();

    public DbSet<InventoryLedger> InventoryLedgers => Set<InventoryLedger>();
    public DbSet<SettlementLedger> SettlementLedgers => Set<SettlementLedger>();

    public DbSet<PriceType> PriceTypes => Set<PriceType>();
    public DbSet<PriceSetting> PriceSettings => Set<PriceSetting>();
    public DbSet<PriceSettingDetail> PriceSettingDetails => Set<PriceSettingDetail>();
    public DbSet<PriceLedger> PriceLedgers => Set<PriceLedger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contractor>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Name).IsRequired().HasMaxLength(250); entity.Property(e => e.FullName).HasMaxLength(500); });
        modelBuilder.Entity<Contract>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Number).IsRequired().HasMaxLength(50); entity.HasOne(d => d.Contractor).WithMany(p => p.Contracts).HasForeignKey(d => d.ContractorId).OnDelete(DeleteBehavior.Cascade); entity.HasOne(d => d.PriceType).WithMany().HasForeignKey(d => d.PriceTypeId).OnDelete(DeleteBehavior.SetNull); });
        modelBuilder.Entity<Unit>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Name).IsRequired().HasMaxLength(100); });
        modelBuilder.Entity<NomenclatureCategory>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Name).IsRequired().HasMaxLength(200); entity.HasOne(d => d.Parent).WithMany(p => p.Children).HasForeignKey(d => d.ParentId).OnDelete(DeleteBehavior.Restrict); });
        modelBuilder.Entity<Nomenclature>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Name).IsRequired().HasMaxLength(300); entity.HasOne(d => d.Category).WithMany(p => p.Nomenclatures).HasForeignKey(d => d.CategoryId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(d => d.Unit).WithMany().HasForeignKey(d => d.UnitId).OnDelete(DeleteBehavior.Restrict); });

        modelBuilder.Entity<Order>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Number).HasMaxLength(50); entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)"); entity.HasOne(d => d.Contractor).WithMany(p => p.Orders).HasForeignKey(d => d.ContractorId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(d => d.Contract).WithMany().HasForeignKey(d => d.ContractId).OnDelete(DeleteBehavior.Restrict); });
        modelBuilder.Entity<OrderDetail>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)"); entity.Property(e => e.Price).HasColumnType("decimal(18,2)"); entity.Property(e => e.Sum).HasColumnType("decimal(18,2)"); entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails).HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.Cascade); entity.HasOne(d => d.Nomenclature).WithMany().HasForeignKey(d => d.NomenclatureId).OnDelete(DeleteBehavior.Restrict); });

        modelBuilder.Entity<SalesInvoice>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Number).HasMaxLength(50); entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)"); entity.HasOne(d => d.Contractor).WithMany().HasForeignKey(d => d.ContractorId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(d => d.Contract).WithMany().HasForeignKey(d => d.ContractId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(d => d.BaseOrder).WithMany().HasForeignKey(d => d.BaseOrderId).OnDelete(DeleteBehavior.SetNull); });
        modelBuilder.Entity<SalesInvoiceDetail>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)"); entity.Property(e => e.Price).HasColumnType("decimal(18,2)"); entity.Property(e => e.Sum).HasColumnType("decimal(18,2)"); entity.HasOne(d => d.SalesInvoice).WithMany(p => p.Details).HasForeignKey(d => d.SalesInvoiceId).OnDelete(DeleteBehavior.Cascade); entity.HasOne(d => d.Nomenclature).WithMany().HasForeignKey(d => d.NomenclatureId).OnDelete(DeleteBehavior.Restrict); });

        modelBuilder.Entity<PurchaseInvoice>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Number).HasMaxLength(50); entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)"); entity.HasOne(d => d.Contractor).WithMany().HasForeignKey(d => d.ContractorId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(d => d.Contract).WithMany().HasForeignKey(d => d.ContractId).OnDelete(DeleteBehavior.Restrict); });
        modelBuilder.Entity<PurchaseInvoiceDetail>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)"); entity.Property(e => e.Price).HasColumnType("decimal(18,2)"); entity.Property(e => e.Sum).HasColumnType("decimal(18,2)"); entity.HasOne(d => d.PurchaseInvoice).WithMany(p => p.Details).HasForeignKey(d => d.PurchaseInvoiceId).OnDelete(DeleteBehavior.Cascade); entity.HasOne(d => d.Nomenclature).WithMany().HasForeignKey(d => d.NomenclatureId).OnDelete(DeleteBehavior.Restrict); });

        modelBuilder.Entity<PaymentDocument>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Number).IsRequired().HasMaxLength(50); entity.Property(e => e.Amount).HasColumnType("decimal(18,2)"); entity.Property(e => e.Purpose).HasMaxLength(1000); entity.HasOne(d => d.Contractor).WithMany().HasForeignKey(d => d.ContractorId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(d => d.Contract).WithMany().HasForeignKey(d => d.ContractId).OnDelete(DeleteBehavior.Restrict); });

        modelBuilder.Entity<InventoryLedger>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.RegistrarName).IsRequired().HasMaxLength(100); entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)"); entity.HasOne(d => d.Nomenclature).WithMany().HasForeignKey(d => d.NomenclatureId).OnDelete(DeleteBehavior.Restrict); });
        modelBuilder.Entity<SettlementLedger>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.RegistrarName).IsRequired().HasMaxLength(100); entity.Property(e => e.Amount).HasColumnType("decimal(18,2)"); entity.HasOne(d => d.Contractor).WithMany().HasForeignKey(d => d.ContractorId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(d => d.Contract).WithMany().HasForeignKey(d => d.ContractId).OnDelete(DeleteBehavior.Restrict); });

        modelBuilder.Entity<PriceType>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Name).IsRequired().HasMaxLength(100); });
        modelBuilder.Entity<PriceSetting>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Number).IsRequired().HasMaxLength(50); });
        modelBuilder.Entity<PriceSettingDetail>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Price).HasColumnType("decimal(18,2)"); entity.HasOne(d => d.PriceSetting).WithMany(p => p.Details).HasForeignKey(d => d.PriceSettingId).OnDelete(DeleteBehavior.Cascade); entity.HasOne(d => d.Nomenclature).WithMany().HasForeignKey(d => d.NomenclatureId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(d => d.PriceType).WithMany().HasForeignKey(d => d.PriceTypeId).OnDelete(DeleteBehavior.Restrict); });
        modelBuilder.Entity<PriceLedger>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Price).HasColumnType("decimal(18,2)"); entity.HasOne(d => d.Nomenclature).WithMany().HasForeignKey(d => d.NomenclatureId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(d => d.PriceType).WithMany().HasForeignKey(d => d.PriceTypeId).OnDelete(DeleteBehavior.Restrict); });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUserService.UserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentUserService.UserId;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}