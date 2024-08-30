using Ardalis.Result;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Repositories;
using Zejji.Entity;

namespace EfCoreUnitOfWork.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<NotificationEntity> _notificationRepository;
        private readonly IFakeService _fakeService;
        private readonly IDbContextScopeFactory _dbContextScopeFactory;

        public NotificationService(IRepository<NotificationEntity> notificationRepository, IFakeService fakeService, IDbContextScopeFactory dbContextScopeFactory)
        {
            _notificationRepository = notificationRepository;
            _fakeService = fakeService;
            _dbContextScopeFactory = dbContextScopeFactory;
        }


        public async Task<Result<NotificationEntity>> AddAsync(string text, CancellationToken cancellationToken = default)
        {
            NotificationEntity notificationEntity;
            try
            {
                using (var dbContextScope = _dbContextScopeFactory.Create())
                {
                    notificationEntity = _notificationRepository.Add(new NotificationEntity { Text = text });
                    await _fakeService.DoWorkAsync();
                    dbContextScope.SaveChanges();
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
            NotificationEntity? notificationEntity;

            using (var dbContextScope = _dbContextScopeFactory.CreateReadOnly())
            {
                notificationEntity = await _notificationRepository.GetByIdAsync(id, cancellationToken);
            }

            if (notificationEntity == null)
            {
                return Result.NotFound();
            }

            return Result.Success(notificationEntity);
        }
    }
}
