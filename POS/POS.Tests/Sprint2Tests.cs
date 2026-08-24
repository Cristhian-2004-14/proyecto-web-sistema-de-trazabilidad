using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using POS.Server.Controllers;
using POS.Server.Data;
using POS.Server.Services;
using POS.Shared.DTOs;
using POS.Shared.Entities;
using Xunit;

namespace POS.Tests;

public class Sprint2Tests
{
    [Fact]
    public async Task ValidReception_IsRegistered()
    {
        await using var fixture = await DbFixture.CreateAsync();
        var result = await fixture.Service.CreateAsync(Request(10), fixture.User.Id);
        Assert.True(result.Id > 0); Assert.Single(result.Details); Assert.Equal(ReceptionStatuses.Confirmed, result.Status);
        Assert.Equal(1, await fixture.Db.Receptions.CountAsync());
    }

    [Fact]
    public async Task ConfirmedReception_IncreasesRawMaterialStock()
    {
        await using var fixture = await DbFixture.CreateAsync();
        await fixture.Service.CreateAsync(Request(12.5m), fixture.User.Id);
        await fixture.Db.Entry(fixture.Material).ReloadAsync();
        Assert.Equal(112.5m, fixture.Material.CurrentStock);
    }

    [Fact]
    public async Task Reception_WithNonPositiveQuantity_IsRejected()
    {
        await using var fixture = await DbFixture.CreateAsync();
        await Assert.ThrowsAsync<BusinessValidationException>(() => fixture.Service.CreateAsync(Request(0), fixture.User.Id));
        Assert.Empty(await fixture.Db.Receptions.ToListAsync());
    }

    [Fact]
    public async Task Reception_WithInactiveSupplier_IsRejected()
    {
        await using var fixture = await DbFixture.CreateAsync(); fixture.Supplier.IsActive=false; await fixture.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessValidationException>(() => fixture.Service.CreateAsync(Request(5), fixture.User.Id));
    }

    [Fact]
    public async Task Reception_WithInactiveRawMaterial_IsRejected()
    {
        await using var fixture = await DbFixture.CreateAsync(); fixture.Material.IsActive=false; await fixture.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessValidationException>(() => fixture.Service.CreateAsync(Request(5), fixture.User.Id));
    }

    [Fact]
    public async Task FailureDuringStockUpdate_RollsBackReceptionAndDetails()
    {
        await using var fixture = await DbFixture.CreateAsync(failing:true);
        fixture.FailingDb!.FailOnSecondSave();
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CreateAsync(Request(8), fixture.User.Id));
        fixture.Db.ChangeTracker.Clear();
        Assert.Equal(0, await fixture.Db.Receptions.CountAsync());
        Assert.Equal(0, await fixture.Db.ReceptionDetails.CountAsync());
        Assert.Equal(100m, (await fixture.Db.RawMaterials.SingleAsync()).CurrentStock);
    }

    [Fact]
    public async Task ConfirmingSameReceptionTwice_DoesNotIncreaseStockTwice()
    {
        await using var fixture = await DbFixture.CreateAsync();
        var pending=Request(7); pending.Status=ReceptionStatuses.Pending;
        var created=await fixture.Service.CreateAsync(pending,fixture.User.Id);
        await fixture.Service.ConfirmAsync(created.Id,fixture.User.Id);
        await fixture.Service.ConfirmAsync(created.Id,fixture.User.Id);
        fixture.Db.ChangeTracker.Clear();
        Assert.Equal(107m,(await fixture.Db.RawMaterials.SingleAsync()).CurrentStock);
    }

    [Fact]
    public async Task ValidProduct_IsCreated()
    {
        await using var fixture=await DbFixture.CreateAsync();
        var controller=new ProductsController(fixture.Db);
        var result=await controller.Post(new ProductRequest{Name="Palmito entero",UnitOfMeasure="unidad",CurrentStock=0,MinimumStock=100,IsActive=true});
        Assert.IsType<CreatedAtActionResult>(result.Result); Assert.Equal(1,await fixture.Db.InventoryProducts.CountAsync());
    }

    [Fact]
    public async Task Product_WithNegativeStock_IsRejected()
    {
        await using var fixture=await DbFixture.CreateAsync();
        var result=await new ProductsController(fixture.Db).Post(new ProductRequest{Name="Inválido",UnitOfMeasure="unidad",CurrentStock=-1});
        Assert.IsType<BadRequestObjectResult>(result.Result); Assert.Empty(await fixture.Db.InventoryProducts.ToListAsync());
    }

    [Fact]
    public void ReceptionPost_DoesNotAuthorizeProductionOrManagementRoles()
    {
        var method=typeof(ReceptionsController).GetMethod(nameof(ReceptionsController.Post))!;
        var roles=method.GetCustomAttributes(typeof(AuthorizeAttribute),true).Cast<AuthorizeAttribute>().Single().Roles!;
        Assert.Contains("Administrador",roles); Assert.Contains("Almacén",roles);
        Assert.DoesNotContain("Producción",roles); Assert.DoesNotContain("Gerencia",roles);
    }

    private static CreateReceptionRequest Request(decimal quantity) => new()
    {
        SupplierId=1, Date=DateTime.Today, Status=ReceptionStatuses.Confirmed,
        Details=[new ReceptionDetailRequest{RawMaterialId=1,Quantity=quantity}]
    };

    private sealed class DbFixture : IAsyncDisposable
    {
        public required SqliteConnection Connection {get;init;}
        public required AppDbContext Db {get;init;}
        public required ReceptionService Service {get;init;}
        public required User User {get;init;}
        public required Supplier Supplier {get;init;}
        public required RawMaterial Material {get;init;}
        public FailingAppDbContext? FailingDb => Db as FailingAppDbContext;

        public static async Task<DbFixture> CreateAsync(bool failing=false)
        {
            var connection=new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
            var options=new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
            AppDbContext db=failing?new FailingAppDbContext(options):new AppDbContext(options);
            await db.Database.EnsureCreatedAsync();
            var role=new RoleEntity{Id=1,Name="Almacén",IsActive=true};
            var user=new User{Id=1,Name="María",LastName="Suárez",Username="almacen",Email="almacen@test.bo",PasswordHash="hash",Role=role,RoleId=1,IsActive=true};
            var supplier=new Supplier{Id=1,Name="Proveedor activo",IsActive=true};
            var material=new RawMaterial{Id=1,Name="Fruto de palma",UnitOfMeasure="kg",CurrentStock=100,MinimumStock=20,IsActive=true};
            db.AddRange(role,user,supplier,material); await db.SaveChangesAsync();
            return new DbFixture{Connection=connection,Db=db,Service=new ReceptionService(db),User=user,Supplier=supplier,Material=material};
        }
        public async ValueTask DisposeAsync(){await Db.DisposeAsync();await Connection.DisposeAsync();}
    }

    private sealed class FailingAppDbContext(DbContextOptions<AppDbContext> options):AppDbContext(options)
    {
        private int saves; private bool enabled;
        public void FailOnSecondSave(){saves=0;enabled=true;}
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken=default)
        {
            if(enabled && ++saves==2) throw new InvalidOperationException("Fallo simulado al actualizar stock.");
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
