﻿using Ardalis.Result;
using EfCoreUnitOfWork.Context;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Repositories;
using EfCoreUnitOfWork.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EfCoreUnitOfWork.Tests;


public class NotificationServiceTest : IDisposable
{
    private const string InMemoryConnectionString = "DataSource=:memory:";
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly Mock<IFakeService> _mockFakeService;
    private readonly SqliteConnection _connection;


    public NotificationServiceTest()
    {
        _connection = new SqliteConnection(InMemoryConnectionString);
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        var notificationsRepository = new EfRepository<NotificationEntity>(_dbContext);
        _mockFakeService = new Mock<IFakeService>();
        _notificationService = new NotificationService(notificationsRepository, _mockFakeService.Object);

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

        await _dbContext.AddRangeAsync(new[] {
            new NotificationEntity { Id = 1, Text = "blabla" },
            new NotificationEntity { Id = expectedId, Text = expectedText },
        });

        await _dbContext.SaveChangesAsync();


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
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedText, result.Value.Text);
        Assert.Equal(1, _dbContext.Notifications.Count());
    }

    [Fact]
    public async Task AddAsync_WithErrorInsideFakeService_ReturnsErrorAndNotificationNotSaved()
    {
        string expectedText = "bon text";

        _mockFakeService.Setup(x => x.DoWorkAsync()).Throws<InvalidOperationException>();

        Result<NotificationEntity> result = await _notificationService.AddAsync(text: expectedText);
        Assert.True(result.IsError());
        Assert.Equal(0, _dbContext.Notifications.Count());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
    }
}