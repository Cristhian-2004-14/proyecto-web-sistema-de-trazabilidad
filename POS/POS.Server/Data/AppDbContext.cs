using Microsoft.EntityFrameworkCore;
using POS.Shared.Entities;

namespace POS.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<RawMaterial> RawMaterials => Set<RawMaterial>();
    public DbSet<Reception> Receptions => Set<Reception>();
    public DbSet<ReceptionDetail> ReceptionDetails => Set<ReceptionDetail>();
    public DbSet<InventoryProduct> InventoryProducts => Set<InventoryProduct>();
    public DbSet<ProductRecipeItem> ProductRecipeItems => Set<ProductRecipeItem>();
    public DbSet<ProductionLot> ProductionLots => Set<ProductionLot>();
    public DbSet<ProductionLotMaterialDetail> ProductionLotMaterialDetails => Set<ProductionLotMaterialDetail>();
    public DbSet<ProductionLotMaterialOrigin> ProductionLotMaterialOrigins => Set<ProductionLotMaterialOrigin>();
    public DbSet<Dispatch> Dispatches => Set<Dispatch>();
    public DbSet<DispatchDetail> DispatchDetails => Set<DispatchDetail>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

    // Entidades heredadas del proyecto base, reservadas para los siguientes Sprints.
    public DbSet<CashClosing> CashClosings => Set<CashClosing>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();
    public DbSet<Stock> StockMovements => Set<Stock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RoleEntity>(entity =>
        {
            entity.ToTable("ROL");
            entity.HasIndex(x => x.Name).IsUnique();
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USUARIO");
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Supplier>(entity => entity.ToTable("PROVEEDOR"));
        modelBuilder.Entity<RawMaterial>(entity =>
        {
            entity.ToTable("MATERIA_PRIMA");
            entity.Property(x => x.CurrentStock).HasColumnType("decimal(18,2)");
            entity.Property(x => x.MinimumStock).HasColumnType("decimal(18,2)");
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_MATERIA_PRIMA_StockActual", "[CurrentStock] >= 0");
                t.HasCheckConstraint("CK_MATERIA_PRIMA_StockMinimo", "[MinimumStock] >= 0");
            });
        });

        modelBuilder.Entity<Reception>(entity =>
        {
            entity.ToTable("RECEPCION");
            entity.Property(x => x.Date).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Supplier).WithMany(x => x.Receptions).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.User).WithMany(x => x.Receptions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ReceptionDetail>(entity =>
        {
            entity.ToTable("DETALLE_RECEPCION");
            entity.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
            entity.HasIndex(x => new { x.ReceptionId, x.RawMaterialId }).IsUnique();
            entity.HasOne(x => x.Reception).WithMany(x => x.Details).HasForeignKey(x => x.ReceptionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.RawMaterial).WithMany(x => x.ReceptionDetails).HasForeignKey(x => x.RawMaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t => t.HasCheckConstraint("CK_DETALLE_RECEPCION_Cantidad", "[Quantity] > 0"));
        });

        modelBuilder.Entity<InventoryProduct>(entity =>
        {
            entity.ToTable("PRODUCTO");
            entity.Property(e => e.Name).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(30).IsRequired();
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PRODUCTO_StockActual", "[CurrentStock] >= 0");
                t.HasCheckConstraint("CK_PRODUCTO_StockMinimo", "[MinimumStock] >= 0");
            });
        });
        modelBuilder.Entity<ProductRecipeItem>(entity =>
        {
            entity.ToTable("RECETA_PRODUCTO");
            entity.Property(x => x.QuantityPerUnit).HasColumnType("decimal(18,4)");
            entity.HasIndex(x => new { x.ProductId, x.RawMaterialId }).IsUnique();
            entity.HasOne(x => x.Product).WithMany(x => x.Recipe).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.RawMaterial).WithMany(x => x.ProductRecipes).HasForeignKey(x => x.RawMaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t => t.HasCheckConstraint("CK_RECETA_PRODUCTO_Cantidad", "[QuantityPerUnit] > 0"));
        });
        modelBuilder.Entity<ProductionLot>(entity =>
        {
            entity.ToTable("LOTE_PRODUCCION");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Product).WithMany(x => x.ProductionLots).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.User).WithMany(x => x.ProductionLots).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_LOTE_PRODUCCION_CantidadPlanificada", "[PlannedQuantity] > 0");
                t.HasCheckConstraint("CK_LOTE_PRODUCCION_CantidadProducida", "[ProducedQuantity] >= 0");
            });
        });
        modelBuilder.Entity<ProductionLotMaterialDetail>(entity =>
        {
            entity.ToTable("DETALLE_LOTE_MATERIA_PRIMA");
            entity.Property(x => x.QuantityUsed).HasColumnType("decimal(18,2)");
            entity.HasIndex(x => new { x.ProductionLotId, x.RawMaterialId }).IsUnique();
            entity.HasOne(x => x.ProductionLot).WithMany(x => x.Details).HasForeignKey(x => x.ProductionLotId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.RawMaterial).WithMany(x => x.ProductionLotDetails).HasForeignKey(x => x.RawMaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t => t.HasCheckConstraint("CK_DETALLE_LOTE_MP_Cantidad", "[QuantityUsed] > 0"));
        });
        modelBuilder.Entity<ProductionLotMaterialOrigin>(entity =>
        {
            entity.ToTable("ORIGEN_MATERIA_PRIMA_LOTE");
            entity.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
            entity.HasIndex(x => new { x.ProductionLotMaterialDetailId, x.ReceptionDetailId }).IsUnique();
            entity.HasOne(x => x.ProductionLotMaterialDetail).WithMany(x => x.Origins).HasForeignKey(x => x.ProductionLotMaterialDetailId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ReceptionDetail).WithMany(x => x.ProductionOrigins).HasForeignKey(x => x.ReceptionDetailId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t => t.HasCheckConstraint("CK_ORIGEN_MATERIA_PRIMA_LOTE_Cantidad", "[Quantity] > 0"));
        });
        modelBuilder.Entity<Dispatch>(entity =>
        {
            entity.ToTable("DESPACHO"); entity.Property(x=>x.Destination).HasMaxLength(150).IsRequired(); entity.Property(x=>x.Observation).HasMaxLength(300); entity.Property(x=>x.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(x=>x.User).WithMany(x=>x.Dispatches).HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<DispatchDetail>(entity =>
        {
            entity.ToTable("DETALLE_DESPACHO"); entity.HasIndex(x=>new{x.DispatchId,x.ProductId}).IsUnique();
            entity.HasOne(x=>x.Dispatch).WithMany(x=>x.Details).HasForeignKey(x=>x.DispatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x=>x.Product).WithMany(x=>x.DispatchDetails).HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x=>x.ProductionLot).WithMany(x=>x.DispatchDetails).HasForeignKey(x=>x.ProductionLotId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t=>t.HasCheckConstraint("CK_DETALLE_DESPACHO_Cantidad","[Quantity] > 0"));
        });
        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.ToTable("MOVIMIENTO_INVENTARIO"); entity.Property(x=>x.MovementType).HasMaxLength(30).IsRequired(); entity.Property(x=>x.Reference).HasMaxLength(100); entity.Property(x=>x.Quantity).HasColumnType("decimal(18,2)");
            entity.HasOne(x=>x.Product).WithMany(x=>x.InventoryMovements).HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x=>x.User).WithMany(x=>x.InventoryMovements).HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t=>t.HasCheckConstraint("CK_MOVIMIENTO_INVENTARIO_Cantidad","[Quantity] > 0"));
        });
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });
        modelBuilder.Entity<Sale>(entity => entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)"));
        modelBuilder.Entity<SaleDetail>(entity =>
        {
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(d => d.Sale).WithMany(s => s.SaleDetails).HasForeignKey(d => d.SaleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CashClosing>(entity =>
        {
            entity.Property(e => e.InitialBalance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalCash).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalCard).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalTransfer).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAdjustments).HasColumnType("decimal(18,2)");
            entity.Property(e => e.FinalBalance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.FinalCash).HasColumnType("decimal(18,2)");
        });
    }
}
