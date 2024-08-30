using Ardalis.Result;
using Castle.Components.DictionaryAdapter.Xml;
using EfCoreUnitOfWork.Context;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Repositories;
using EfCoreUnitOfWork.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using System.Net.Sockets;
using Zejji.Entity;

namespace EfCoreUnitOfWork.Tests;


public class NotificationServiceTest : IDisposable
{
    private const string InMemoryConnectionString = "DataSource=:memory:";
    private readonly INotificationService _notificationService;
    private readonly Mock<IFakeService> _mockFakeService;
    private readonly SqliteConnection _connection;
    private readonly DbContextScopeFactory _dbContextScopeFactory;

    public NotificationServiceTest()
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

        _dbContextScopeFactory =  new DbContextScopeFactory(dbContextFactory);

        var notificationsRepository = new EfRepository<NotificationEntity>(new AmbientDbContextLocator());
        _mockFakeService = new Mock<IFakeService>();
        _notificationService = new NotificationService(notificationsRepository, _mockFakeService.Object, _dbContextScopeFactory);

    }

    [Fact]
    public async Task GetAsync_IdNotPresent_ReturnsNotFound()
    {
        Result<NotificationEntity> result = await _notificationService.GetAsync(id: 1);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task GetAsync_IdPresent_ReturnsNotification()
    {
        string expectedText = "bon text";
        int expectedId = 2;

        using (var dbContextScope = _dbContextScopeFactory.Create())
        {
            await dbContextScope.DbContexts.Get<AppDbContext>().Notifications.AddRangeAsync(new[] {
                new NotificationEntity { Id = 1, Text = "blabla" },
                new NotificationEntity { Id = expectedId, Text = expectedText },
            });

            await dbContextScope.SaveChangesAsync();
        }

        Result<NotificationEntity> result = await _notificationService.GetAsync(id: expectedId);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedId, result.Value.Id);
        Assert.Equal(expectedText, result.Value.Text);
    }

    [Fact]
    public async Task AddAsync_NoError_ReturnsSuccessAndNotificationSaved()
    {
        string expectedText = "bon text";

        _mockFakeService.Setup(x => x.DoWorkAsync()).Returns(Task.CompletedTask);

        Result<NotificationEntity> result = await _notificationService.AddAsync(text: expectedText);

        using (var dbContextScope = _dbContextScopeFactory.CreateReadOnly())
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedText, result.Value.Text);
            Assert.Equal(1, dbContextScope.DbContexts.Get<AppDbContext>().Notifications.Count());
        }
    }

    [Fact]
    public async Task AddAsync_WithErrorInsideFakeService_ReturnsErrorAndNotificationNotSaved()
    {
        string expectedText = "bon text";

        _mockFakeService.Setup(x => x.DoWorkAsync()).Throws<InvalidOperationException>();

        Result<NotificationEntity> result = await _notificationService.AddAsync(text: expectedText);

        using (var dbContextScope = _dbContextScopeFactory.CreateReadOnly())
        {
            Assert.True(result.IsError());
            Assert.Equal(0, dbContextScope.DbContexts.Get<AppDbContext>().Notifications.Count());
        }
    }

    public void Dispose()
    {
        _connection.Close();
    }
}