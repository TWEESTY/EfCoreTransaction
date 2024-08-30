namespace EfCoreUnitOfWork.Repositories
{
    public interface IRepository<T> where T : class
    {
        public T Add(T entity);

        public ValueTask<T?> GetByIdAsync<Type>(Type id, CancellationToken cancellationToken = default);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
