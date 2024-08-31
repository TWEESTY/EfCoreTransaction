namespace EfCoreUnitOfWork.Repositories
{
    public interface IUnitOfWorkManager
    {
        IUnitOfWork StartOneUnitOfWork();
    }
}
