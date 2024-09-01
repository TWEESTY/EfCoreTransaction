using Ardalis.Result;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Repositories;

namespace EfCoreUnitOfWork.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<NotificationEntity> _notificationRepository;
        private readonly IFakeService _fakeService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public NotificationService(IRepository<NotificationEntity> notificationRepository, IFakeService fakeService, IUnitOfWorkManager unitOfWorkManager)
        {
            _notificationRepository = notificationRepository;
            _fakeService = fakeService;
            _unitOfWorkManager = unitOfWorkManager;
        }


        public async Task<Result<NotificationEntity>> AddAsync(string text, CancellationToken cancellationToken = default)
        {
            NotificationEntity notificationEntity;
            try
            {
                await using (IUnitOfWork unitOfWork = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
                {
                    notificationEntity = _notificationRepository.Add(new NotificationEntity { Text = text });
                    await _fakeService.DoWorkAsync();
                    await _notificationRepository.SaveChangesAsync(cancellationToken);

                    await _unitOfWorkManager.EndUnitOfWorkAsync(unitOfWork);
                }
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
