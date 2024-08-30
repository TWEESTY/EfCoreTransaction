namespace EfCoreUnitOfWork.Repositories
{
    public interface IUnitOfWorkManager
    {
        IUnitOfWork StartOneUnitOfWork();

        void EndOneUnitOfWork();
    }
}
