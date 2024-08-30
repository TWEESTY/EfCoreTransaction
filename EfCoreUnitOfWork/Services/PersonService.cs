using Ardalis.Result;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Repositories;

namespace EfCoreUnitOfWork.Services
{
    public class PersonService : IPersonService
    {
        private readonly IRepository<PersonEntity> _personRepository;
        private readonly INotificationService _notificationService;
        private readonly IFakeService _fakeService;
        private readonly IUnitOfWork _unitOfWork;

        public PersonService(IRepository<PersonEntity> personRepository, INotificationService notificationService, IFakeService fakeService, IUnitOfWork unitOfWork)
        {
            _personRepository = personRepository;
            _notificationService = notificationService;
            _fakeService = fakeService;
            _unitOfWork = unitOfWork;
        }


        public async Task<Result<PersonEntity>> AddAsync(string name, CancellationToken cancellationToken = default)
        {
            PersonEntity personEntity;
            Result<PersonEntity>? result = null;

            try
            {
                await _unitOfWork.DoWorkAsync(async () =>
                {
                    personEntity = _personRepository.Add(new PersonEntity { Name = name });
                    Result<NotificationEntity> notificationResult = await _notificationService.AddAsync("une notification", cancellationToken);

                    if (notificationResult.IsError())
                    {
                        result = Result.Error("erreur");
                        return;
                    }

                    await _fakeService.DoWorkAsync();

                    await _personRepository.SaveChangesAsync();

                    result = Result.Success(personEntity);
                });
            }
            catch (Exception ex)
            {
                return result = Result.Error(ex.Message);
            }

            return result!;
        }

        public async Task<Result<PersonEntity>> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            PersonEntity? personEntity = await _personRepository.GetByIdAsync(id, cancellationToken);

            if (personEntity == null)
            {
                return Result.NotFound();
            }

            return Result.Success(personEntity);
        }
    }
}
