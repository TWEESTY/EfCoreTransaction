using Ardalis.Result;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Repositories;

namespace EfCoreUnitOfWork.Services
{
    public class NotificationService : INotificationService
    {
        private IRepository<NotificationEntity> _notificationRepository;
        private IFakeService _fakeService;

        public NotificationService(IRepository<NotificationEntity> notificationRepository, IFakeService fakeService)
        {
            _notificationRepository = notificationRepository;
            _fakeService = fakeService;
        }


        public async Task<Result<NotificationEntity>> AddAsync(string text, CancellationToken cancellationToken = default)
        {
            NotificationEntity notificationEntity;
            try
            {
                notificationEntity = _notificationRepository.Add(new NotificationEntity { Text = text });
                // await _notificationRepository.SaveChangesAsync(cancellationToken);
                await _fakeService.DoWorkAsync();
                await _notificationRepository.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Error(ex.Message);
            }

            return Result.Success(notificationEntity);

        }

        public async Task<Result<NotificationEntity>> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            NotificationEntity? notificationEntity = await _notificationRepository.GetByIdAsync(id, cancellationToken);

            if (notificationEntity == null)
            {
                return Result.NotFound();
            }

            return Result.Success(notificationEntity);
        }
    }
}
