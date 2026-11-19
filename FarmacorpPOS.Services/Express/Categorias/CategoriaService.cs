using FarmacorpPOS.Domain.Entities.Express.Categorias;
using FarmacorpPOS.Repositories.UnitsOfWork;
using FarmacorpPOS.Services.DTOs.Express.Categorias;
using FarmacorpPOS.Services.Excepciones;

namespace FarmacorpPOS.Services.Express.Categorias;

public class CategoriaService : ICategoriaService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoriaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoriaAsignadaDTO> AsignarAsync(AsignarCategoriaSolicitudDTO solicitud)
    {
        var producto = await _unitOfWork.ExpProductos.ObtenerPorIdAsync(solicitud.IdProducto)
            ?? throw new ReglaNegocioException($"No existe el producto {solicitud.IdProducto}.");

        var categoria = await _unitOfWork.Categorias.ObtenerPorIdAsync(solicitud.IdCategoria)
            ?? throw new ReglaNegocioException($"No existe la categoría {solicitud.IdCategoria}.");

        if (!categoria.Activo)
        {
            throw new ReglaNegocioException($"La categoría '{categoria.Descripcion}' está inactiva.");
        }

        // La base ya lo impide con un índice único, pero así el usuario recibe
        // un mensaje claro en vez de un error de SQL.
        if (await _unitOfWork.ProductosCategorias.ExisteAsync(pc => pc.IdProducto == solicitud.IdProducto && pc.IdCategoria == solicitud.IdCategoria))
        {
            throw new ReglaNegocioException($"El producto '{producto.Nombre}' ya tiene asignada la categoría '{categoria.Descripcion}'.");
        }

        var asignacion = new ProductoCategoria
        {
            FechaCreacion = DateTime.Now,
            IdProducto = solicitud.IdProducto,
            IdCategoria = solicitud.IdCategoria,
        };

        _unitOfWork.ProductosCategorias.Agregar(asignacion);
        await _unitOfWork.GuardarCambiosAsync();

        var cantidadCategorias = await _unitOfWork.ProductosCategorias.ContarAsync(pc => pc.IdProducto == solicitud.IdProducto);

        return new CategoriaAsignadaDTO(asignacion.IdDetalle, producto.Nombre, categoria.Descripcion, cantidadCategorias);
    }
}
