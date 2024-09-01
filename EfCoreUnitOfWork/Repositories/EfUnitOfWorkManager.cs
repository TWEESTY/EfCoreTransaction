using Microsoft.EntityFrameworkCore;

namespace EfCoreUnitOfWork.Repositories
{
    public class EfUnitOfWorkManager : IUnitOfWorkManager
    {
        private int _numberOfUnitOfWork = 0;
        private List<DbContext> _dbContexts;

        public EfUnitOfWorkManager(List<DbContext> dbContexts)
        {
            _dbContexts = dbContexts;

        }

        public void EndOneUnitOfWork()
        {
            _numberOfUnitOfWork--;
        }

        public IUnitOfWork StartOneUnitOfWork()
        {
            return new EfUnitOfWork(this, _dbContexts, isParent: _numberOfUnitOfWork++ == 0);
        }
    }
}
