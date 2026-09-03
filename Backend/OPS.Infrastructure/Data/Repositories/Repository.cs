using Microsoft.EntityFrameworkCore;
using OPS.Application.Common.Interfaces;
using OPS.Domain.Common;

namespace OPS.Infrastructure.Data.Repositories;

public class Repository<T>(AppDbContext context) : IRepository<T>
    where T : BaseEntity
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Create(T entity)
    {
        _dbSet.Add(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }
}
