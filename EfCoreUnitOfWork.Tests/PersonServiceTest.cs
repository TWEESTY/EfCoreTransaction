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
    private readonly Mock<IFakeService> _mockFakeService1InstanceForPersonService;
    private readonly Mock<IFakeService> _mockFakeService2InstanceForPersonService;
    private readonly Mock<IFakeService> _mockFakeService3InstanceForPersonService;
    private readonly Mock<IFakeService> _mockFakeService4InstanceForPersonService;
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
        var unitOfWorkManager = new EfUnitOfWorkManager(_dbContext);

        _mockFakeServiceInstanceForNotificationService = new Mock<IFakeService>();
        _mockFakeService1InstanceForPersonService = new Mock<IFakeService>();
        _mockFakeService2InstanceForPersonService = new Mock<IFakeService>();
        _mockFakeService3InstanceForPersonService = new Mock<IFakeService>();
        _mockFakeService4InstanceForPersonService = new Mock<IFakeService>();

        _notificationService = new NotificationService(notificationsRepository, _mockFakeServiceInstanceForNotificationService.Object, unitOfWorkManager);
        _personService = new PersonService(
            personRepository, 
            _notificationService, 
            _mockFakeService1InstanceForPersonService.Object, 
            _mockFakeService2InstanceForPersonService.Object,
            _mockFakeService3InstanceForPersonService.Object,
            _mockFakeService4InstanceForPersonService.Object,
            unitOfWorkManager);
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
        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

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
        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

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
        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Throws<InvalidOperationException>();


        Result<PersonEntity> result = await _personService.AddAsync(name: expectedName);
        Assert.True(result.IsError());
        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(0, _dbContext.Persons.Count());
    }

    [Fact]
    public async Task AddForNestedTesting_WithNoError_SixPersonsSaved()
    {

        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService2InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService3InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService4InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        await _personService.AddForNestedTesting();

        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(6, _dbContext.Persons.Count());
    }

    [Fact]
    public async Task AddForNestedTesting_WithErrorInsideFakeService1_NoPersonSaved()
    {

        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Throws<InvalidCastException>();
        _mockFakeService2InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService3InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService4InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<InvalidCastException>(async() => await _personService.AddForNestedTesting());

        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(0, _dbContext.Persons.Count());
    }

    [Fact]
    public async Task AddForNestedTesting_WithErrorInsideFakeService2_NoPersonSaved()
    {

        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService2InstanceForPersonService.Setup(x => x.DoWorkAsync()).Throws<InvalidCastException>();
        _mockFakeService3InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService4InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<InvalidCastException>(async () => await _personService.AddForNestedTesting());

        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(0, _dbContext.Persons.Count());
    }

    [Fact]
    public async Task AddForNestedTesting_WithErrorInsideFakeService3_ThreePersonsSaved()
    {

        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService2InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService3InstanceForPersonService.Setup(x => x.DoWorkAsync()).Throws<InvalidCastException>();
        _mockFakeService4InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<InvalidCastException>(async () => await _personService.AddForNestedTesting());

        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(3, _dbContext.Persons.Count());
    }

    [Fact]
    public async Task AddForNestedTesting_WithErrorInsideFakeService4_ThreePersonsSaved()
    {

        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService2InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService3InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService4InstanceForPersonService.Setup(x => x.DoWorkAsync()).Throws<InvalidCastException>();

        await Assert.ThrowsAsync<InvalidCastException>(async () => await _personService.AddForNestedTesting());

        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(3, _dbContext.Persons.Count());
    }

    [Fact]
    public async Task AddWithTryCatchForFirstUoWForNestedTesting_WithNoError_SixPersonsSaved()
    {

        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService2InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService3InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService4InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        await _personService.AddWithTryCatchForFirstUoWForNestedTesting();

        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(6, _dbContext.Persons.Count());
    }

    [Fact]
    public async Task AddWithTryCatchForFirstUoWForNestedTesting_WithErrorInsideFakeService1_ThreePersonsSaved()
    {

        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Throws<InvalidCastException>();
        _mockFakeService2InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService3InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService4InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        await _personService.AddWithTryCatchForFirstUoWForNestedTesting();

        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(3, _dbContext.Persons.Count());
    }

    [Fact]
    public async Task AddWithTryCatchForFirstUoWForNestedTesting_WithErrorInsideFakeService2_ThreePersonsSaved()
    {

        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService2InstanceForPersonService.Setup(x => x.DoWorkAsync()).Throws<InvalidCastException>();
        _mockFakeService3InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService4InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        await _personService.AddWithTryCatchForFirstUoWForNestedTesting();

        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(3, _dbContext.Persons.Count());
    }

    [Fact]
    public async Task AddWithTryCatchForFirstUoWForNestedTesting_WithErrorInsideFakeService3_ThreePersonsSaved()
    {

        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService2InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService3InstanceForPersonService.Setup(x => x.DoWorkAsync()).Throws<InvalidCastException>();
        _mockFakeService4InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<InvalidCastException>(async () => await _personService.AddWithTryCatchForFirstUoWForNestedTesting());

        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(3, _dbContext.Persons.Count());
    }

    [Fact]
    public async Task AddWithTryCatchForFirstUoWForNestedTesting_WithErrorInsideFakeService4_ThreePersonsSaved()
    {

        _mockFakeService1InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService2InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService3InstanceForPersonService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);
        _mockFakeService4InstanceForPersonService.Setup(x => x.DoWorkAsync()).Throws<InvalidCastException>();

        await Assert.ThrowsAsync<InvalidCastException>(async () => await _personService.AddWithTryCatchForFirstUoWForNestedTesting());

        Assert.Equal(0, _dbContext.Notifications.Count());
        Assert.Equal(3, _dbContext.Persons.Count());
    }



    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
    }
}