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

El sistema sigue una **Arquitectura N-Capas** de cinco proyectos. Cada capa tiene una única responsabilidad y solo conoce a la capa de abajo:

```
┌──────────────────────────────────────────┐
│   Presentación (ConsoleApp)              │  Controllers: leen la consola y llaman a los servicios
└───────────────────┬──────────────────────┘  Program.cs: contenedor de IoC
                    │
┌───────────────────▼──────────────────────┐
│   Servicios (Services)                   │  Transacciones, Reglas de Negocio 1 a 5,
└───────────────────┬──────────────────────┘  estrategias Base y GanaMax
                    │  IUnitOfWork
┌───────────────────▼──────────────────────┐
│   Repositorios (Repositories)            │  IRepository<T>, UnitOfWork
└───────────────────┬──────────────────────┘
                    │
┌───────────────────▼──────────────────────┐
│   Infraestructura (Infrastructure)       │  DbContext, Fluent API, migraciones
└───────────────────┬──────────────────────┘
                    │
┌───────────────────▼──────────────────────┐
│   Dominio (Domain)                       │  Entidades del módulo Express y del módulo ERP
└──────────────────────────────────────────┘

              SQL Server LocalDB  ◄── solo Infrastructure habla con la base
```

---

## Estructura de la solución

```
FarmacorpPOSExpress.sln
│
├── FarmacorpPOS.Domain/                 # Capa de Dominio
│   └── Entities/
│       ├── Express/                     # Módulo Express
│       │   ├── Productos/
│       │   │   ├── ExpProducto.cs       # Producto base
│       │   │   └── TipoProducto.cs
│       │   ├── Categorias/
│       │   │   ├── Categoria.cs         # Jerárquica (auto-referencia)
│       │   │   └── ProductoCategoria.cs # Relación Producto ↔ Categoría
│       │   └── Ventas/
│       │       └── VentaExpress.cs      # Venta de un único producto
│       └── Erp/                         # Módulo ERP
│           ├── ErpProducto.cs           # Costo, UniqueCodigo, Stock (PK = FK a ExpProducto)
│           └── CodigoBarra.cs           # Códigos de barras asignados a un producto
│
├── FarmacorpPOS.Infrastructure/         # Capa de Infraestructura
│   ├── Data/
│   │   ├── FarmacorpDbContext.cs
│   │   ├── FarmacorpDbContextSeeder.cs  # Migra y carga los datos iniciales al arrancar
│   │   ├── Configurations/              # Fluent API, una clase por entidad
│   │   │   ├── Express/
│   │   │   │   ├── Productos/
│   │   │   │   ├── Categorias/
│   │   │   │   └── Ventas/
│   │   │   └── Erp/
│   │   └── Migrations/
│   └── DependencyInjection.cs           # AddInfrastructureServices()
│
├── FarmacorpPOS.Repositories/           # Capa de Repositorios
│   ├── IRepository.cs
│   ├── Repository.cs
│   ├── UnitsOfWork/
│   │   ├── IUnitOfWork.cs
│   │   └── UnitOfWork.cs
│   └── DependencyInjection.cs           # AddRepositoryServices()
│
├── FarmacorpPOS.Services/               # Capa de Servicios
│   ├── DTOs/                            # Records de entrada (…Solicitud) y salida
│   │   ├── Erp/
│   │   │   ├── Productos/
│   │   │   └── CodigosBarras/
│   │   └── Express/
│   │       ├── Categorias/
│   │       └── Ventas/
│   ├── Estrategias/                     # IEstrategiaNegocio, EstrategiaBase, EstrategiaGanaMax
│   ├── Excepciones/                     # ReglaNegocioException
│   ├── Erp/
│   │   ├── Productos/                   # Registro Nuevo Producto ERP
│   │   └── CodigosBarras/               # Asignación de Códigos de Barras
│   ├── Express/
│   │   ├── Categorias/                  # Asignación de Categorías
│   │   └── Ventas/                      # Registro de Venta
│   └── DependencyInjection.cs           # AddBusinessServices() y elección de la estrategia
│
└── FarmacorpPOS.ConsoleApp/             # Capa de Presentación
    ├── Program.cs                       # Arma el host, registra cada capa y abre el menú principal
    ├── Menus/
    │   └── MenuPrincipal.cs             # Bucle del menú, un scope por opción y manejo de errores
    ├── Controllers/                     # Leen la consola, arman el DTO y llaman al servicio
    │   ├── Erp/
    │   │   ├── Productos/               # Registrar y listar productos
    │   │   └── CodigosBarras/           # Asignar códigos de barras
    │   └── Express/
    │       ├── Categorias/              # Asignar categorías
    │       └── Ventas/                  # Registrar ventas
    ├── Utilidades/                      # Lectura validada de datos, tablas y formato de montos
    ├── DependencyInjection.cs           # AddPresentationServices()
    └── appsettings.json                 # Cadena de conexión, estrategia activa y nivel de log
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

### Estrategia GanaMax

Durante 3 meses el negocio aplica la estrategia **GanaMax**, que cambia tres reglas. El resto se mantiene igual que en Base:

| N.º | Base | GanaMax |
|-----|------|---------|
| **RN1** | Precio = Costo × 1.50 | Precio = Costo × 1.80 |
| **RN3** | 30 % de descuento con una única categoría | 10 % de descuento con una única categoría |
| **RN4** | Stock disponible | Stock disponible **y** que después de la venta queden **más de 10 unidades** |

Cada estrategia es una clase que implementa `IEstrategiaNegocio`, y los servicios solo conocen esa interfaz. La activa se elige con la clave `Estrategia` del `appsettings.json`, así que volver a Base es cambiar un valor y reiniciar, sin tocar código.

### Flujo del Registro de Venta

```
1. Obtener el producto y su ficha ERP           No existe o inactivo → rechazar la venta
2. RN4 → Validar stock según la estrategia       No cumple → rechazar la venta
3. Calcular subtotal = Precio × Cantidad
4. RN3 → ¿El producto tiene exactamente 1 categoría?
         Sí → descuento del 30 % (Base) o 10 % (GanaMax)
5. RN2 → Guardar el UniqueCodigo del producto en la venta
6. RN5 → Descontar la cantidad vendida del Stock ERP
7. UnitOfWork.GuardarCambiosAsync() → la venta y la actualización del stock se confirman
   en una sola transacción (todo o nada)
```

Cuando una regla no se cumple, el servicio lanza `ReglaNegocioException` con un mensaje para el usuario (por ejemplo, *"Con la estrategia GanaMax la venta debe dejar más de 10 unidades en stock, y quedarían 10."*) y no se guarda nada.

---

## Patrones y conceptos aplicados

### Repository
Abstrae el acceso a datos de cada entidad mediante una interfaz genérica (`IRepository<T>`) con operaciones como `ObtenerPorIdAsync`, `ObtenerTodosAsync`, `BuscarAsync`, `ExisteAsync`, `ContarAsync`, `Agregar`, `Actualizar` y `Eliminar`. La capa de servicios no conoce a Entity Framework directamente.

### Unit of Work
`IUnitOfWork` agrupa los repositorios y comparte un único `DbContext`, de modo que varias operaciones (por ejemplo, registrar la venta **y** actualizar el stock) se confirmen de forma **atómica** con una sola llamada a `GuardarCambiosAsync()`.

### Inversión de Control (IoC) e Inyección de Dependencias (DI)
Las capas superiores dependen de **abstracciones** (interfaces), no de implementaciones concretas. Las dependencias se inyectan por **constructor** con `Microsoft.Extensions.DependencyInjection`.

Cada capa registra sus propias dependencias en un `DependencyInjection.cs`, siguiendo el enfoque de [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture). `Program.cs` solo arma el host y llama a cada capa, sin conocer sus clases concretas:

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.AddInfrastructureServices();   // DbContext + SQL Server
builder.AddRepositoryServices();       // IUnitOfWork
builder.AddBusinessServices();         // Servicios y estrategia Base / GanaMax
builder.AddPresentationServices();     // Controllers y menú

using var host = builder.Build();

await host.SeedDatabaseAsync();         // Migra la base y carga los datos iniciales
await host.Services.GetRequiredService<MenuPrincipal>().MostrarAsync();
```

Por ejemplo, la capa de infraestructura:

```csharp
public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("FarmacorpDb")
        ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'FarmacorpDb' en appsettings.json.");

    builder.Services.AddDbContext<FarmacorpDbContext>(options =>
        options.UseSqlServer(connectionString));
}
```

### Entity Framework Code First + Fluent API
El modelo de base de datos se genera a partir de las clases de dominio. Toda la configuración de mapeo (claves, longitudes, precisión decimal, relaciones, índices únicos) se define con **Fluent API** en clases `IEntityTypeConfiguration<T>`, manteniendo las entidades libres de atributos de persistencia:

```csharp
public class ErpProductoConfiguration : IEntityTypeConfiguration<ErpProducto>
{
    public void Configure(EntityTypeBuilder<ErpProducto> builder)
    {
        builder.ToTable("ErpProductos");
        builder.HasKey(p => p.IdProducto);
        builder.Property(p => p.UniqueCodigo).IsRequired();
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
     },
     "Estrategia": "Base"
   }
   ```
   `Estrategia` acepta `Base` o `GanaMax`. Basta con cambiarla y reiniciar la aplicación para pasar de una estrategia a la otra.

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

La migración inicial (`InitialCreate`) ya está incluida en `FarmacorpPOS.Infrastructure/Data/Migrations` y solo crea el esquema.

**No hace falta aplicarla a mano.** Al arrancar, la aplicación ejecuta `FarmacorpDbContextSeeder`, que:

1. Aplica las migraciones pendientes (crea la base si no existe).
2. Carga los tipos de producto y las categorías iniciales.

Cada dato se busca por su descripción antes de insertarlo, así el proceso se puede repetir en cada arranque sin duplicar nada. Si se agrega un tipo o una categoría nueva a la lista, solo se inserta lo que falta. Las subcategorías apuntan a su categoría padre por navegación, sin depender de Ids fijos.

Si prefieres aplicar las migraciones manualmente:
```bash
dotnet ef database update --project FarmacorpPOS.Infrastructure --startup-project FarmacorpPOS.ConsoleApp
```

Crear una migración nueva después de cambiar el modelo:
```bash
dotnet ef migrations add <NombreMigracion> --project FarmacorpPOS.Infrastructure --startup-project FarmacorpPOS.ConsoleApp --output-dir Data/Migrations
```

> Si solo tienes instalados runtimes más nuevos que .NET 8, antepón `DOTNET_ROLL_FORWARD=Major` a estos comandos.

---

## Uso del sistema

Al ejecutar la aplicación se muestra el menú principal, con la estrategia activa en el encabezado:

```
=========================================
        FARMACORP POS EXPRESS
        Estrategia activa: Base
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

Las opciones que piden un Id (códigos de barras, categorías y ventas) muestran antes la lista de productos y categorías disponibles. Los montos aceptan coma o punto decimal (`10,50` o `10.50`) y las fechas se escriben como `dd/mm/aaaa`. Si un dato tiene un formato inválido, se vuelve a pedir.

Para **volver al menú** desde cualquier opción, escribe `x` en el dato que se esté pidiendo. La operación se cancela sin guardar nada.

### Ejemplo de venta

Paracetamol 500mg, con costo Bs 10.50, una sola categoría (Analgésicos) y estrategia Base:

```
--- Registrar venta ---
  Id   Nombre                   Tipo                   Precio  Stock  Categ.  Cód.
  1    Paracetamol 500mg        Medicamento          Bs 15.75     20       1     1

Id del producto: 1
Cantidad: 2
Cliente (Enter para omitir): Ana Pérez
Venta #1 registrada (estrategia Base).
  Producto:        Paracetamol 500mg
  UniqueCodigo:    1b4c12f8-ca62-47ee-9689-4e99d281a572
  Cantidad:        2
  Precio unit.:    Bs 15.75
  Subtotal:        Bs 31.50
  Descuento:       -Bs 9.45
  Total:           Bs 22.05
  Stock restante:  18
```

- **RN1:** precio = 10.50 × 1.50 = Bs 15.75
- **RN2:** el `UniqueCodigo` del producto queda guardado en la venta
- **RN3:** una sola categoría → 30 % de descuento sobre Bs 31.50
- **RN4 y RN5:** había stock suficiente y bajó de 20 a 18

Si una regla no se cumple, se muestra el motivo y no se guarda nada:

```
Stock insuficiente: hay 18 unidades y se pidieron 999.
Con la estrategia GanaMax la venta debe dejar más de 10 unidades en stock, y quedarían 10.
```

---

## Autor

**Ismael MP** — [@yzkael](https://github.com/yzkael)
