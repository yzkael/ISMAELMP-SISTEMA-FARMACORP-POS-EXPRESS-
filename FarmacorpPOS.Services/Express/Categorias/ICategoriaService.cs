using FarmacorpPOS.Services.DTOs.Express.Categorias;

namespace FarmacorpPOS.Services.Express.Categorias;

public interface ICategoriaService
{
    Task<CategoriaAsignadaDTO> AsignarAsync(AsignarCategoriaSolicitudDTO solicitud);
    Task<IReadOnlyList<CategoriaDTO>> ListarAsync();
}
