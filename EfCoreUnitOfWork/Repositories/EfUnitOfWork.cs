
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EfCoreUnitOfWork.Repositories
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;

        public EfUnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DoWorkAsync(Func<Task> function)
        {
            IDbContextTransaction? transaction = null;
            bool hasToCreateTransaction = _dbContext.Database.CurrentTransaction == null;

            try
            {
                if (hasToCreateTransaction)
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync();
                }


                await function();

                if (hasToCreateTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                }
            }
            catch
            {

                if (hasToCreateTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                    await transaction.DisposeAsync();
                }

                throw;
            }
        }

        public async Task<T> DoWorkAsync<T>(Func<Task<T>> function)
        {
            IDbContextTransaction? transaction = null;
            bool hasToCreateTransaction = _dbContext.Database.CurrentTransaction == null;

            try
            {
                if (hasToCreateTransaction)
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync();
                }


                T result = await function();

                if (hasToCreateTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                }

                return result;
            }
            catch
            {

                if (hasToCreateTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                    await transaction.DisposeAsync();
                }

                throw;
            }
        }
    }
}
