using Ardalis.Result;
using EfCoreUnitOfWork.Entities;

namespace EfCoreUnitOfWork.Services
{
    public interface IPersonService
    {
        Task<Result<PersonEntity>> AddAsync(string name, CancellationToken cancellationToken = default);
        Task<Result<PersonEntity>> GetAsync(int id, CancellationToken cancellationToken = default);
        Task AddForNestedTesting(CancellationToken cancellationToken = default);

        Task AddWithTryCatchForFirstUoWForNestedTesting(CancellationToken cancellationToken = default);

    }
}