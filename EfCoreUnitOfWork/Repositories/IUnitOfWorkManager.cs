namespace EfCoreUnitOfWork.Repositories
{
    public interface IUnitOfWorkManager
    {
        Task<IUnitOfWork> StartOneUnitOfWorkAsync();

        Task EndUnitOfWorkAsync(IUnitOfWork unitOfWork, bool forceRollback = false);
    }
}
