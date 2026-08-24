using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using POS.Server.Controllers;
using POS.Server.Data;
using POS.Server.Services;
using POS.Shared.DTOs;
using POS.Shared.Entities;
using Xunit;

namespace POS.Tests;

public class Sprint3Tests
{
    [Fact] public async Task ValidLot_IsCreated(){await using var f=await Fixture.Create();var x=await f.Service.CreateAsync(NewLot(),f.User.Id);Assert.True(x.Id>0);Assert.Equal(ProductionLotStatuses.Pending,x.Status);}
    [Fact] public async Task DuplicateCode_IsRejected(){await using var f=await Fixture.Create();await f.Service.CreateAsync(NewLot(),1);await Assert.ThrowsAsync<BusinessValidationException>(()=>f.Service.CreateAsync(NewLot(),1));}
    [Fact] public async Task NonPositivePlannedQuantity_IsRejected(){await using var f=await Fixture.Create();var r=NewLot();r.PlannedQuantity=0;await Assert.ThrowsAsync<BusinessValidationException>(()=>f.Service.CreateAsync(r,1));}
    [Fact] public async Task InactiveProduct_IsRejected(){await using var f=await Fixture.Create();f.Product.IsActive=false;await f.Db.SaveChangesAsync();await Assert.ThrowsAsync<BusinessValidationException>(()=>f.Service.CreateAsync(NewLot(),1));}
    [Fact] public async Task ValidConsumption_IsRegistered(){await using var f=await Fixture.Create();var lot=await f.CreateLot();var x=await f.Service.ConsumeAsync(lot.Id,Consume(10),1);Assert.Single(x!.Materials);Assert.Equal(ProductionLotStatuses.InProgress,x.Status);}
    [Fact] public async Task Consumption_DecreasesStock(){await using var f=await Fixture.Create();var lot=await f.CreateLot();await f.Service.ConsumeAsync(lot.Id,Consume(12),1);f.Db.ChangeTracker.Clear();Assert.Equal(88,(await f.Db.RawMaterials.SingleAsync()).CurrentStock);}
    [Fact] public async Task ConsumptionAboveStock_IsRejected(){await using var f=await Fixture.Create();var lot=await f.CreateLot();await Assert.ThrowsAsync<BusinessValidationException>(()=>f.Service.ConsumeAsync(lot.Id,Consume(101),1));}
    [Fact] public async Task InactiveMaterial_IsRejected(){await using var f=await Fixture.Create();var lot=await f.CreateLot();f.Material.IsActive=false;await f.Db.SaveChangesAsync();await Assert.ThrowsAsync<BusinessValidationException>(()=>f.Service.ConsumeAsync(lot.Id,Consume(5),1));}
    [Fact] public async Task ConsumptionFailure_RollsBackEverything(){await using var f=await Fixture.Create(true);var lot=await f.CreateLot();f.Failing!.FailOnSecondSave();await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.ConsumeAsync(lot.Id,Consume(10),1));f.Db.ChangeTracker.Clear();Assert.Empty(await f.Db.ProductionLotMaterialDetails.ToListAsync());Assert.Equal(100,(await f.Db.RawMaterials.SingleAsync()).CurrentStock);Assert.Equal(ProductionLotStatuses.Pending,(await f.Db.ProductionLots.SingleAsync()).Status);}
    [Fact] public async Task RepeatedConsumption_DoesNotDiscountTwice(){await using var f=await Fixture.Create();var lot=await f.CreateLot();await f.Service.ConsumeAsync(lot.Id,Consume(10),1);await f.Service.ConsumeAsync(lot.Id,Consume(10),1);f.Db.ChangeTracker.Clear();Assert.Equal(90,(await f.Db.RawMaterials.SingleAsync()).CurrentStock);Assert.Single(await f.Db.ProductionLotMaterialDetails.ToListAsync());}
    [Fact] public async Task Lot_IsFinished(){await using var f=await Fixture.Create();var lot=await f.ConsumedLot();var x=await f.Service.FinishAsync(lot.Id,new(){ProducedQuantity=80},1);Assert.Equal(ProductionLotStatuses.Finished,x!.Status);Assert.NotNull(x.EndDate);Assert.Equal(80,x.ProducedQuantity);}
    [Fact] public async Task Finishing_IncreasesProductStock(){await using var f=await Fixture.Create();var lot=await f.ConsumedLot();await f.Service.FinishAsync(lot.Id,new(){ProducedQuantity=80},1);f.Db.ChangeTracker.Clear();Assert.Equal(90,(await f.Db.InventoryProducts.SingleAsync()).CurrentStock);}
    [Fact] public async Task RepeatedFinish_DoesNotIncreaseTwice(){await using var f=await Fixture.Create();var lot=await f.ConsumedLot();await f.Service.FinishAsync(lot.Id,new(){ProducedQuantity=80},1);await f.Service.FinishAsync(lot.Id,new(){ProducedQuantity=80},1);f.Db.ChangeTracker.Clear();Assert.Equal(90,(await f.Db.InventoryProducts.SingleAsync()).CurrentStock);}
    [Fact] public async Task FinishFailure_RollsBackEverything(){await using var f=await Fixture.Create(true);var lot=await f.ConsumedLot();f.Failing!.FailOnSecondSave();await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.FinishAsync(lot.Id,new(){ProducedQuantity=80},1));f.Db.ChangeTracker.Clear();var saved=await f.Db.ProductionLots.SingleAsync();Assert.Equal(ProductionLotStatuses.InProgress,saved.Status);Assert.Equal(0,saved.ProducedQuantity);Assert.Equal(10,(await f.Db.InventoryProducts.SingleAsync()).CurrentStock);}
    [Fact] public void Warehouse_IsNotAuthorizedToModifyProduction(){AssertModifyRolesExclude("Almacén");}
    [Fact] public void Management_IsNotAuthorizedToModifyProduction(){AssertModifyRolesExclude("Gerencia");}

    private static void AssertModifyRolesExclude(string role){foreach(var name in new[]{nameof(ProductionLotsController.Post),nameof(ProductionLotsController.Consume),nameof(ProductionLotsController.Finish)}){var method=typeof(ProductionLotsController).GetMethod(name)!;var roles=method.GetCustomAttributes(typeof(AuthorizeAttribute),true).Cast<AuthorizeAttribute>().Single().Roles!;Assert.Contains("Administrador",roles);Assert.Contains("Producción",roles);Assert.DoesNotContain(role,roles);}}
    private static CreateProductionLotRequest NewLot()=>new(){ProductId=1,Code="LP-2026-001",StartDate=DateTime.Today,PlannedQuantity=100};
    private static MaterialConsumptionRequest Consume(decimal quantity)=>new(){Items=[new(){RawMaterialId=1,Quantity=quantity}]};

    private sealed class Fixture:IAsyncDisposable
    {
        public required SqliteConnection Connection{get;init;} public required AppDbContext Db{get;init;} public required ProductionLotService Service{get;init;}
        public required User User{get;init;} public required RawMaterial Material{get;init;} public required InventoryProduct Product{get;init;}
        public FailingContext? Failing=>Db as FailingContext;
        public static async Task<Fixture>Create(bool failing=false){var c=new SqliteConnection("Data Source=:memory:");await c.OpenAsync();var o=new DbContextOptionsBuilder<AppDbContext>().UseSqlite(c).Options;AppDbContext db=failing?new FailingContext(o):new AppDbContext(o);await db.Database.EnsureCreatedAsync();var role=new RoleEntity{Id=1,Name="Producción",IsActive=true};var user=new User{Id=1,Name="Ana",LastName="Paz",Username="produccion",Email="p@test.bo",PasswordHash="hash",Role=role,RoleId=1,IsActive=true};var material=new RawMaterial{Id=1,Name="Palmito fresco",UnitOfMeasure="kg",CurrentStock=100,MinimumStock=10,IsActive=true};var product=new InventoryProduct{Id=1,Name="Palmito entero",UnitOfMeasure="unidad",CurrentStock=10,MinimumStock=5,IsActive=true};db.AddRange(role,user,material,product);await db.SaveChangesAsync();return new(){Connection=c,Db=db,Service=new(db),User=user,Material=material,Product=product};}
        public Task<ProductionLotResponse>CreateLot()=>Service.CreateAsync(NewLot(),User.Id);
        public async Task<ProductionLotResponse>ConsumedLot(){var lot=await CreateLot();return (await Service.ConsumeAsync(lot.Id,Consume(10),User.Id))!;}
        public async ValueTask DisposeAsync(){await Db.DisposeAsync();await Connection.DisposeAsync();}
    }
    private sealed class FailingContext(DbContextOptions<AppDbContext> options):AppDbContext(options)
    {private int saves;private bool enabled;public void FailOnSecondSave(){saves=0;enabled=true;}public override Task<int>SaveChangesAsync(CancellationToken token=default){if(enabled&&++saves==2)throw new InvalidOperationException("Fallo simulado.");return base.SaveChangesAsync(token);}}
}
