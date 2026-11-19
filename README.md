# Sistema FARMACORP POS EXPRESS

> Antes que nada, quiero agradecer la oportunidad de participar en este proceso. Me emociona mucho la posibilidad de formar parte de **NexoCorp**, y puse en este proyecto el mismo cuidado con el que me gustaría trabajar en el equipo.
>
> La solución tiene una estructura **N-Capas** inspirada en dos referencias que admiro. La primera es el repositorio [CleanArchitecture de Jason Taylor](https://github.com/jasontaylordev/CleanArchitecture), de donde tomé, por ejemplo, que cada capa registre sus propias dependencias y que la base se inicialice al arrancar. La segunda es el libro de **Milan Jovanović** sobre .NET y la **Onion Architecture**, de donde viene la idea de la repository layer y la service layer.
>
> También los invito a revisar mi [GitHub](https://github.com/yzkael), donde está documentado mi avance y mi pasión por .NET, así como los distintos tipos de arquitectura que he ido explorando.
>
> — Ismael

Solución a la prueba técnica **"Desarrollo e Implementación – Sistema FARMACORP POS EXPRESS"**.

Es un punto de venta de consola en **.NET 8 / C#** con **arquitectura N-Capas**. Integra los dos módulos del enunciado:

- **Express**: el módulo original, con la **Venta Express** (venta por artículo).
- **ERP**: agregado después. Completa el producto con costo, `UniqueCodigo`, stock y códigos de barras.

Implementa las cuatro transacciones pedidas, las Reglas de Negocio 1 a 5 y la estrategia **GanaMax**, que se activa o se vuelve a Base en cualquier momento desde el menú, sin reiniciar la aplicación.

---

## Tabla de contenido

1. [Cómo ejecutarlo](#cómo-ejecutarlo)
2. [Requisitos del enunciado y dónde se cumplen](#requisitos-del-enunciado-y-dónde-se-cumplen)
3. [Arquitectura](#arquitectura)
4. [Estructura de la solución](#estructura-de-la-solución)
5. [Reglas de negocio](#reglas-de-negocio)
6. [Decisiones de diseño](#decisiones-de-diseño)
7. [Interpretaciones del enunciado](#interpretaciones-del-enunciado)
8. [Cómo se verificó](#cómo-se-verificó)
9. [Uso del sistema](#uso-del-sistema)
10. [Migraciones de base de datos](#migraciones-de-base-de-datos)

---

## Cómo ejecutarlo

### Windows (SQL Server Express LocalDB)

Requisitos: [.NET SDK 8](https://dotnet.microsoft.com/download) y **SQL Server Express LocalDB**, que viene con Visual Studio.

```bash
git clone https://github.com/yzkael/ISMAELMP-SISTEMA-FARMACORP-POS-EXPRESS-.git
cd ISMAELMP-SISTEMA-FARMACORP-POS-EXPRESS-
dotnet run --project FarmacorpPOS.ConsoleApp
```

En Visual Studio, se marca `FarmacorpPOS.ConsoleApp` como proyecto de inicio y se presiona F5.

**No hace falta crear la base ni correr migraciones.** Al arrancar, la aplicación crea la base `FarmacorpPOSExpress` en LocalDB, aplica la migración y carga los tipos de producto y las categorías iniciales.

### Cambiar entre estrategia Base y GanaMax

**Desde la aplicación, en cualquier momento:** la opción **6. Cambiar estrategia** del menú pasa de Base a GanaMax o de vuelta a Base, sin reiniciar. El cambio se aplica desde la siguiente operación, y el encabezado del menú siempre muestra la estrategia activa.

**El `appsettings.json` solo define la estrategia con la que arranca la aplicación:**

```json
{
  "ConnectionStrings": {
    "FarmacorpDb": "Server=(localdb)\\MSSQLLocalDB;Database=FarmacorpPOSExpress;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Estrategia": "Base"
}
```

El cambio hecho desde el menú no se guarda en ningún archivo. Al reiniciar, la aplicación vuelve a arrancar con la estrategia del `appsettings.json`. Por eso, durante los 3 meses de GanaMax conviene poner ahí `"GanaMax"`, y volver a `"Base"` cuando termine. Si la clave falta se usa Base. Si tiene un valor desconocido, la aplicación no arranca.

### Linux o macOS (SQL Server en Docker)

LocalDB solo existe en Windows. En otros sistemas se puede levantar SQL Server en Docker y sobrescribir la cadena de conexión con una variable de entorno, sin modificar el `appsettings.json`:

```bash
docker run -d --name farmacorp-sql -e ACCEPT_EULA=Y -e 'MSSQL_SA_PASSWORD=Farmacorp_Local_2026!' \
  -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest

export ConnectionStrings__FarmacorpDb='Server=localhost,1433;Database=FarmacorpPOSExpress;User Id=sa;Password=Farmacorp_Local_2026!;TrustServerCertificate=True'
dotnet run --project FarmacorpPOS.ConsoleApp

# Para arrancar directamente con GanaMax sin tocar archivos:
Estrategia=GanaMax dotnet run --project FarmacorpPOS.ConsoleApp
```

> Si la máquina solo tiene runtimes más nuevos que .NET 8, se antepone `DOTNET_ROLL_FORWARD=Major` a `dotnet run`.

---

## Requisitos del enunciado y dónde se cumplen

### Condiciones técnicas

| Requisito                                        | Cómo se cumple                                                                                                  | Dónde                                             |
| ------------------------------------------------ | --------------------------------------------------------------------------------------------------------------- | ------------------------------------------------- |
| .NET / C#                                        | .NET 8 (LTS), C# 12                                                                                             | Todos los `.csproj`                               |
| Arquitectura N-Capas                             | Cinco proyectos, y cada uno solo conoce a la capa de abajo                                                      | [Arquitectura](#arquitectura)                     |
| Presentación en consola                          | Menú con un Controller por transacción                                                                          | `FarmacorpPOS.ConsoleApp`                         |
| UnitOfWork                                       | `IUnitOfWork` agrupa los repositorios sobre un solo `DbContext` y confirma con un único `GuardarCambiosAsync()` | `FarmacorpPOS.Repositories/UnitsOfWork`           |
| Repositories                                     | Repositorio genérico `IRepository<T>`                                                                           | `FarmacorpPOS.Repositories`                       |
| Inversión de Control e Inyección de Dependencias | Cada capa registra sus dependencias en su `DependencyInjection.cs`, y todo se inyecta por constructor           | `DependencyInjection.cs` de cada proyecto         |
| Entity Framework Code First                      | El esquema sale de las entidades, con la migración `InitialCreate`                                              | `FarmacorpPOS.Infrastructure/Data/Migrations`     |
| Fluent API Configurations                        | Una clase `IEntityTypeConfiguration<T>` por entidad. Las entidades no tienen atributos de EF                    | `FarmacorpPOS.Infrastructure/Data/Configurations` |
| SQL Server Express LocalDB                       | Cadena de conexión por defecto                                                                                  | `FarmacorpPOS.ConsoleApp/appsettings.json`        |

### Transacciones y reglas

| Requisito                                                         | Dónde                                                                                |
| ----------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| 1. Registro Nuevo Producto ERP, con `UniqueCodigo` automático     | `ProductoErpService.RegistrarAsync`                                                  |
| 2. Asignación de Códigos de Barras, con `UniqueCodigo` automático | `CodigoBarraService.AsignarAsync`                                                    |
| 3. Asignación de Categorías de Productos                          | `CategoriaService.AsignarAsync`                                                      |
| 4. Registro de Venta de un único producto                         | `VentaService.RegistrarAsync`                                                        |
| RN1 – Precio = costo + margen                                     | `IEstrategiaNegocio.CalcularPrecio`                                                  |
| RN2 – `UniqueCodigo` del producto en la venta                     | `VentaService` → `VentaExpress.UniqueProducto`                                       |
| RN3 – Descuento si y solo si hay una única categoría              | `VentaService` con `IEstrategiaNegocio.PorcentajeDescuentoCategoriaUnica`            |
| RN4 – Venta solo con stock disponible                             | `IEstrategiaNegocio.ValidarStock`                                                    |
| RN5 – Actualizar el stock del ERP al vender                       | `VentaService`, en el mismo guardado que la venta                                    |
| Estrategia GanaMax, con vuelta a Base en cualquier momento        | `EstrategiaBase`, `EstrategiaGanaMax`, `EstrategiaService` y la opción 6 del menú     |

---

## Arquitectura

```
┌──────────────────────────────────────────┐
│   Presentación (ConsoleApp)              │  Menú y Controllers: leen la consola,
└───────────────────┬──────────────────────┘  arman el DTO y llaman al servicio
                    │
┌───────────────────▼──────────────────────┐
│   Servicios (Services)                   │  Transacciones, Reglas 1 a 5,
└───────────────────┬──────────────────────┘  estrategias Base y GanaMax
                    │  solo IUnitOfWork
┌───────────────────▼──────────────────────┐
│   Repositorios (Repositories)            │  IRepository<T>, UnitOfWork
└───────────────────┬──────────────────────┘
                    │
┌───────────────────▼──────────────────────┐
│   Infraestructura (Infrastructure)       │  DbContext, Fluent API, migraciones, seeder
└───────────────────┬──────────────────────┘
                    │
┌───────────────────▼──────────────────────┐
│   Dominio (Domain)                       │  Entidades de Express y ERP
└──────────────────────────────────────────┘

              SQL Server  ◄── solo Infrastructure habla con la base
```

| Proyecto       | Referencia a                           | Responsabilidad                                                                   |
| -------------- | -------------------------------------- | --------------------------------------------------------------------------------- |
| Domain         | Ninguno                                | Entidades sin dependencias, ni siquiera de EF Core                                |
| Infrastructure | Domain                                 | Lo único que conoce EF Core y SQL Server                                          |
| Repositories   | Infrastructure (privada), Domain       | Acceso a datos y unidad de trabajo                                                |
| Services       | Repositories, Domain                   | Reglas de negocio, sin ver el `DbContext`                                         |
| ConsoleApp     | Services, Repositories, Infrastructure | Interfaz de usuario. Referencia a las capas de abajo solo para registrarlas en DI |

`Program.cs` solo arma el host y le pide a cada capa que se registre:

```csharp
var builder = Host.CreateApplicationBuilder(...);

builder.AddInfrastructureServices();   // DbContext + SQL Server + seeder
builder.AddRepositoryServices();       // IUnitOfWork
builder.AddBusinessServices();         // Servicios y estrategia Base / GanaMax
builder.AddPresentationServices();     // Controllers y menú

using var host = builder.Build();

await host.SeedDatabaseAsync();         // Migra la base y carga los datos iniciales
await host.Services.GetRequiredService<MenuPrincipal>().MostrarAsync();
```

---

## Estructura de la solución

Las entidades, configuraciones, servicios, DTOs y Controllers se separan igual que el diagrama del enunciado: primero por módulo (**Express** / **ERP**) y dentro de cada módulo por área.

```
FarmacorpPOSExpress.sln
│
├── FarmacorpPOS.Domain/
│   └── Entities/
│       ├── Express/
│       │   ├── Productos/        ExpProducto, TipoProducto
│       │   ├── Categorias/       Categoria (jerárquica), ProductoCategoria
│       │   └── Ventas/           VentaExpress
│       └── Erp/                  ErpProducto (PK = FK a ExpProducto), CodigoBarra
│
├── FarmacorpPOS.Infrastructure/
│   ├── Data/
│   │   ├── FarmacorpDbContext.cs
│   │   ├── FarmacorpDbContextSeeder.cs   Migra y carga los datos iniciales al arrancar
│   │   ├── Configurations/               Fluent API, una clase por entidad (Express/…, Erp/)
│   │   └── Migrations/                   InitialCreate
│   └── DependencyInjection.cs            AddInfrastructureServices()
│
├── FarmacorpPOS.Repositories/
│   ├── IRepository.cs, Repository.cs
│   ├── UnitsOfWork/                      IUnitOfWork, UnitOfWork
│   └── DependencyInjection.cs            AddRepositoryServices()
│
├── FarmacorpPOS.Services/
│   ├── Estrategias/                      IEstrategiaNegocio, EstrategiaBase, EstrategiaGanaMax,
│   │                                     EstrategiaService (estrategia activa y cambio en caliente)
│   ├── Excepciones/                      ReglaNegocioException
│   ├── DTOs/                             Records de entrada (…SolicitudDTO) y salida (…DTO)
│   ├── Erp/
│   │   ├── Productos/                    Registro Nuevo Producto ERP
│   │   └── CodigosBarras/                Asignación de Códigos de Barras
│   ├── Express/
│   │   ├── Categorias/                   Asignación de Categorías
│   │   └── Ventas/                       Registro de Venta
│   └── DependencyInjection.cs            AddBusinessServices() y estrategia inicial
│
└── FarmacorpPOS.ConsoleApp/
    ├── Program.cs
    ├── Menus/MenuPrincipal.cs            Menú, un scope de DI por opción, manejo de errores
    ├── Controllers/                      Erp/Productos, Erp/CodigosBarras, Express/Categorias, Express/Ventas, Estrategias
    ├── Utilidades/                       Lectura validada, tablas y formato de montos
    ├── DependencyInjection.cs            AddPresentationServices()
    └── appsettings.json                  Cadena de conexión y estrategia con la que arranca
```

---

## Reglas de negocio

| N.º     | Regla                                                            | Base             | GanaMax                                                                      |
| ------- | ---------------------------------------------------------------- | ---------------- | ---------------------------------------------------------------------------- |
| **RN1** | Precio del producto al registrarlo                               | Costo × 1.50     | Costo × 1.80                                                                 |
| **RN2** | El `UniqueCodigo` del producto se guarda en la venta             | Sí               | Igual que Base                                                               |
| **RN3** | Descuento si y solo si el producto tiene **una única categoría** | 30 %             | 10 %                                                                         |
| **RN4** | Condición para permitir la venta                                 | Stock disponible | Stock disponible **y** que después de la venta queden **más de 10 unidades** |
| **RN5** | La venta descuenta el stock del ERP                              | Sí               | Igual que Base                                                               |

### Flujo del Registro de Venta

```
1. Obtener el producto y su ficha ERP        No existe, está inactivo o no tiene ficha ERP → rechazar
2. RN4 → Validar stock según la estrategia    No cumple → rechazar
3. Subtotal = Precio × Cantidad
4. RN3 → ¿Exactamente 1 categoría?            Sí → descuento del 30 % (Base) o 10 % (GanaMax)
5. RN2 → Guardar el UniqueCodigo del producto en la venta
6. RN5 → Descontar la cantidad del stock ERP
7. GuardarCambiosAsync() → la venta y el descuento de stock se guardan juntos o no se guarda nada
```

---

## Decisiones de diseño

### Arquitectura

**1. Cinco capas en lugar de cuatro.** La capa de datos se dividió en **Infrastructure** (EF Core, `DbContext`, Fluent API, migraciones) y **Repositories** (repositorios y UnitOfWork). Así el detalle técnico de la base queda separado del contrato de acceso a datos que usan los servicios.

**2. Services no puede ver el `DbContext`, y el compilador lo garantiza.** Repositories referencia a Infrastructure con `PrivateAssets="all"`, así que esa referencia no se propaga. Se comprobó que un proyecto que solo referencia a Repositories no puede usar `FarmacorpDbContext` (error `CS0234`). La regla no depende de la disciplina de quien programa.

**3. Cada capa registra sus propias dependencias.** Siguiendo el enfoque de [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture), cada proyecto expone un método `Add...Services()` en su `DependencyInjection.cs`. `Program.cs` no conoce ninguna clase concreta de las capas.

**4. Sin `IDesignTimeDbContextFactory`.** `dotnet ef` usa el host de la consola para obtener el `DbContext`. Así la cadena de conexión vive en un solo lugar (`appsettings.json`), y las migraciones nunca apuntan a una base distinta de la que usa la aplicación.

**5. Repository y UnitOfWork sobre EF Core.** EF Core ya implementa ambos patrones: `DbContext` funciona como unidad de trabajo y `DbSet<T>` como repositorio. Se incluyeron porque el enunciado los pide explícitamente, y porque permiten aislar a Services de EF Core (punto 2).

### Modelo de datos

**6. Nombres idénticos al diagrama.** Las entidades, tablas, columnas y claves siguen el diagrama del enunciado (`ExpProductos`, `ErpProductos`, `VentaExpress`, `IdDetalle`…), para que se puedan comparar uno a uno. Las columnas que en el diagrama no están en negrita son opcionales.

**7. `ErpProducto` comparte la clave con `ExpProducto`.** `IdProducto` es a la vez su clave primaria y su clave foránea, como indica el diagrama (PK, FK1). Es una relación 1 a 0..1: el ERP extiende al producto Express sin modificar su tabla.

**8. `UniqueCodigo` como `Guid`.** Los códigos únicos del producto y del código de barras se generan con `Guid.NewGuid()` y tienen un índice único. Un GUID se puede generar sin consultar la base ni mantener un contador, y no choca entre registros. No se usó GUID v7 porque .NET 8 no lo trae, y como la clave clusterizada de las tablas es un `int`, el orden de estos códigos no afecta el rendimiento.

**9. La venta guarda una copia del nombre y el precio.** `VentaExpress.Producto` y `VentaExpress.Precio` guardan los valores del momento de la venta, como sugiere el diagrama. Si el producto cambia de nombre o de precio, la venta sigue mostrando lo que realmente se cobró.

**10. `Descuento` guarda el monto, y `NULL` cuando no aplica.** No se guarda el porcentaje, porque depende de la estrategia activa en ese momento. El monto deja constancia de cuánto se descontó de verdad.

**11. Restricciones en la base como última barrera.**

- Un índice único en (`IdProducto`, `IdCategoria`) impide asignar dos veces la misma categoría. Sin él, un producto con una sola categoría real contaría como dos y perdería el descuento de la RN3.
- Un `CHECK (Stock >= 0)` impide el stock negativo.
- Las ventas, los tipos y las categorías padre usan borrado restringido, para no perder historial.

**12. Datos iniciales con un seeder, no con `HasData`.** `FarmacorpDbContextSeeder` aplica las migraciones y carga los datos al arrancar. Busca cada dato por su descripción antes de insertarlo, así se puede ejecutar en cada arranque sin duplicar nada, y las subcategorías apuntan a su padre sin Ids fijos. La migración solo describe el esquema.

### Reglas de negocio

**13. Las estrategias implementan una interfaz y se cambian en caliente.** `IEstrategiaNegocio` contiene **solo lo que cambia** entre Base y GanaMax: el precio (RN1), el porcentaje de la RN3 y la validación de stock (RN4). Las reglas que no cambian (RN2, RN5 y la condición de una única categoría) viven una sola vez en los servicios, sin `if`s por estrategia.

`EstrategiaService` es una única instancia para toda la aplicación y guarda cuál estrategia está activa. Arranca con la del `appsettings.json` y la opción 6 del menú la cambia. `IEstrategiaNegocio` se resuelve **por operación** a partir de la estrategia activa, así cada venta o registro usa la estrategia vigente al empezar, y un cambio nunca la alcanza a mitad de camino. Los servicios de venta y de producto no saben que la estrategia puede cambiar: siguen recibiendo un `IEstrategiaNegocio` por constructor.

Si la clave del `appsettings.json` falta se usa Base. Si tiene un valor desconocido, la aplicación no arranca, porque es mejor no arrancar que vender con una regla que nadie eligió.

**14. Redondeo a 2 decimales.** Precios y descuentos se redondean con `MidpointRounding.AwayFromZero`, como en caja.

**15. Errores de negocio con `ReglaNegocioException`.** Cuando algo no está permitido (stock insuficiente, categoría repetida, producto inexistente), el servicio lanza esta excepción con un mensaje listo para mostrar. La consola la distingue de una falla inesperada. Los servicios validan antes de guardar, así que una operación rechazada no deja nada a medias.

**16. Entrada y salida con DTOs.** Cada método de servicio recibe un único DTO (`...SolicitudDTO`) y devuelve otro. La consola nunca recibe entidades del dominio. `Precio` y `UniqueCodigo` no forman parte de ninguna solicitud, porque los calcula el sistema.

### Presentación

**17. Un scope de DI por opción del menú.** Cada operación tiene su propio `DbContext`, así un error no arrastra cambios pendientes a la siguiente. Los errores se manejan en un solo lugar (`MenuPrincipal`).

**18. Pensada para usarla sin conocer los Ids.** Antes de pedir un Id se muestran los productos y categorías disponibles. Los datos con formato inválido se vuelven a pedir, los montos aceptan coma o punto, y escribir `x` en cualquier dato vuelve al menú sin guardar nada.

---

## Interpretaciones del enunciado

Algunos puntos del enunciado admiten más de una lectura. Estas son las elegidas:

| Punto del enunciado                                                       | Interpretación                                                                                                                                                                                                                      |
| ------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| "Margen del 50 % / 80 % del costo"                                        | Precio = Costo × 1.50 (o × 1.80). El margen se suma al costo.                                                                                                                                                                       |
| "Asociado a una única categoría"                                          | Se cuentan las categorías asignadas directamente al producto. Un producto asignado solo a _Analgésicos_ (subcategoría de _Medicamentos_) tiene **una** categoría y recibe el descuento. Con cero o con dos o más, no hay descuento. |
| Descuento de la RN3                                                       | Se aplica sobre el subtotal (precio × cantidad).                                                                                                                                                                                    |
| "La venta es realizada para un único producto"                            | Cada venta es de un solo producto, pero puede ser de varias unidades.                                                                                                                                                               |
| GanaMax: "deja un Stock por encima de 10 unidades"                        | El stock restante debe ser **mayor que 10**. Si quedan 11, se permite. Si quedan 10, se rechaza.                                                                                                                                    |
| GanaMax "durante 3 meses, permitiendo volver en cualquier momento a Base" | Se cambia desde el menú en cualquier momento, sin reiniciar. No se activa ni desactiva sola por fecha, para que los precios no cambien sin que el negocio lo decida. El `appsettings.json` solo define la estrategia de arranque. |
| Cambiar de estrategia con productos ya registrados                        | La RN1 se aplica **al registrar** el producto, como dice el enunciado. Los precios ya registrados no se recalculan. La RN3 y la RN4 sí usan la estrategia activa al momento de cada venta.                                        |
| Validaciones no pedidas                                                   | Se agregaron por sentido común: no se vende un producto inactivo ni uno sin ficha ERP (no tiene stock que validar ni descontar), no se asigna una categoría inactiva, el costo y la cantidad deben ser mayores a cero, el costo admite como máximo 2 decimales (si no, la RN1 calcularía el precio con un costo distinto al que guarda la base) y los textos no pueden superar el largo de su columna. |

---

## Cómo se verificó

Se probó de punta a punta, desde la consola, contra **SQL Server 2025 en Docker**, usando la misma migración y el mismo seeder que se ejecutan con LocalDB. Los casos de borde de las reglas (dos categorías, stock en cero, cantidades inválidas) se ejercitaron además con un arnés de pruebas local sobre el proveedor en memoria de EF Core. Ese arnés no forma parte de esta entrega.

| Escenario                                                    | Resultado                                                                                            |
| ------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------- |
| Registro con costo Bs 10.50 (Base)                           | Precio Bs 15.75 (RN1) y `UniqueCodigo` generado                                                      |
| Venta de 2 unidades a Bs 15.75 con una sola categoría (Base) | Subtotal Bs 31.50, descuento Bs 9.45 (30 %), total Bs 22.05. Stock de 20 a 18 (RN3, RN5)             |
| Venta de un producto sin categorías                          | Sin descuento                                                                                        |
| Venta de un producto con dos categorías                      | Sin descuento                                                                                        |
| Venta de 999 unidades con 18 en stock                        | Rechazada, sin cambios en la base (RN4)                                                              |
| Registro con costo Bs 10 (GanaMax)                           | Precio Bs 18.00 (RN1)                                                                                |
| Venta de 5 unidades a Bs 18 con una categoría (GanaMax)      | Descuento Bs 9.00 (10 %)                                                                             |
| Venta que dejaría exactamente 10 unidades (GanaMax)          | Rechazada (RN4)                                                                                      |
| Venta guardada                                               | `UniqueProducto` igual al `UniqueCodigo` del producto (RN2) y `Descuento` en `NULL` cuando no aplica |
| Categoría asignada dos veces                                 | Rechazada con un mensaje claro                                                                       |
| Dos arranques seguidos                                       | El seeder no duplica tipos ni categorías                                                             |
| En una misma sesión: Base → GanaMax → Base desde la opción 6 | Cada registro y venta usó la estrategia activa en ese momento, sin reiniciar                          |
| Costo con 3 decimales (Bs 10.555)                            | Rechazado con un mensaje claro                                                                       |
| Nombre de producto o de cliente de 200 caracteres            | Rechazado con un mensaje claro, sin llegar a SQL Server                                              |
| `"Estrategia": "Otra"`                                       | La aplicación no arranca y explica el valor esperado                                                 |
| Cancelar con `x` a mitad de una operación                    | No se guarda nada                                                                                    |
| `dotnet ef migrations has-pending-model-changes`             | El modelo coincide con la migración                                                                  |

---

## Uso del sistema

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
 6. Cambiar estrategia (Base / GanaMax)
 0. Salir
=========================================
Seleccione una opción:
```

- La opción 6 cambia la estrategia en el acto. La siguiente operación ya usa la nueva estrategia.

- Las opciones que piden un Id muestran antes los productos y categorías disponibles.
- Los montos aceptan `10,50` o `10.50`. Las fechas se escriben `dd/mm/aaaa`.
- Escribir `x` en cualquier dato vuelve al menú sin guardar nada.

### Ejemplo de venta

Paracetamol 500mg, con costo Bs 10.50, una sola categoría (Analgésicos) y estrategia Base:

```
--- Registrar venta ---
(Escribe 'x' en cualquier dato para volver al menú)
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

Si una regla no se cumple, se muestra el motivo y no se guarda nada:

```
Stock insuficiente: hay 18 unidades y se pidieron 999.
Con la estrategia GanaMax la venta debe dejar más de 10 unidades en stock, y quedarían 10.
```

---

## Migraciones de base de datos

La migración `InitialCreate` está en `FarmacorpPOS.Infrastructure/Data/Migrations` y la aplicación la aplica sola al arrancar. Para manejarlas a mano hace falta la herramienta `dotnet-ef` (`dotnet tool install --global dotnet-ef`):

```bash
# Aplicar migraciones
dotnet ef database update --project FarmacorpPOS.Infrastructure --startup-project FarmacorpPOS.ConsoleApp

# Crear una migración nueva después de cambiar el modelo
dotnet ef migrations add <NombreMigracion> --project FarmacorpPOS.Infrastructure --startup-project FarmacorpPOS.ConsoleApp --output-dir Data/Migrations
```

---

## Autor

**Ismael MP** — [@yzkael](https://github.com/yzkael)
