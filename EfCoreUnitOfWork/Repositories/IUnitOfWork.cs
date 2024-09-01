namespace EfCoreUnitOfWork.Repositories
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        public Task StartAsync();
        public Task EndAsync(bool forceRollback = false);
    }
}
