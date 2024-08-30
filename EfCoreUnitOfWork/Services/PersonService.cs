using Ardalis.Result;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Repositories;
using Zejji.Entity;

namespace EfCoreUnitOfWork.Services
{
    public class PersonService : IPersonService
    {
        private readonly IRepository<PersonEntity> _personRepository;
        private readonly INotificationService _notificationService;
        private readonly IFakeService _fakeService;
        private readonly IDbContextScopeFactory _dbContextScopeFactory;

        public PersonService(IRepository<PersonEntity> personRepository, INotificationService notificationService, IFakeService fakeService, IDbContextScopeFactory dbContextScopeFactory)
        {
            _personRepository = personRepository;
            _notificationService = notificationService;
            _fakeService = fakeService;
            _dbContextScopeFactory = dbContextScopeFactory;
        }


        public async Task<Result<PersonEntity>> AddAsync(string name, CancellationToken cancellationToken = default)
        {
            PersonEntity personEntity;

            try
            {
                using (var dbContextScope = _dbContextScopeFactory.Create())
                {
                    personEntity = _personRepository.Add(new PersonEntity { Name = name });
                    Result<NotificationEntity> notificationResult = await _notificationService.AddAsync("une notification", cancellationToken);

                    if (notificationResult.IsError())
                    {
                        return Result.Error("erreur");
                    }

                    await _fakeService.DoWorkAsync();

                    await dbContextScope.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return Result.Error(ex.Message);
            }

            return Result<PersonEntity>.Success(personEntity);
        }

        public async Task<Result<PersonEntity>> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            PersonEntity? personEntity;

            using (var dbContextScope = _dbContextScopeFactory.CreateReadOnly())
            {
                personEntity = await _personRepository.GetByIdAsync(id, cancellationToken);
            }

            if (personEntity == null)
            {
                return Result.NotFound();
            }

            return Result.Success(personEntity);
        }
    }
}
