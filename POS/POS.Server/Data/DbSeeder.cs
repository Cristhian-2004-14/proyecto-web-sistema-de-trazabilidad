using Microsoft.EntityFrameworkCore;
using POS.Shared.Entities;

namespace POS.Server.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();
        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new RoleEntity { Name = "Administrador", Description = "Administración completa del sistema", IsActive = true },
                new RoleEntity { Name = "Producción", Description = "Gestión de lotes y producción", IsActive = true },
                new RoleEntity { Name = "Almacén", Description = "Recepciones, inventario y despachos", IsActive = true },
                new RoleEntity { Name = "Gerencia", Description = "Consultas y reportes", IsActive = true });
            await db.SaveChangesAsync();
        }
        if (!await db.Users.AnyAsync())
        {
            var roles = await db.Roles.ToDictionaryAsync(x => x.Name, x => x.Id);
            foreach (var (name, lastName, username, email, role) in new[]
            {
                ("Ana", "Rojas", "admin", "admin@palmaverde.bo", "Administrador"),
                ("Pedro", "Mendoza", "produccion", "produccion@palmaverde.bo", "Producción"),
                ("María", "Suárez", "almacen", "almacen@palmaverde.bo", "Almacén"),
                ("Carla", "Vargas", "gerencia", "gerencia@palmaverde.bo", "Gerencia")
            }) db.Users.Add(new User { Name = name, LastName = lastName, Username = username, Email = email, RoleId = roles[role], IsActive = true, PasswordHash = BCrypt.Net.BCrypt.HashPassword("PalmaVerde2026!") });
        }
        if (!await db.Suppliers.AnyAsync()) db.Suppliers.AddRange(
            new Supplier { Name = "Oleaginosas del Norte", Nit = "1029384011", Phone = "33445566", Address = "Santa Cruz" },
            new Supplier { Name = "Insumos Tropicales", Nit = "2048576018", Phone = "33778899", Address = "Warnes" });
        if (!await db.RawMaterials.AnyAsync()) db.RawMaterials.AddRange(
            new RawMaterial { Name = "Fruto de palma", UnitOfMeasure = "kg", CurrentStock = 850, MinimumStock = 250 },
            new RawMaterial { Name = "Envase 1 litro", UnitOfMeasure = "unidad", CurrentStock = 600, MinimumStock = 150 },
            new RawMaterial { Name = "Etiqueta Palma Verde", UnitOfMeasure = "unidad", CurrentStock = 600, MinimumStock = 150 });
        if (!await db.InventoryProducts.AnyAsync()) db.InventoryProducts.Add(
            new InventoryProduct { Name = "Palmito entero", Description = "Producto terminado demo para Sprint 2", UnitOfMeasure = "unidad", CurrentStock = 0, MinimumStock = 100, IsActive = true });
        await db.SaveChangesAsync();
        if (!await db.ProductRecipeItems.AnyAsync())
        {
            var product = await db.InventoryProducts.OrderBy(x => x.Id).FirstAsync();
            var materials = await db.RawMaterials.OrderBy(x => x.Id).ToListAsync();
            if (materials.Count > 0) db.ProductRecipeItems.Add(new ProductRecipeItem { ProductId=product.Id, RawMaterialId=materials[0].Id, QuantityPerUnit=1.25m });
            if (materials.Count > 1) db.ProductRecipeItems.Add(new ProductRecipeItem { ProductId=product.Id, RawMaterialId=materials[1].Id, QuantityPerUnit=1m });
            if (materials.Count > 2) db.ProductRecipeItems.Add(new ProductRecipeItem { ProductId=product.Id, RawMaterialId=materials[2].Id, QuantityPerUnit=1m });
            await db.SaveChangesAsync();
        }

        await SeedExpandedDemoCatalogAsync(db);
    }

    private static async Task SeedExpandedDemoCatalogAsync(AppDbContext db)
    {
        var desiredMaterials = new[]
        {
            new RawMaterial { Name = "Envase 500 ml", UnitOfMeasure = "unidad", CurrentStock = 900, MinimumStock = 200 },
            new RawMaterial { Name = "Etiqueta 500 ml", UnitOfMeasure = "unidad", CurrentStock = 900, MinimumStock = 200 },
            new RawMaterial { Name = "Soda cáustica", UnitOfMeasure = "kg", CurrentStock = 300, MinimumStock = 60 },
            new RawMaterial { Name = "Esencia natural", UnitOfMeasure = "litro", CurrentStock = 180, MinimumStock = 30 },
            new RawMaterial { Name = "Empaque para jabón", UnitOfMeasure = "unidad", CurrentStock = 1200, MinimumStock = 250 },
            new RawMaterial { Name = "Antioxidante alimentario", UnitOfMeasure = "kg", CurrentStock = 120, MinimumStock = 20 }
        };

        var existingMaterialNames = await db.RawMaterials.Select(x => x.Name).ToHashSetAsync();
        db.RawMaterials.AddRange(desiredMaterials.Where(x => !existingMaterialNames.Contains(x.Name)));
        await db.SaveChangesAsync();

        var desiredProducts = new[]
        {
            new InventoryProduct { Name = "Aceite de palma 1 litro", Description = "Aceite refinado en presentación familiar", UnitOfMeasure = "unidad", MinimumStock = 100 },
            new InventoryProduct { Name = "Aceite de palma 500 ml", Description = "Aceite refinado en presentación mediana", UnitOfMeasure = "unidad", MinimumStock = 150 },
            new InventoryProduct { Name = "Jabón vegetal de palma", Description = "Jabón artesanal elaborado con derivados de palma", UnitOfMeasure = "unidad", MinimumStock = 120 },
            new InventoryProduct { Name = "Manteca vegetal 500 g", Description = "Producto alimentario procesado a base de palma", UnitOfMeasure = "unidad", MinimumStock = 80 }
        };

        var existingProductNames = await db.InventoryProducts.Select(x => x.Name).ToHashSetAsync();
        db.InventoryProducts.AddRange(desiredProducts.Where(x => !existingProductNames.Contains(x.Name)));
        await db.SaveChangesAsync();

        var materials = await db.RawMaterials.ToDictionaryAsync(x => x.Name);
        var products = await db.InventoryProducts.Include(x => x.Recipe).ToDictionaryAsync(x => x.Name);
        var recipes = new Dictionary<string, (string Material, decimal Quantity)[]>
        {
            ["Aceite de palma 1 litro"] =
            [
                ("Fruto de palma", 1.30m), ("Envase 1 litro", 1m),
                ("Etiqueta Palma Verde", 1m), ("Antioxidante alimentario", 0.01m)
            ],
            ["Aceite de palma 500 ml"] =
            [
                ("Fruto de palma", 0.65m), ("Envase 500 ml", 1m),
                ("Etiqueta 500 ml", 1m), ("Antioxidante alimentario", 0.005m)
            ],
            ["Jabón vegetal de palma"] =
            [
                ("Fruto de palma", 0.25m), ("Soda cáustica", 0.04m),
                ("Esencia natural", 0.01m), ("Empaque para jabón", 1m)
            ],
            ["Manteca vegetal 500 g"] =
            [
                ("Fruto de palma", 0.70m), ("Envase 500 ml", 1m),
                ("Etiqueta 500 ml", 1m), ("Antioxidante alimentario", 0.008m)
            ]
        };

        foreach (var (productName, ingredients) in recipes)
        {
            var product = products[productName];
            if (product.Recipe.Count != 0) continue;
            foreach (var ingredient in ingredients)
                db.ProductRecipeItems.Add(new ProductRecipeItem
                {
                    ProductId = product.Id,
                    RawMaterialId = materials[ingredient.Material].Id,
                    QuantityPerUnit = ingredient.Quantity
                });
        }
        await db.SaveChangesAsync();

        const string seedReceptionMarker = "Carga demo ampliada para pruebas de producción";
        var supplier = await db.Suppliers.OrderBy(x => x.Id).FirstAsync();
        var warehouseUser = await db.Users.FirstOrDefaultAsync(x => x.Username == "almacen")
            ?? await db.Users.OrderBy(x => x.Id).FirstAsync();
        var reception = await db.Receptions.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Observation == seedReceptionMarker);
        if (reception is null)
        {
            reception = new Reception
            {
                SupplierId = supplier.Id, UserId = warehouseUser.Id, Date = DateTime.Today,
                Status = ReceptionStatuses.Confirmed, Observation = seedReceptionMarker
            };
            db.Receptions.Add(reception);
        }

        var demoNames = desiredMaterials.Select(x => x.Name)
            .Concat(["Fruto de palma", "Envase 1 litro", "Etiqueta Palma Verde"]).ToHashSet();
        var materialIdsWithReception = await db.ReceptionDetails.Select(x => x.RawMaterialId).ToHashSetAsync();
        foreach (var material in materials.Values.Where(x => demoNames.Contains(x.Name) && x.CurrentStock > 0 && !materialIdsWithReception.Contains(x.Id)))
            reception.Details.Add(new ReceptionDetail { RawMaterialId = material.Id, Quantity = material.CurrentStock });

        await db.SaveChangesAsync();
    }
}
