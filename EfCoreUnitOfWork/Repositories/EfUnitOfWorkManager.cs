using Microsoft.EntityFrameworkCore;

namespace EfCoreUnitOfWork.Repositories
{
    public class EfUnitOfWorkManager : IUnitOfWorkManager
    {
        private int _numberOfUnitOfWork = 0;
        private DbContext _dbContext;

        public EfUnitOfWorkManager(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void EndOneUnitOfWork()
        {
            _numberOfUnitOfWork--;
        }

        public IUnitOfWork StartOneUnitOfWork()
        {
            return new EfUnitOfWork(this, _dbContext, isParent: _numberOfUnitOfWork++ == 0);
        }


    }
}
