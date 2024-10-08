using Ardalis.Result;
using EfCoreUnitOfWork.Context;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Repositories;
using EfCoreUnitOfWork.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EfCoreUnitOfWork.Tests;


public class PersonServiceTest : IDisposable
{
    private const string InMemoryConnectionString = "DataSource=:memory:";
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IPersonService _personService;
    private readonly Mock<IFakeService> _mockFakeServiceInstanceForNotificationService;
    private readonly Mock<IFakeService> _mockFakeServiceInstanceForPersonService;
    private readonly SqliteConnection _connection;


    public PersonServiceTest()
    {
        _connection = new SqliteConnection(InMemoryConnectionString);
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        var notificationsRepository = new EfRepository<NotificationEntity>(_dbContext);
        var personRepository = new EfRepository<PersonEntity>(_dbContext);

        _mockFakeServiceInstanceForNotificationService = new Mock<IFakeService>();
        _mockFakeServiceInstanceForPersonService = new Mock<IFakeService>();

        _notificationService = new NotificationService(notificationsRepository, _mockFakeServiceInstanceForNotificationService.Object);
        _personService = new PersonService(personRepository, _notificationService, _mockFakeServiceInstanceForPersonService.Object);
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

        await _dbContext.AddRangeAsync(new[] {
            new PersonEntity { Id = 1, Name = "Dupont" },
            new PersonEntity { Id = expectedId, Name = expectedName },
        });

        await _dbContext.SaveChangesAsync();


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
        Assert.Equal(1, _dbContext.Persons.Count());
        Assert.Equal(1, _dbContext.Notifications.Count());

    }

    [Fact]
    public async Task AddAsync_WithErrorInsideNotificationService_ReturnsErrorAndNotificationAndPersonNotSaved()
    {
        string expectedName = "Henry";

        _mockFakeServiceInstanceForNotificationService.Setup(x => x.DoWorkAsync()).Throws<InvalidOperationException>();
        _mockFakeServiceInstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        Result<PersonEntity> result = await _personService.AddAsync(name: expectedName);
        Assert.True(result.IsError());
        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(0, _dbContext.Persons.Count());
    }

    [Fact]
    public async Task AddAsync_WithErrorInsidePersonService_ReturnsErrorAndNotificationAndPersonNotSaved()
    {
        string expectedName = "Henry";

        _mockFakeServiceInstanceForNotificationService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeServiceInstanceForPersonService.Setup(x => x.DoWorkAsync()).Throws<InvalidOperationException>();


        Result<PersonEntity> result = await _personService.AddAsync(name: expectedName);
        Assert.True(result.IsError());
        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(0, _dbContext.Persons.Count());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
    }
}