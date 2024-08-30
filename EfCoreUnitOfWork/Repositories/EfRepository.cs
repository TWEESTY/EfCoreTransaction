using EfCoreUnitOfWork.Context;
using Zejji.Entity;

namespace EfCoreUnitOfWork.Repositories
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly IAmbientDbContextLocator _contextLocator;

        public EfRepository(IAmbientDbContextLocator contextLocator)
        {
            _contextLocator = contextLocator;
        }

        public T Add(T entity)
        {
            _contextLocator.Get<AppDbContext>()!.Set<T>().Add(entity);
            return entity;
        }

        public ValueTask<T?> GetByIdAsync<Type>(Type id, CancellationToken cancellationToken)
        {
            return _contextLocator.Get<AppDbContext>()!.Set<T>().FindAsync(new object[1] { id }, cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _contextLocator.Get<AppDbContext>()!.SaveChangesAsync(cancellationToken);
        }
    }
}
