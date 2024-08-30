using EfCoreUnitOfWork.Context;

namespace EfCoreUnitOfWork.Repositories
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _appDbContext;
        public EfRepository(AppDbContext dbContext)
        {
            _appDbContext = dbContext;
        }

        public T Add(T entity)
        {
            _appDbContext.Set<T>().Add(entity);
            return entity;
        }

        public ValueTask<T?> GetByIdAsync<Type>(Type id, CancellationToken cancellationToken)
        {
            return _appDbContext.Set<T>().FindAsync(new object[1] { id }, cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
