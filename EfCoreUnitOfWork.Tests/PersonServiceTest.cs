using Ardalis.Result;
using EfCoreUnitOfWork.Context;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Repositories;
using EfCoreUnitOfWork.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Zejji.Entity;

namespace EfCoreUnitOfWork.Tests;


public class PersonServiceTest : IDisposable
{
    private const string InMemoryConnectionString = "DataSource=:memory:";
    private readonly INotificationService _notificationService;
    private readonly IPersonService _personService;
    private readonly Mock<IFakeService> _mockFakeServiceInstanceForNotificationService;
    private readonly Mock<IFakeService> _mockFakeServiceInstanceForPersonService;
    private readonly SqliteConnection _connection;
    private readonly DbContextScopeFactory _dbContextScopeFactory;


    public PersonServiceTest()
    {
        

        var dbContextFactory = new RegisteredDbContextFactory();
        _connection = new SqliteConnection("DataSource=:memory:");

        dbContextFactory.RegisterDbContextType<AppDbContext>(() =>
        {
            _connection.Open();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(_connection)
                    .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        });

        _dbContextScopeFactory = new DbContextScopeFactory(dbContextFactory);
        var ambienDbContextLocator = new AmbientDbContextLocator();


        var notificationsRepository = new EfRepository<NotificationEntity>(ambienDbContextLocator);
        var personRepository = new EfRepository<PersonEntity>(ambienDbContextLocator);

        _mockFakeServiceInstanceForNotificationService = new Mock<IFakeService>();
        _mockFakeServiceInstanceForPersonService = new Mock<IFakeService>();

        _notificationService = new NotificationService(notificationsRepository, _mockFakeServiceInstanceForNotificationService.Object, _dbContextScopeFactory);
        _personService = new PersonService(personRepository, _notificationService, _mockFakeServiceInstanceForPersonService.Object, _dbContextScopeFactory);
    }

    [Fact]
    public async Task GetAsync_IdNotPresent_ReturnsNotFound()
    {
        Result<PersonEntity> result = await _personService.GetAsync(id: 1);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task GetAsync_IdPresent_ReturnsPerson()
    {
        string expectedName = "Henry";
        int expectedId = 2;

        using (var dbContextScope = _dbContextScopeFactory.Create())
        {
            await dbContextScope.DbContexts.Get<AppDbContext>().AddRangeAsync(new[] {
                new PersonEntity { Id = 1, Name = "Dupont" },
                new PersonEntity { Id = expectedId, Name = expectedName },
            });

            await dbContextScope.DbContexts.Get<AppDbContext>().SaveChangesAsync();
        }


        Result<PersonEntity> result = await _personService.GetAsync(id: expectedId);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedId, result.Value.Id);
        Assert.Equal(expectedName, result.Value.Name);
    }

    [Fact]
    public async Task AddAsync_NoError_ReturnsSuccessAndPersonAndNotificationSaved()
    {
        string expectedName = "Henry";

        _mockFakeServiceInstanceForNotificationService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeServiceInstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        Result<PersonEntity> result = await _personService.AddAsync(name: expectedName);
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedName, result.Value.Name);

        using (var dbContextScope = _dbContextScopeFactory.CreateReadOnly())
        {
            Assert.Equal(1, dbContextScope.DbContexts.Get<AppDbContext>().Persons.Count());
            Assert.Equal(1, dbContextScope.DbContexts.Get<AppDbContext>().Notifications.Count());
        }

    }

    [Fact]
    public async Task AddAsync_WithErrorInsideNotificationService_ReturnsErrorAndNotificationAndPersonNotSaved()
    {
        string expectedName = "Henry";

        _mockFakeServiceInstanceForNotificationService.Setup(x => x.DoWorkAsync()).Throws<InvalidOperationException>();
        _mockFakeServiceInstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        Result<PersonEntity> result = await _personService.AddAsync(name: expectedName);

        using (var dbContextScope = _dbContextScopeFactory.CreateReadOnly())
        {
            Assert.True(result.IsError());
            Assert.Equal(0, dbContextScope.DbContexts.Get<AppDbContext>().Notifications.Count());
            Assert.Equal(0, dbContextScope.DbContexts.Get<AppDbContext>().Persons.Count());
        }
    }

    [Fact]
    public async Task AddAsync_WithErrorInsidePersonService_ReturnsErrorAndNotificationAndPersonNotSaved()
    {
        string expectedName = "Henry";

        _mockFakeServiceInstanceForNotificationService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeServiceInstanceForPersonService.Setup(x => x.DoWorkAsync()).Throws<InvalidOperationException>();


        Result<PersonEntity> result = await _personService.AddAsync(name: expectedName);

        using (var dbContextScope = _dbContextScopeFactory.CreateReadOnly())
        {
            Assert.True(result.IsError());
            Assert.Equal(0, dbContextScope.DbContexts.Get<AppDbContext>().Notifications.Count());
            Assert.Equal(0, dbContextScope.DbContexts.Get<AppDbContext>().Persons.Count());
        }
    }

    public void Dispose()
    {
        _connection.Close();
    }
}