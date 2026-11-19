using FarmacorpPOS.Domain.Entities.Express.Ventas;
using FarmacorpPOS.Repositories.UnitsOfWork;
using FarmacorpPOS.Services.DTOs.Express.Ventas;
using FarmacorpPOS.Services.Estrategias;
using FarmacorpPOS.Services.Excepciones;

namespace FarmacorpPOS.Services.Express.Ventas;

public class VentaService : IVentaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEstrategiaNegocio _estrategia;

    public VentaService(IUnitOfWork unitOfWork, IEstrategiaNegocio estrategia)
    {
        _unitOfWork = unitOfWork;
        _estrategia = estrategia;
    }

    public async Task<VentaRegistradaDTO> RegistrarAsync(RegistrarVentaSolicitudDTO solicitud)
    {
        if (solicitud.Cantidad <= 0)
        {
            throw new ReglaNegocioException("La cantidad debe ser mayor a cero.");
        }

        var producto = await _unitOfWork.ExpProductos.ObtenerPorIdAsync(solicitud.IdProducto)
            ?? throw new ReglaNegocioException($"No existe el producto {solicitud.IdProducto}.");

        if (!producto.Activo)
        {
            throw new ReglaNegocioException($"El producto '{producto.Nombre}' está inactivo y no se puede vender.");
        }

        // Sin ficha ERP no hay stock que validar ni descontar, así que no hay
        // forma de cumplir las Reglas 4 y 5.
        var erp = await _unitOfWork.ErpProductos.ObtenerPorIdAsync(solicitud.IdProducto)
            ?? throw new ReglaNegocioException($"El producto '{producto.Nombre}' no está registrado en el ERP y no tiene stock.");

        // Regla 4
        _estrategia.ValidarStock(erp.Stock, solicitud.Cantidad);

        var subtotal = producto.Precio * solicitud.Cantidad;

        // Regla 3: "si y solo si" una única categoría. Con cero o con dos o más
        // categorías no hay descuento.
        var cantidadCategorias = await _unitOfWork.ProductosCategorias.ContarAsync(pc => pc.IdProducto == producto.IdProducto);
        var descuento = cantidadCategorias == 1
            ? Math.Round(subtotal * _estrategia.PorcentajeDescuentoCategoriaUnica, 2, MidpointRounding.AwayFromZero)
            : 0m;

        var venta = new VentaExpress
        {
            Fecha = DateTime.Now,
            Cliente = string.IsNullOrWhiteSpace(solicitud.Cliente) ? null : solicitud.Cliente.Trim(),
            Producto = producto.Nombre,
            UniqueProducto = erp.UniqueCodigo, // Regla 2
            Cantidad = solicitud.Cantidad,
            Precio = producto.Precio,
            // Null cuando no aplica, para distinguir "sin descuento" de un descuento calculado.
            Descuento = descuento > 0 ? descuento : null,
            Total = subtotal - descuento,
            IdProducto = producto.IdProducto,
        };

        // Regla 5
        erp.Stock -= solicitud.Cantidad;

        _unitOfWork.VentasExpress.Agregar(venta);
        _unitOfWork.ErpProductos.Actualizar(erp);

        // Un solo guardado: si falla, no queda ni la venta sin descontar stock
        // ni el stock descontado sin venta.
        await _unitOfWork.GuardarCambiosAsync();

        return new VentaRegistradaDTO(
            venta.Id,
            venta.Producto,
            erp.UniqueCodigo,
            venta.Cantidad,
            venta.Precio,
            subtotal,
            descuento,
            venta.Total,
            erp.Stock,
            _estrategia.Nombre);
    }
}
