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

            try
            {
                _unitOfWork.Begin();
                personEntity = _personRepository.Add(new PersonEntity { Name = name });
                Result<NotificationEntity> notificationResult = await _notificationService.AddAsync("une notification", cancellationToken);
                
                if (notificationResult.IsError())
                {
                    return Result.Error("erreur");
                }

                await _fakeService.DoWorkAsync();

                await _personRepository.SaveChangesAsync();
                await _unitOfWork.EndAsync();
            }
            catch (Exception ex)
            {
                return Result.Error(ex.Message);
            }

            return Result<PersonEntity>.Success(personEntity);
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
