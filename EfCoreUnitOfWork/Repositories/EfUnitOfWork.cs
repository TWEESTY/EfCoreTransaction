using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EfCoreUnitOfWork.Repositories
{
    // NOT THREAD SAFE LIKE DBCONTEXT
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;
        private IDbContextTransaction? _currentTransaction;
        private readonly bool _isParent;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public EfUnitOfWork(IUnitOfWorkManager unitOfWorkManager, DbContext dbContext, bool isParent = false)
        {
            _dbContext = dbContext;
            _isParent = isParent;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task StartAsync()
        {
            if (_isParent && _dbContext.Database.CurrentTransaction == null)
                _currentTransaction = await _dbContext.Database.BeginTransactionAsync();
        }

        public Task EndAsync(bool forceRollback = false)
        {
            if (forceRollback)
                return DoRollbackIfNecessaryAsync();

            return DoCommitIfNecessaryAsync();

        }

        private async Task DoRollbackIfNecessaryAsync()
        {
            if (_isParent && _currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                _dbContext.ChangeTracker.Clear();
            }
        }

        private async Task DoCommitIfNecessaryAsync()
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
                _dbContext.ChangeTracker.Clear();
                throw;
            }
            finally
            {
                _currentTransaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_currentTransaction is not null)
                await _unitOfWorkManager.EndUnitOfWorkAsync(this, forceRollback: true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }
}
