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
        private readonly bool _isParent;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public EfUnitOfWork(IUnitOfWorkManager unitOfWorkManager, DbContext dbContext, bool isParent = false)
        {
            _dbContext = dbContext;
            _isParent = isParent;
            _unitOfWorkManager = unitOfWorkManager;
            
            if(isParent && _dbContext.Database.CurrentTransaction == null)
                _currentTransaction = _dbContext.Database.BeginTransaction();

        }

        public void Dispose()
        {
            try
            {
                if (_isParent && _currentTransaction != null)
                {
                    _currentTransaction.Rollback();
                    _dbContext.ChangeTracker.Clear();
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
                throw;
            }
            finally
            {
                _currentTransaction = null;
            }

        }
    }
}
