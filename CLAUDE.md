# CLAUDE.md

Prueba técnica de Farmacorp: un POS de consola en .NET 8 / C#. El enunciado completo (reglas de negocio y diagrama de tablas) está en el PDF de la evaluación; el README resume ambos.

## Comandos

```bash
dotnet build FarmacorpPOSExpress.sln
dotnet run --project FarmacorpPOS.ConsoleApp
DOTNET_ROLL_FORWARD=Major dotnet ef migrations add <Nombre> --project FarmacorpPOS.Infrastructure --startup-project FarmacorpPOS.ConsoleApp --output-dir Data/Migrations
DOTNET_ROLL_FORWARD=Major dotnet ef database update --project FarmacorpPOS.Infrastructure --startup-project FarmacorpPOS.ConsoleApp
```

- El proyecto apunta a `net8.0`. Las plantillas del SDK instalado no ofrecen net8.0, así que al crear un proyecto nuevo hay que cambiar el `TargetFramework` a mano en el `.csproj`.
- La solución usa el formato `.sln`, no `.slnx` (`dotnet new sln --format sln`).
- En esta máquina solo están los runtimes 9 y 10, por eso los comandos de EF llevan `DOTNET_ROLL_FORWARD=Major`.
- No hay `IDesignTimeDbContextFactory`, igual que en [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture). `dotnet ef` arranca la consola y toma el `DbContext` del mismo contenedor de DI que usa la app. Así la cadena de conexión vive en un solo lugar (`appsettings.json`) y las migraciones nunca apuntan a otra base. Por eso:
  - La consola tiene que armarse con `Host.CreateApplicationBuilder`, porque es lo que `dotnet ef` sabe interceptar.
  - El paquete `Microsoft.EntityFrameworkCore.Design` va en la consola (proyecto de inicio), no en Infrastructure.
  - Mientras la consola no exista no se pueden generar migraciones nuevas.
- LocalDB solo existe en Windows. En Linux se pueden generar migraciones (`migrations add` y `migrations script` no se conectan a la base), pero para aplicarlas hace falta Windows o SQL Server en Docker.

## Arquitectura

Arquitectura N-Capas con cinco proyectos. Cada capa solo conoce a la de abajo:

```
FarmacorpPOS.ConsoleApp      → Presentación: Controllers que leen la consola y llaman a los servicios. Program.cs arma el host y llama al `Add...Services()` de cada capa
FarmacorpPOS.Services        → Transacciones y reglas de negocio, incluidas las estrategias Base y GanaMax
FarmacorpPOS.Repositories    → IRepository<T>, Repository<T>, IUnitOfWork, UnitOfWork
FarmacorpPOS.Infrastructure  → Data/: DbContext, configuraciones Fluent API y migraciones
FarmacorpPOS.Domain          → Entidades. No depende de ningún otro proyecto ni paquete
```

Referencias entre proyectos:

| Proyecto       | Referencia a                                  |
|----------------|-----------------------------------------------|
| Infrastructure | Domain                                        |
| Repositories   | Infrastructure, Domain                        |
| Services       | Repositories, Domain                          |
| ConsoleApp     | Services, Repositories e Infrastructure (solo para registrar todo en el contenedor de DI) |

Por qué está separado así:

- **Infrastructure es lo único que sabe de EF Core y SQL Server.** Si mañana cambia la base o el ORM, el cambio se queda en esta capa.
- **Repositories separa "cómo se guarda" de "qué se guarda".** Los servicios piden y guardan entidades sin saber que detrás hay un `DbContext`.
- **Services no ve el `DbContext`.** Solo trabaja con `IUnitOfWork`, así las reglas de negocio se pueden probar sin base de datos. Repositories referencia a Infrastructure con `PrivateAssets="all"` para que el `DbContext` no le llegue a Services por referencia transitiva.
- **Los Controllers no tienen lógica de negocio.** Leen y validan el formato de lo que escribe el usuario, arman el DTO, llaman a un servicio y muestran el resultado. Si una regla aparece en un Controller, va en Services.

Reglas que aplican a todas las capas:

- **Domain no referencia a EF Core.** Las entidades son clases simples, sin atributos `[Key]`, `[Required]`, etc. Todo el mapeo va en Infrastructure con `IEntityTypeConfiguration<T>`, una clase por entidad. Es requisito de la prueba usar Fluent API.
- **Una transacción de negocio = un `GuardarCambiosAsync()`.** Por ejemplo, la venta y el descuento de stock se confirman juntos, porque si uno falla el otro no debe quedar guardado.
- **Inyección por constructor** con `Microsoft.Extensions.DependencyInjection`. Nada de `new` sobre servicios ni repositorios fuera de `Program.cs`. La única excepción es `UnitOfWork`, que crea sus repositorios porque todos tienen que compartir su mismo `DbContext`. Los servicios piden `IUnitOfWork` y nunca un repositorio suelto.
- **Datos iniciales con `FarmacorpDbContextSeeder`, no con `HasData`.** Al arrancar, la consola llama a `host.SeedDatabaseAsync()`, que aplica las migraciones pendientes y luego carga los tipos de producto y las categorías mínimas para probar las transacciones. Así quien clone el repo solo tiene que ejecutar la app.
  - Cada dato se busca por su descripción antes de insertarlo, así se puede correr en cada arranque sin duplicar nada. Si mañana se agrega un tipo o una categoría a la lista, solo se inserta lo que falta.
  - Las categorías padre se guardan primero y las subcategorías apuntan a ellas por navegación, sin depender de Ids fijos.
  - Las migraciones solo describen el esquema; los datos no viven en ellas.
- **Cada capa registra sus propias dependencias** en un `DependencyInjection.cs` en la raíz del proyecto, con un método de extensión sobre `IHostApplicationBuilder`: `AddInfrastructureServices()`, `AddRepositoryServices()`, `AddBusinessServices()` y `AddPresentationServices()`. `Program.cs` solo los llama en orden y no conoce las clases concretas de ninguna capa. Estas clases van en el namespace `Microsoft.Extensions.DependencyInjection` para que no haga falta un `using` por capa, y sus nombres siguen la convención de .NET en inglés (`Add...Services`).
- **Las interfaces viven al lado de su implementación** (`IRepository.cs` junto a `Repository.cs`, `IVentaService.cs` junto a `VentaService.cs`), así al abrir una carpeta se ve el contrato y quien lo cumple.

## Capa de presentación (consola)

- **`MenuPrincipal` crea un scope de DI por opción.** Cada operación tiene su propio `DbContext`, así lo que quedó a medias en una operación fallida no se arrastra a la siguiente.
- **Los errores se manejan una sola vez, en `MenuPrincipal`,** y no con un `try/catch` en cada Controller.
- **Toda lectura de datos pasa por `Utilidades/Entrada`.** Cada método insiste hasta recibir un valor con formato válido: los montos aceptan coma o punto y las fechas van en `dd/mm/aaaa`. Si la entrada se termina (Ctrl+D o un archivo redirigido), lanza `OperationCanceledException` y la app sale limpiamente en vez de quedar pidiendo datos.
- **Escribir `x` en cualquier dato vuelve al menú.** `Entrada` lanza `VolverAlMenuException` y `MenuPrincipal` la atrapa. Nunca deja nada a medias, porque los servicios solo guardan al final. Por eso los Controllers deben leer todos los datos antes de llamar a un servicio que guarda, y cada opción que pide datos muestra `Pantalla.ComoVolver()` debajo del título.
- **Las listas que se repiten se dibujan en `Utilidades/Tablas`.** Por ejemplo, la tabla de productos se muestra antes de pedir un Id.
- **La cultura es `InvariantCulture`,** así los montos y las fechas se ven igual en cualquier máquina.
- **El `ContentRootPath` es la carpeta del ejecutable,** para que `appsettings.json` se encuentre sin importar desde dónde se lance la app.
- **El log está en `Warning`.** Si no, EF escribiría cada consulta SQL en medio del menú.
- **Para probar contra otra base sin tocar `appsettings.json`,** se usan variables de entorno. `ConnectionStrings__FarmacorpDb` cambia la conexión (por ejemplo, a un SQL Server en Docker) y `Estrategia` cambia la estrategia.

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
- La misma separación se repite en las capas de arriba cuando aplique (por ejemplo, `Infrastructure/Data/Configurations/Express/Ventas` y `Services/Erp`).

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
- Los `UniqueCodigo` (y `VentaExpress.UniqueProducto`) son `Guid`, con índice único. Se generan en Services con `Guid.NewGuid()` y nunca los ingresa el usuario. No usamos GUID v7: .NET 8 no lo trae y no queremos un generador propio.

## Reglas de negocio y estrategias

El sistema tiene que poder cambiar entre la estrategia **Base** y **GanaMax** en cualquier momento, sin tocar código. Las reglas que cambian van detrás de una abstracción (`Services/Estrategias`): una estrategia por modo, elegida con la clave `Estrategia` del `appsettings.json` (`Base` o `GanaMax`) e inyectada por DI. Nada de `if`s por estrategia dentro de los servicios.

| Regla | Base | GanaMax |
|-------|------|---------|
| RN1 – Precio al registrar producto | Costo × 1.50 | Costo × 1.80 |
| RN2 – Guardar `UniqueCodigo` en la venta | Sí | Igual |
| RN3 – Descuento si el producto tiene **exactamente una** categoría | 30 % | 10 % |
| RN4 – Validación de stock | Stock disponible | Stock disponible **y** que después de la venta queden más de 10 unidades |
| RN5 – Descontar stock en el ERP al vender | Sí | Igual |

La venta es siempre de un único producto.

- `IEstrategiaNegocio` solo contiene lo que cambia entre estrategias (RN1, el porcentaje de RN3 y RN4). La condición de RN3 (exactamente una categoría), RN2 y RN5 son iguales en ambas y viven en los servicios.
- `AddBusinessServices()` es el único lugar que sabe qué estrategias existen. Si falta la clave `Estrategia`, usa Base; si tiene un valor desconocido, la app no arranca, porque es mejor no arrancar que vender con una regla que nadie eligió.
- Los montos se redondean a 2 decimales con `MidpointRounding.AwayFromZero`, que es como se redondea en caja.

## Convenciones de la capa de servicios

- **Errores de negocio con `ReglaNegocioException`.** Cuando el usuario pide algo que el negocio no permite (stock insuficiente, producto inexistente, categoría repetida), el servicio lanza esta excepción con un mensaje listo para mostrar. `MenuPrincipal` muestra ese mensaje tal cual y trata cualquier otra excepción como una falla inesperada (la registra en el log y muestra un aviso genérico).
- **Entrada y salida con `record`, en `Services/DTOs/`.** La consola nunca recibe entidades, así puede cambiar el modelo sin romper la presentación.
  - **Todo método público de un servicio que recibe datos los recibe en un único DTO**, nunca parámetros sueltos como `int idProducto`. Las consultas sin filtros (`ListarAsync()`) no llevan parámetros. Así la firma dice qué operación es, y agregar un dato más no rompe a quien la llama.
  - **Todos los nombres terminan en `DTO`**, para distinguirlos a simple vista de las entidades del dominio. Los de entrada se llaman `...SolicitudDTO` (ej. `RegistrarVentaSolicitudDTO`, `AsignarCategoriaSolicitudDTO`) y los de salida describen el resultado (ej. `VentaRegistradaDTO`).
  - Los DTOs no van junto a los servicios, sino en `DTOs/` con las mismas subcarpetas de módulo y área (ej. `DTOs/Express/Ventas/`, namespace `FarmacorpPOS.Services.DTOs.Express.Ventas`). Así las carpetas de servicios solo tienen la interfaz y su implementación, y todos los contratos de datos se encuentran en un solo lugar.
- **Lo que calcula el sistema no viene en la solicitud.** `Precio` y `UniqueCodigo` no son parámetros, porque los define la Regla 1 y la generación automática, nunca quien registra.
- **Validar antes que la base.** Aunque haya un índice único o un check constraint, el servicio valida primero para que el usuario reciba un mensaje claro y no un error de SQL. Las restricciones de la base quedan como última barrera.
- **Cada servicio guarda una sola vez,** al final y solo si todo se cumplió.

## Estilo de código

- Código, nombres y comentarios en español, igual que el dominio del enunciado.
- Los comentarios explican el **porqué**, no el cómo. Escritos con naturalidad, como se los explicarías a un compañero. Si la línea se entiende sola, no lleva comentario. Nada de `/// <summary>` que repitan el nombre de la clase o de la tabla, ni etiquetas como `// Navegación`.
- `Nullable` e `ImplicitUsings` habilitados. Usa namespaces de archivo (`namespace X;`).
- Las colecciones de navegación se inicializan con `new List<T>()`, y las referencias obligatorias con `= null!`.
