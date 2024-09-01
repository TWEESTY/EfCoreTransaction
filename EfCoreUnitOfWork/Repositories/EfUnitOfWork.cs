using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EfCoreUnitOfWork.Repositories
{
    // NOT THREAD SAFE LIKE DBCONTEXT
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly List<DbContext> _dbContexts;
        private IDbContextTransaction? _currentTransaction;
        private readonly bool _isParent;
        private readonly EfUnitOfWorkManager _unitOfWorkManager;

        public EfUnitOfWork(EfUnitOfWorkManager unitOfWorkManager, List<DbContext> dbContexts, bool isParent = false)
        {
            _dbContexts = dbContexts;
            _isParent = isParent;
            _unitOfWorkManager = unitOfWorkManager;

            if (isParent && _dbContexts.First().Database.CurrentTransaction == null)
            {
                foreach (DbContext dbContext in dbContexts)
                {
                    if (_currentTransaction is null)
                        _currentTransaction = dbContext.Database.BeginTransaction();
                    else
                        dbContext.Database.UseTransaction(_currentTransaction.GetDbTransaction());
                }
            }

        }

        public void Dispose()
        {
            try
            {
                if (_isParent && _currentTransaction != null)
                {
                    _currentTransaction.Rollback();
                    _dbContexts.ForEach(context => context.ChangeTracker.Clear());
                }
            }
            finally
            {
                _unitOfWorkManager.EndOneUnitOfWork();
            }
        }

        public async Task EndAsync()
        {
            if (!_isParent || _currentTransaction == null)
                return;

            try
            {
                await _currentTransaction.CommitAsync();
            }
            catch
            {
                // TODO logging
                _currentTransaction?.Rollback();
                _dbContexts.ForEach(context => context.ChangeTracker.Clear());
                throw;
            }
            finally
            {
                _currentTransaction = null;
            }

        }
    }
}
