namespace EfCoreUnitOfWork.Repositories
{
    public interface IUnitOfWork
    {
        Task DoWorkAsync(Func<Task> function);

        Task<T> DoWorkAsync<T>(Func<Task<T>> function);
    }
}
