using Ardalis.Result;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Repositories;

namespace EfCoreUnitOfWork.Services
{
    public class PersonService : IPersonService
    {
        private readonly IRepository<PersonEntity> _personRepository;
        private readonly INotificationService _notificationService;
        private readonly IFakeService _fakeService1;
        private readonly IFakeService _fakeService2;
        private readonly IFakeService _fakeService3;
        private readonly IFakeService _fakeService4;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public PersonService(IRepository<PersonEntity> personRepository, INotificationService notificationService, IFakeService fakeService, IFakeService fakeService2, IFakeService fakeService3, IFakeService fakeService4, IUnitOfWorkManager unitOfWorkManager)
        {
            _personRepository = personRepository;
            _notificationService = notificationService;
            _fakeService1 = fakeService;
            _fakeService2 = fakeService2;
            _fakeService3 = fakeService3;
            _fakeService4 = fakeService4;
            _unitOfWorkManager = unitOfWorkManager;
        }


        public async Task<Result<PersonEntity>> AddAsync(string name, CancellationToken cancellationToken = default)
        {
            PersonEntity personEntity;

            try
            {
                await using (IUnitOfWork unitOfWork = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
                {
                    personEntity = _personRepository.Add(new PersonEntity { Name = name });
                    Result<NotificationEntity> notificationResult = await _notificationService.AddAsync("une notification", cancellationToken);

                    if (notificationResult.IsError())
                    {
                        return Result.Error("erreur");
                    }

                    await _fakeService1.DoWorkAsync();

                    await _personRepository.SaveChangesAsync();
                    await _unitOfWorkManager.EndUnitOfWorkAsync(unitOfWork);
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
            PersonEntity? personEntity = await _personRepository.GetByIdAsync(id, cancellationToken);

            if (personEntity == null)
            {
                return Result.NotFound();
            }

            return Result.Success(personEntity);
        }

        public async Task AddForNestedTesting(CancellationToken cancellationToken)
        {
            await using (IUnitOfWork unitOfWork1 = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
            {
                _personRepository.Add(new PersonEntity { Name = "parent1" });

                await using (IUnitOfWork nestedUnitOfWork = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild11" });

                    await _personRepository.SaveChangesAsync();

                    await _fakeService1.DoWorkAsync();

                    await _unitOfWorkManager.EndUnitOfWorkAsync(nestedUnitOfWork);
                }

                await using (IUnitOfWork nestedUnitOfWork = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild12" });

                    await _personRepository.SaveChangesAsync();
                    await _unitOfWorkManager.EndUnitOfWorkAsync(nestedUnitOfWork);
                }

                await _personRepository.SaveChangesAsync();
                await _fakeService2.DoWorkAsync();
                await _unitOfWorkManager.EndUnitOfWorkAsync(unitOfWork1);
            }

            await using (IUnitOfWork unitOfWork1 = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
            {
                _personRepository.Add(new PersonEntity { Name = "parent2" });

                await using (IUnitOfWork nestedUnitOfWork = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild21" });

                    await _personRepository.SaveChangesAsync();

                    await _fakeService3.DoWorkAsync();

                    await _unitOfWorkManager.EndUnitOfWorkAsync(nestedUnitOfWork);
                }

                await using (IUnitOfWork nestedUnitOfWork = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild22" });

                    await _personRepository.SaveChangesAsync();
                    await _unitOfWorkManager.EndUnitOfWorkAsync(nestedUnitOfWork);
                }

                await _personRepository.SaveChangesAsync();
                await _fakeService4.DoWorkAsync();
                await _unitOfWorkManager.EndUnitOfWorkAsync(unitOfWork1);
            }
        }

        public async Task AddWithTryCatchForFirstUoWForNestedTesting(CancellationToken cancellationToken = default)
        {
            try
            {
                await using (IUnitOfWork unitOfWork1 = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
                {
                    _personRepository.Add(new PersonEntity { Name = "parent1" });

                    await using (IUnitOfWork nestedUnitOfWork = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
                    {
                        _personRepository.Add(new PersonEntity { Name = "nestedChild11" });

                        await _personRepository.SaveChangesAsync();

                        await _fakeService1.DoWorkAsync();

                        await _unitOfWorkManager.EndUnitOfWorkAsync(nestedUnitOfWork);
                    }

                    await using (IUnitOfWork nestedUnitOfWork = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
                    {
                        _personRepository.Add(new PersonEntity { Name = "nestedChild12" });

                        await _personRepository.SaveChangesAsync();
                        await _unitOfWorkManager.EndUnitOfWorkAsync(nestedUnitOfWork);
                    }

                    await _personRepository.SaveChangesAsync();
                    await _fakeService2.DoWorkAsync();
                    await _unitOfWorkManager.EndUnitOfWorkAsync(unitOfWork1);
                }
            }
            catch (Exception ex)
            {
            }

            await using (IUnitOfWork unitOfWork1 = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
            {
                _personRepository.Add(new PersonEntity { Name = "parent2" });

                await using (IUnitOfWork nestedUnitOfWork = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild21" });

                    await _personRepository.SaveChangesAsync();

                    await _fakeService3.DoWorkAsync();

                    await _unitOfWorkManager.EndUnitOfWorkAsync(nestedUnitOfWork);
                }

                await using (IUnitOfWork nestedUnitOfWork = await _unitOfWorkManager.StartOneUnitOfWorkAsync())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild22" });

                    await _personRepository.SaveChangesAsync();
                    await _unitOfWorkManager.EndUnitOfWorkAsync(nestedUnitOfWork);
                }

                await _personRepository.SaveChangesAsync();
                await _fakeService4.DoWorkAsync();
                await _unitOfWorkManager.EndUnitOfWorkAsync(unitOfWork1);
            }
        }
    }
}
