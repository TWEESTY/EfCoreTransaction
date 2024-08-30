using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace EfCoreUnitOfWork.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        public Task EndAsync();
    }
}
