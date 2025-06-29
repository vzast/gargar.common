using System.Linq.Expressions;

namespace Gargar.Common.Application.Interfaces;

public interface IUoWService<TEntity, Tkey, TDTO, TMapper> where TDTO : class where TMapper : class, IMapper<TEntity, TDTO>, new()
{
    Task<TDTO?> GetByIdAsync(Tkey id, CancellationToken cancellationToken = default);

    Task<TDTO> AddAsync(TEntity dto, CancellationToken cancellationToken = default);

    Task<TDTO> UpdateAsync(Tkey id, object obj, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Tkey id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predecate, CancellationToken cancellationToken = default);
}