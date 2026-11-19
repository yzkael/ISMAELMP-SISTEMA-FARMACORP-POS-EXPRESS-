using System.Linq.Expressions;

namespace FarmacorpPOS.Repositories;

// No guarda nada por sí solo: los cambios se confirman con
// IUnitOfWork.GuardarCambiosAsync(), para que una venta y su descuento de
// stock se guarden juntos o no se guarde ninguno.
public interface IRepository<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IReadOnlyList<T>> ObtenerTodosAsync();
    Task<IReadOnlyList<T>> BuscarAsync(Expression<Func<T, bool>> filtro);

    // Existen aparte de BuscarAsync para que la base haga el conteo y no
    // tengamos que traer filas solo para contarlas (ej. las categorías de la Regla 3).
    Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro);
    Task<int> ContarAsync(Expression<Func<T, bool>> filtro);

    void Agregar(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
}
