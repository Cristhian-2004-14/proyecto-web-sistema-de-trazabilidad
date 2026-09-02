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
    }
}
