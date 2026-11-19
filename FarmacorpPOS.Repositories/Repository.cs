using System.Linq.Expressions;
using FarmacorpPOS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmacorpPOS.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly FarmacorpDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(FarmacorpDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> ObtenerPorIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IReadOnlyList<T>> ObtenerTodosAsync() => await _dbSet.ToListAsync();

    public async Task<IReadOnlyList<T>> BuscarAsync(Expression<Func<T, bool>> filtro) =>
        await _dbSet.Where(filtro).ToListAsync();

    public Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro) => _dbSet.AnyAsync(filtro);

    public Task<int> ContarAsync(Expression<Func<T, bool>> filtro) => _dbSet.CountAsync(filtro);

    public void Agregar(T entidad) => _dbSet.Add(entidad);

    // Marca solo esta entidad. DbSet.Update() recorrería también sus relaciones
    // y terminaría guardando entidades que nadie pidió actualizar.
    public void Actualizar(T entidad) => _context.Entry(entidad).State = EntityState.Modified;

    public void Eliminar(T entidad) => _dbSet.Remove(entidad);
}
