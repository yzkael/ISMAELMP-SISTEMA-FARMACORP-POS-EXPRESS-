# Sistema FARMACORP POS EXPRESS

Sistema de punto de venta (POS) desarrollado en **.NET / C#** con **arquitectura N-Capas**, que integra dos módulos:

- **Módulo Express**: módulo original del sistema, orientado a la **Venta Express**, es decir, la venta por artículo.
- **Módulo ERP**: incorporado posteriormente por decisión de los inversionistas. Agrega la funcionalidad de **catálogo e inventario base** y completa la información del producto: costo, código único del producto (`UniqueCodigo`), stock, códigos de barras, categorías, etc.

La capa de presentación es una **aplicación de consola**.

---

## Tabla de contenido

1. [Tecnologías](#tecnologías)
2. [Arquitectura](#arquitectura)
3. [Estructura de la solución](#estructura-de-la-solución)
4. [Transacciones implementadas](#transacciones-implementadas)
5. [Reglas de negocio](#reglas-de-negocio)
6. [Patrones y conceptos aplicados](#patrones-y-conceptos-aplicados)
7. [Requisitos previos](#requisitos-previos)
8. [Instalación y ejecución](#instalación-y-ejecución)
9. [Migraciones de base de datos](#migraciones-de-base-de-datos)
10. [Uso del sistema](#uso-del-sistema)

---

## Tecnologías

| Componente            | Tecnología                                   |
|-----------------------|----------------------------------------------|
| Plataforma            | .NET 8                                       |
| Lenguaje              | C#                                           |
| Presentación          | Aplicación de Consola                        |
| ORM                   | Entity Framework Core (Code First)           |
| Mapeo de entidades    | Fluent API (`IEntityTypeConfiguration<T>`)   |
| Base de datos         | SQL Server Express LocalDB                   |
| Inyección de dependencias | `Microsoft.Extensions.DependencyInjection` |

---

## Arquitectura

El sistema sigue una **Arquitectura N-Capas**, donde cada capa tiene una responsabilidad única y solo depende de las capas inferiores (o de abstracciones):

```
┌──────────────────────────────────────────┐
│   Capa de Presentación (Consola)         │  Menús, captura de datos, composición de dependencias (IoC)
└───────────────────┬──────────────────────┘
                    │
┌───────────────────▼──────────────────────┐
│   Capa de Negocio (Servicios)            │  Transacciones y Reglas de Negocio 1 a 5
└───────────────────┬──────────────────────┘
                    │  (interfaces IUnitOfWork / IRepository)
┌───────────────────▼──────────────────────┐
│   Capa de Acceso a Datos                 │  DbContext, Fluent API, Repositories, UnitOfWork
└───────────────────┬──────────────────────┘
                    │
┌───────────────────▼──────────────────────┐
│   Capa de Dominio (Entidades)            │  Entidades del módulo Express y del módulo ERP
└──────────────────────────────────────────┘
                    │
              SQL Server LocalDB
```

---

## Estructura de la solución

```
FarmacorpPOSExpress.sln
│
├── FarmacorpPOS.Domain/                 # Capa de Dominio
│   └── Entities/
│       ├── Producto.cs                  # Producto del módulo Express
│       ├── ProductoErp.cs               # Información ERP: Costo, UniqueCodigo, Stock
│       ├── CodigoBarra.cs               # Códigos de barras asignados a un producto
│       ├── Categoria.cs
│       ├── ProductoCategoria.cs         # Relación Producto ↔ Categoría
│       └── Venta.cs                     # Venta Express (un único producto)
│
├── FarmacorpPOS.DataAccess/             # Capa de Acceso a Datos
│   ├── Context/
│   │   └── FarmacorpDbContext.cs
│   ├── Configurations/                  # Fluent API (una clase por entidad)
│   ├── Repositories/
│   │   ├── Interfaces/IRepository.cs
│   │   └── Repository.cs
│   ├── UnitOfWork/
│   │   ├── IUnitOfWork.cs
│   │   └── UnitOfWork.cs
│   └── Migrations/
│
├── FarmacorpPOS.Business/               # Capa de Negocio
│   ├── Interfaces/
│   └── Services/
│       ├── ProductoService.cs           # Registro Nuevo Producto ERP
│       ├── CodigoBarraService.cs        # Asignación de Códigos de Barras
│       ├── CategoriaService.cs          # Asignación de Categorías
│       └── VentaService.cs              # Registro de Venta
│
└── FarmacorpPOS.ConsoleApp/             # Capa de Presentación
    ├── Program.cs                       # Configuración del contenedor IoC
    ├── Menus/
    └── appsettings.json                 # Cadena de conexión
```

---

## Transacciones implementadas

### 1. Registro Nuevo Producto ERP
Registra un producto junto con su información ERP (costo, stock, etc.).
- El valor de **`UniqueCodigo`** se **genera automáticamente**.
- Aplica la **Regla de Negocio 1** para calcular el precio.

### 2. Registro de Asignación de Códigos de Barras
Asigna uno o más códigos de barras a un producto existente.
- El **`UniqueCodigo`** que representa el código de barras se **genera automáticamente**.

### 3. Registro de Asignación de Categorías de Productos
Asocia un producto con una o varias categorías.

### 4. Registro de Venta
Registra una Venta Express para **un único producto**, aplicando las **Reglas de Negocio 2, 3, 4 y 5**.

---

## Reglas de negocio

| N.º | Regla | Transacción |
|-----|-------|-------------|
| **RN1** | El **precio** del producto se determina con un **margen del 50 % sobre el costo**: `Precio = Costo × 1.50` | Registro Nuevo Producto ERP |
| **RN2** | Se registra el **`UniqueCodigo` del producto** como parte de la venta. | Registro de Venta |
| **RN3** | Se aplica un **descuento del 30 %** **si y solo si** el producto está asociado a **una única categoría**. | Registro de Venta |
| **RN4** | Solo se permite la venta si el artículo tiene **stock disponible**. | Registro de Venta |
| **RN5** | Como parte de la venta se **actualiza el stock** del artículo en el módulo ERP. | Registro de Venta |

### Flujo del Registro de Venta

```
1. Obtener el producto y su información ERP
2. RN4 → ¿Stock disponible para la cantidad solicitada?  No → rechazar la venta
3. Calcular subtotal = Precio × Cantidad
4. RN3 → ¿El producto tiene exactamente 1 categoría?     Sí → descuento del 30 %
5. RN2 → Asignar el UniqueCodigo del producto a la venta
6. RN5 → Descontar la cantidad vendida del Stock ERP
7. UnitOfWork.SaveChanges() → la venta y la actualización del stock se confirman
   en una sola transacción (todo o nada)
```

---

## Patrones y conceptos aplicados

### Repository
Abstrae el acceso a datos de cada entidad mediante una interfaz genérica (`IRepository<T>`) con operaciones como `GetById`, `GetAll`, `Find`, `Add`, `Update` y `Remove`. La capa de negocio no conoce a Entity Framework directamente.

### Unit of Work
`IUnitOfWork` agrupa los repositorios y comparte un único `DbContext`, de modo que varias operaciones (por ejemplo, registrar la venta **y** actualizar el stock) se confirmen de forma **atómica** con una sola llamada a `SaveChanges()`.

### Inversión de Control (IoC) e Inyección de Dependencias (DI)
Las capas superiores dependen de **abstracciones** (interfaces), no de implementaciones concretas. Las dependencias se registran en el contenedor de `Microsoft.Extensions.DependencyInjection` en `Program.cs` y se inyectan por **constructor**:

```csharp
services.AddDbContext<FarmacorpDbContext>(o =>
    o.UseSqlServer(configuration.GetConnectionString("FarmacorpDb")));

services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IProductoService, ProductoService>();
services.AddScoped<ICodigoBarraService, CodigoBarraService>();
services.AddScoped<ICategoriaService, CategoriaService>();
services.AddScoped<IVentaService, VentaService>();
```

### Entity Framework Code First + Fluent API
El modelo de base de datos se genera a partir de las clases de dominio. Toda la configuración de mapeo (claves, longitudes, precisión decimal, relaciones, índices únicos) se define con **Fluent API** en clases `IEntityTypeConfiguration<T>`, manteniendo las entidades libres de atributos de persistencia:

```csharp
public class ProductoErpConfiguration : IEntityTypeConfiguration<ProductoErp>
{
    public void Configure(EntityTypeBuilder<ProductoErp> builder)
    {
        builder.ToTable("ProductoErp");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.UniqueCodigo).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.UniqueCodigo).IsUnique();
        builder.Property(p => p.Costo).HasPrecision(18, 2);
    }
}
```

---

## Requisitos previos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) o superior
- **SQL Server Express LocalDB** (incluido con Visual Studio o instalable desde el instalador de SQL Server Express)
- Herramienta de EF Core:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

> **Nota:** LocalDB solo está disponible en **Windows**. En Linux o macOS se puede usar SQL Server en Docker y cambiar la cadena de conexión:
> ```bash
> docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=TuPassword123!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
> ```
> ```
> Server=localhost,1433;Database=FarmacorpPOSExpress;User Id=sa;Password=TuPassword123!;TrustServerCertificate=True;
> ```

---

## Instalación y ejecución

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/yzkael/ISMAELMP-SISTEMA-FARMACORP-POS-EXPRESS-.git
   cd ISMAELMP-SISTEMA-FARMACORP-POS-EXPRESS-
   ```

2. **Configurar la cadena de conexión** en `FarmacorpPOS.ConsoleApp/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "FarmacorpDb": "Server=(localdb)\\MSSQLLocalDB;Database=FarmacorpPOSExpress;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Restaurar paquetes y compilar**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Crear la base de datos** (ver [Migraciones](#migraciones-de-base-de-datos))

5. **Ejecutar la aplicación**
   ```bash
   dotnet run --project FarmacorpPOS.ConsoleApp
   ```

---

## Migraciones de base de datos

Crear la migración inicial:
```bash
dotnet ef migrations add InitialCreate \
  --project FarmacorpPOS.DataAccess \
  --startup-project FarmacorpPOS.ConsoleApp
```

Aplicar las migraciones a la base de datos:
```bash
dotnet ef database update \
  --project FarmacorpPOS.DataAccess \
  --startup-project FarmacorpPOS.ConsoleApp
```

---

## Uso del sistema

Al ejecutar la aplicación se muestra el menú principal:

```
=========================================
      FARMACORP POS EXPRESS
=========================================
 1. Registrar nuevo producto ERP
 2. Asignar código de barras a producto
 3. Asignar categoría a producto
 4. Registrar venta
 5. Listar productos
 0. Salir
=========================================
Seleccione una opción:
```

### Ejemplo de venta

```
Producto:        Paracetamol 500mg
UniqueCodigo:    PRD-000001
Costo:           10.00
Precio (RN1):    15.00
Categorías:      1 (Analgésicos)  → aplica descuento 30 % (RN3)
Stock antes:     20
Cantidad:        2
-----------------------------------------
Subtotal:        30.00
Descuento:       -9.00
Total:           21.00
Stock después:   18  (RN5)
```

---

## Autor

**Ismael MP** — [@yzkael](https://github.com/yzkael)
