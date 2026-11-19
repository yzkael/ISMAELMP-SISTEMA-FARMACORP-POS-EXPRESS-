namespace FarmacorpPOS.Services.DTOs.Erp.CodigosBarras;

// Solo pide el producto: el UniqueCodigo del código de barras lo genera el sistema.
public record AsignarCodigoBarraSolicitudDTO(int IdProducto);
