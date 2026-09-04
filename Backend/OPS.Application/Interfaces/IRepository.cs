using OPS.Domain.Common;

namespace OPS.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    void Create(T entity);
    void Update(T entity);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    IQueryable<T> Query();
}
