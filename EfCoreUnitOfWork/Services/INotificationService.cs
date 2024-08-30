using Ardalis.Result;
using EfCoreUnitOfWork.Entities;

namespace EfCoreUnitOfWork.Services
{
    public interface INotificationService
    {
        Task<Result<NotificationEntity>> AddAsync(string text, CancellationToken cancellationToken = default);
        Task<Result<NotificationEntity>> GetAsync(int id, CancellationToken cancellationToken = default);
    }
}