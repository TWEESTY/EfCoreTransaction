using Ardalis.Result;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Repositories;
using System.Xml.Linq;

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
                using (IUnitOfWork unitOfWork = _unitOfWorkManager.StartOneUnitOfWork())
                {
                    personEntity = _personRepository.Add(new PersonEntity { Name = name });
                    Result<NotificationEntity> notificationResult = await _notificationService.AddAsync("une notification", cancellationToken);

                    if (notificationResult.IsError())
                    {
                        return Result.Error("erreur");
                    }

                    await _fakeService1.DoWorkAsync();

                    await _personRepository.SaveChangesAsync();
                    await unitOfWork.EndAsync();
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
            using(IUnitOfWork unitOfWork1 = _unitOfWorkManager.StartOneUnitOfWork())
            {
                _personRepository.Add(new PersonEntity { Name = "parent1" });

                using (IUnitOfWork nestedUnitOfWork = _unitOfWorkManager.StartOneUnitOfWork())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild11" });

                    await _personRepository.SaveChangesAsync();

                    await _fakeService1.DoWorkAsync();

                    await nestedUnitOfWork.EndAsync();
                }

                using (IUnitOfWork nestedUnitOfWork = _unitOfWorkManager.StartOneUnitOfWork())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild12" });

                    await _personRepository.SaveChangesAsync();
                    await nestedUnitOfWork.EndAsync();
                }

                await _personRepository.SaveChangesAsync();
                await _fakeService2.DoWorkAsync();
                await unitOfWork1.EndAsync();
            }

            using (IUnitOfWork unitOfWork1 = _unitOfWorkManager.StartOneUnitOfWork())
            {
                _personRepository.Add(new PersonEntity { Name = "parent2" });

                using (IUnitOfWork nestedUnitOfWork = _unitOfWorkManager.StartOneUnitOfWork())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild21" });

                    await _personRepository.SaveChangesAsync();

                    await _fakeService3.DoWorkAsync();

                    await nestedUnitOfWork.EndAsync();
                }

                using (IUnitOfWork nestedUnitOfWork = _unitOfWorkManager.StartOneUnitOfWork())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild22" });

                    await _personRepository.SaveChangesAsync();
                    await nestedUnitOfWork.EndAsync();
                }

                await _personRepository.SaveChangesAsync();
                await _fakeService4.DoWorkAsync();
                await unitOfWork1.EndAsync();
            }
        }

        public async Task AddWithTryCatchForFirstUoWForNestedTesting(CancellationToken cancellationToken = default)
        {
            try
            {
                using (IUnitOfWork unitOfWork1 = _unitOfWorkManager.StartOneUnitOfWork())
                {
                    _personRepository.Add(new PersonEntity { Name = "parent1" });

                    using (IUnitOfWork nestedUnitOfWork = _unitOfWorkManager.StartOneUnitOfWork())
                    {
                        _personRepository.Add(new PersonEntity { Name = "nestedChild11" });

                        await _personRepository.SaveChangesAsync();

                        await _fakeService1.DoWorkAsync();

                        await nestedUnitOfWork.EndAsync();
                    }

                    using (IUnitOfWork nestedUnitOfWork = _unitOfWorkManager.StartOneUnitOfWork())
                    {
                        _personRepository.Add(new PersonEntity { Name = "nestedChild12" });

                        await _personRepository.SaveChangesAsync();
                        await nestedUnitOfWork.EndAsync();
                    }

                    await _personRepository.SaveChangesAsync();
                    await _fakeService2.DoWorkAsync();
                    await unitOfWork1.EndAsync();
                }
            }
            catch (Exception ex)
            {
            }

            using (IUnitOfWork unitOfWork1 = _unitOfWorkManager.StartOneUnitOfWork())
            {
                _personRepository.Add(new PersonEntity { Name = "parent2" });

                using (IUnitOfWork nestedUnitOfWork = _unitOfWorkManager.StartOneUnitOfWork())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild21" });

                    await _personRepository.SaveChangesAsync();

                    await _fakeService3.DoWorkAsync();

                    await nestedUnitOfWork.EndAsync();
                }

                using (IUnitOfWork nestedUnitOfWork = _unitOfWorkManager.StartOneUnitOfWork())
                {
                    _personRepository.Add(new PersonEntity { Name = "nestedChild22" });

                    await _personRepository.SaveChangesAsync();
                    await nestedUnitOfWork.EndAsync();
                }

                await _personRepository.SaveChangesAsync();
                await _fakeService4.DoWorkAsync();
                await unitOfWork1.EndAsync();
            }
        }
    }
}
