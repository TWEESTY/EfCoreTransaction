using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Data.Common;

namespace EfCoreUnitOfWork.Repositories
{
    // NOT THREAD SAFE LIKE DBCONTEXT
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;
        private IDbContextTransaction? _currentTransaction;
        private int countOfBeginCalls = 0;

        public EfUnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void Begin(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_dbContext.Database.CurrentTransaction == null)
            {
                _currentTransaction = _dbContext.Database.BeginTransaction();
                countOfBeginCalls++;
            }
        }

        public void Dispose()
        {
            if(_currentTransaction != null)
            {
                _currentTransaction.Rollback();
            }
        }

        public async Task EndAsync()
        {
            if (countOfBeginCalls != 1)
            {
                countOfBeginCalls--;
                return;
            }

            countOfBeginCalls--;

            try
            {
                if(_currentTransaction != null)
                    await _currentTransaction.CommitAsync();
            }
            catch
            {
                // TODO logging
                _currentTransaction?.Rollback();
                throw;
            }
            finally
            {
                _currentTransaction = null;
            }

        }
    }
}
