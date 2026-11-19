# CLAUDE.md

Prueba técnica de Farmacorp: un POS de consola en .NET 8 / C#. El enunciado completo (reglas de negocio y diagrama de tablas) está en el PDF de la evaluación; el README resume ambos.

## Comandos

```bash
dotnet build FarmacorpPOSExpress.sln
dotnet run --project FarmacorpPOS.ConsoleApp
dotnet ef migrations add <Nombre> --project FarmacorpPOS.DataAccess --startup-project FarmacorpPOS.ConsoleApp
dotnet ef database update     --project FarmacorpPOS.DataAccess --startup-project FarmacorpPOS.ConsoleApp
```

- El proyecto apunta a `net8.0`. Las plantillas del SDK instalado no ofrecen net8.0, así que al crear un proyecto nuevo hay que cambiar el `TargetFramework` a mano en el `.csproj`.
- La solución usa el formato `.sln`, no `.slnx` (`dotnet new sln --format sln`).

## Arquitectura

Arquitectura N-Capas. Cada capa solo conoce a las de abajo:

```
FarmacorpPOS.ConsoleApp   → Presentación: menús y armado del contenedor de DI (Program.cs)
FarmacorpPOS.Business     → Servicios con las transacciones y las reglas de negocio
FarmacorpPOS.DataAccess   → DbContext, configuraciones Fluent API, Repositories, UnitOfWork, Migrations
FarmacorpPOS.Domain       → Entidades. No depende de ningún otro proyecto ni paquete
```

- **Domain no referencia a EF Core.** Las entidades son clases simples, sin atributos `[Key]`, `[Required]`, etc. Todo el mapeo va en DataAccess con `IEntityTypeConfiguration<T>`, una clase por entidad. Es requisito de la prueba usar Fluent API.
- **Business no ve el `DbContext`.** Solo trabaja con `IUnitOfWork` y los repositorios, así la lógica se puede probar sin base de datos.
- **Una transacción de negocio = un `SaveChanges()`.** Por ejemplo, la venta y el descuento de stock se confirman juntos, porque si uno falla el otro no debe quedar guardado.
- **Inyección por constructor** con `Microsoft.Extensions.DependencyInjection`. Nada de `new` sobre servicios ni repositorios fuera de `Program.cs`.

## Modelo de datos

Los nombres siguen exactamente el diagrama del enunciado. No hay que "mejorarlos", porque el evaluador va a comparar contra ese diagrama.

Las entidades están separadas por módulo, igual que en el diagrama, y dentro de Express también por área:

```
Entities/
├── Express/
│   ├── Productos/    → ExpProducto, TipoProducto
│   ├── Categorias/   → Categoria, ProductoCategoria
│   └── Ventas/       → VentaExpress
└── Erp/              → ErpProducto, CodigoBarra
```

Así se ve de un vistazo qué pertenece al sistema original y qué agregó el ERP, y dentro de Express se encuentra rápido todo lo de un área sin revisar el módulo completo.

- El namespace siempre coincide con la carpeta (ej. `FarmacorpPOS.Domain.Entities.Express.Ventas`).
- Cualquier entidad nueva va en la carpeta de su módulo y de su área.
- La misma separación se repite en las capas de arriba cuando aplique (por ejemplo, `Configurations/Express/Ventas` y `Configurations/Erp`).

| Módulo  | Entidad             | Tabla                 | Clave               |
|---------|---------------------|-----------------------|---------------------|
| Express | `TipoProducto`      | `TiposProducto`       | `IdTipoProducto`    |
| Express | `ExpProducto`       | `ExpProductos`        | `IdProducto`        |
| Express | `Categoria`         | `Categorias`          | `IdCategoria`       |
| Express | `ProductoCategoria` | `ProductosCategorias` | `IdDetalle`         |
| Express | `VentaExpress`      | `VentaExpress`        | `Id`                |
| ERP     | `ErpProducto`       | `ErpProductos`        | `IdProducto` (PK y FK a `ExpProductos`) |
| ERP     | `CodigoBarra`       | `CodigosBarras`       | `IdCodigoBarra`     |

- Los campos que en el diagrama no están en negrita son opcionales (`?`): `Observaciones`, `IdCategoriaPadre`, `Cliente`, `UniqueProducto`, `Descuento`.
- `Categoria` se referencia a sí misma para formar jerarquías.
- `VentaExpress.Producto` es el **nombre** del producto en texto (una copia al momento de vender). Por eso la propiedad que apunta a la entidad se llama `ExpProducto`.
- Los montos son `decimal` y en Fluent API se mapean con `HasPrecision(18, 2)`.
- Los `UniqueCodigo` (y `VentaExpress.UniqueProducto`) son `Guid`, con índice único. Se generan en Business con `Guid.NewGuid()` y nunca los ingresa el usuario. No usamos GUID v7: .NET 8 no lo trae y no queremos un generador propio.

## Reglas de negocio y estrategias

El sistema tiene que poder cambiar entre la estrategia **Base** y **GanaMax** en cualquier momento, sin tocar código. Las reglas que cambian van detrás de una abstracción (una estrategia elegida por configuración e inyectada por DI), no en `if`s dentro de los servicios.

| Regla | Base | GanaMax |
|-------|------|---------|
| RN1 – Precio al registrar producto | Costo × 1.50 | Costo × 1.80 |
| RN2 – Guardar `UniqueCodigo` en la venta | Sí | Igual |
| RN3 – Descuento si el producto tiene **exactamente una** categoría | 30 % | 10 % |
| RN4 – Validación de stock | Stock disponible | Stock disponible **y** que después de la venta queden más de 10 unidades |
| RN5 – Descontar stock en el ERP al vender | Sí | Igual |

La venta es siempre de un único producto.

## Estilo de código

- Código, nombres y comentarios en español, igual que el dominio del enunciado.
- Los comentarios explican el **porqué**, no el cómo. Escritos con naturalidad, como se los explicarías a un compañero. Si la línea se entiende sola, no lleva comentario. Nada de `/// <summary>` que repitan el nombre de la clase o de la tabla, ni etiquetas como `// Navegación`.
- `Nullable` e `ImplicitUsings` habilitados. Usa namespaces de archivo (`namespace X;`).
- Las colecciones de navegación se inicializan con `new List<T>()`, y las referencias obligatorias con `= null!`.
