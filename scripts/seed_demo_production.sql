/*
    Palma Verde - carga demo ampliada para SQL Server

    Requisitos:
      1. Haber aplicado previamente las migraciones de Entity Framework Core.
      2. Contar con al menos un proveedor y el usuario demo "almacen".

    El script es idempotente: puede ejecutarse más de una vez sin duplicar
    materias primas, productos, recetas ni la recepción de respaldo.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Materials TABLE
    (
        [Name] nvarchar(100) NOT NULL,
        UnitOfMeasure nvarchar(30) NOT NULL,
        CurrentStock decimal(18,2) NOT NULL,
        MinimumStock decimal(18,2) NOT NULL
    );

    INSERT INTO @Materials ([Name], UnitOfMeasure, CurrentStock, MinimumStock)
    VALUES
        (N'Fruto de palma', N'kg', 850, 250),
        (N'Envase 1 litro', N'unidad', 600, 150),
        (N'Etiqueta Palma Verde', N'unidad', 600, 150),
        (N'Envase 500 ml', N'unidad', 900, 200),
        (N'Etiqueta 500 ml', N'unidad', 900, 200),
        (N'Soda cáustica', N'kg', 300, 60),
        (N'Esencia natural', N'litro', 180, 30),
        (N'Empaque para jabón', N'unidad', 1200, 250),
        (N'Antioxidante alimentario', N'kg', 120, 20);

    MERGE MATERIA_PRIMA AS target
    USING @Materials AS source ON target.[Name] = source.[Name]
    WHEN NOT MATCHED THEN
        INSERT ([Name], UnitOfMeasure, CurrentStock, MinimumStock, IsActive)
        VALUES (source.[Name], source.UnitOfMeasure, source.CurrentStock, source.MinimumStock, 1);

    DECLARE @Products TABLE
    (
        [Name] nvarchar(120) NOT NULL,
        [Description] nvarchar(250) NOT NULL,
        MinimumStock int NOT NULL
    );

    INSERT INTO @Products ([Name], [Description], MinimumStock)
    VALUES
        (N'Palmito entero', N'Producto terminado demo', 100),
        (N'Aceite de palma 1 litro', N'Aceite refinado en presentación familiar', 100),
        (N'Aceite de palma 500 ml', N'Aceite refinado en presentación mediana', 150),
        (N'Jabón vegetal de palma', N'Jabón artesanal elaborado con derivados de palma', 120),
        (N'Manteca vegetal 500 g', N'Producto alimentario procesado a base de palma', 80);

    MERGE PRODUCTO AS target
    USING @Products AS source ON target.[Name] = source.[Name]
    WHEN NOT MATCHED THEN
        INSERT ([Name], [Description], UnitOfMeasure, CurrentStock, MinimumStock, IsActive)
        VALUES (source.[Name], source.[Description], N'unidad', 0, source.MinimumStock, 1);

    DECLARE @Recipes TABLE
    (
        ProductName nvarchar(120) NOT NULL,
        MaterialName nvarchar(100) NOT NULL,
        QuantityPerUnit decimal(18,4) NOT NULL
    );

    INSERT INTO @Recipes (ProductName, MaterialName, QuantityPerUnit)
    VALUES
        (N'Palmito entero', N'Fruto de palma', 1.2500),
        (N'Palmito entero', N'Envase 1 litro', 1.0000),
        (N'Palmito entero', N'Etiqueta Palma Verde', 1.0000),
        (N'Aceite de palma 1 litro', N'Fruto de palma', 1.3000),
        (N'Aceite de palma 1 litro', N'Envase 1 litro', 1.0000),
        (N'Aceite de palma 1 litro', N'Etiqueta Palma Verde', 1.0000),
        (N'Aceite de palma 1 litro', N'Antioxidante alimentario', 0.0100),
        (N'Aceite de palma 500 ml', N'Fruto de palma', 0.6500),
        (N'Aceite de palma 500 ml', N'Envase 500 ml', 1.0000),
        (N'Aceite de palma 500 ml', N'Etiqueta 500 ml', 1.0000),
        (N'Aceite de palma 500 ml', N'Antioxidante alimentario', 0.0050),
        (N'Jabón vegetal de palma', N'Fruto de palma', 0.2500),
        (N'Jabón vegetal de palma', N'Soda cáustica', 0.0400),
        (N'Jabón vegetal de palma', N'Esencia natural', 0.0100),
        (N'Jabón vegetal de palma', N'Empaque para jabón', 1.0000),
        (N'Manteca vegetal 500 g', N'Fruto de palma', 0.7000),
        (N'Manteca vegetal 500 g', N'Envase 500 ml', 1.0000),
        (N'Manteca vegetal 500 g', N'Etiqueta 500 ml', 1.0000),
        (N'Manteca vegetal 500 g', N'Antioxidante alimentario', 0.0080);

    INSERT INTO RECETA_PRODUCTO (ProductId, RawMaterialId, QuantityPerUnit)
    SELECT product.Id, material.Id, recipe.QuantityPerUnit
    FROM @Recipes recipe
    CROSS APPLY (SELECT TOP (1) Id FROM PRODUCTO WHERE [Name] = recipe.ProductName ORDER BY Id) product
    CROSS APPLY (SELECT TOP (1) Id FROM MATERIA_PRIMA WHERE [Name] = recipe.MaterialName ORDER BY Id) material
    WHERE NOT EXISTS
    (
        SELECT 1 FROM RECETA_PRODUCTO existing
        WHERE existing.ProductId = product.Id AND existing.RawMaterialId = material.Id
    );

    DECLARE @SupplierId int = (SELECT TOP (1) Id FROM PROVEEDOR WHERE IsActive = 1 ORDER BY Id);
    DECLARE @UserId int = (SELECT TOP (1) Id FROM USUARIO WHERE Username = N'almacen' AND IsActive = 1 ORDER BY Id);
    DECLARE @Marker nvarchar(300) = N'Carga demo ampliada para pruebas de producción';

    IF @SupplierId IS NULL OR @UserId IS NULL
        THROW 51000, 'Se necesita un proveedor activo y el usuario almacen antes de cargar la recepción demo.', 1;

    DECLARE @ReceptionId int = (SELECT TOP (1) Id FROM RECEPCION WHERE Observation = @Marker ORDER BY Id);
    IF @ReceptionId IS NULL
    BEGIN
        INSERT INTO RECEPCION (SupplierId, UserId, [Date], Observation, [Status])
        VALUES (@SupplierId, @UserId, CONVERT(date, GETDATE()), @Marker, N'Confirmada');
        SET @ReceptionId = SCOPE_IDENTITY();
    END;

    INSERT INTO DETALLE_RECEPCION (ReceptionId, RawMaterialId, Quantity)
    SELECT @ReceptionId, material.Id, material.CurrentStock
    FROM MATERIA_PRIMA material
    INNER JOIN @Materials demo ON demo.[Name] = material.[Name]
    WHERE material.CurrentStock > 0
      AND NOT EXISTS (SELECT 1 FROM DETALLE_RECEPCION detail WHERE detail.RawMaterialId = material.Id);

    COMMIT TRANSACTION;

    SELECT N'Carga demo aplicada correctamente.' AS Resultado;
    SELECT [Name], UnitOfMeasure, CurrentStock, MinimumStock, IsActive
    FROM MATERIA_PRIMA WHERE [Name] IN (SELECT [Name] FROM @Materials) ORDER BY [Name];
    SELECT product.[Name], COUNT(recipe.Id) AS Ingredientes
    FROM PRODUCTO product LEFT JOIN RECETA_PRODUCTO recipe ON recipe.ProductId = product.Id
    WHERE product.[Name] IN (SELECT [Name] FROM @Products)
    GROUP BY product.[Name] ORDER BY product.[Name];
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
