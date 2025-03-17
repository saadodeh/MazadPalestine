using System.Diagnostics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.Features.Notifications.Commands.DeleteAllNotifications;
using MzadPalestine.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using MzadPalestine.Application.Features.Notifications.Queries.GetUnreadNotificationsCount;
using MzadPalestine.Application.Features.Notifications.Queries.GetUserNotifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Infrastructure.Data;
using Xunit;

namespace MzadPalestine.Tests.Performance.Features.Notifications;

public class NotificationPerformanceTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly IServiceProvider _serviceProvider;
    private readonly Stopwatch _stopwatch;

    public NotificationPerformanceTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _serviceProvider = _fixture.ServiceProvider;
        _stopwatch = new Stopwatch();
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public async Task GetUserNotifications_ShouldPerformEfficiently_WithLargeDataset(int notificationCount)
    {
        // Arrange
        await SetupTestData(notificationCount);
        var handler = _serviceProvider.GetRequiredService<GetUserNotificationsQueryHandler>();
        var query = new GetUserNotificationsQuery(1, 10);

        // Act
        _stopwatch.Start();
        var result = await handler.Handle(query, CancellationToken.None);
        _stopwatch.Stop();

        // Assert
        result.IsSuccess.Should().BeTrue();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public async Task GetUnreadCount_ShouldPerformEfficiently_WithLargeDataset(int notificationCount)
    {
        // Arrange
        await SetupTestData(notificationCount);
        var handler = _serviceProvider.GetRequiredService<GetUnreadNotificationsCountQueryHandler>();
        var query = new GetUnreadNotificationsCountQuery();

        // Act
        _stopwatch.Start();
        var result = await handler.Handle(query, CancellationToken.None);
        _stopwatch.Stop();

        // Assert
        result.IsSuccess.Should().BeTrue();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(500); // Should complete within 500ms
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public async Task MarkAllAsRead_ShouldPerformEfficiently_WithLargeDataset(int notificationCount)
    {
        // Arrange
        await SetupTestData(notificationCount);
        var handler = _serviceProvider.GetRequiredService<MarkAllNotificationsAsReadCommandHandler>();
        var command = new MarkAllNotificationsAsReadCommand();

        // Act
        _stopwatch.Start();
        var result = await handler.Handle(command, CancellationToken.None);
        _stopwatch.Stop();

        // Assert
        result.IsSuccess.Should().BeTrue();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Should complete within 2 seconds
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public async Task DeleteAllNotifications_ShouldPerformEfficiently_WithLargeDataset(int notificationCount)
    {
        // Arrange
        await SetupTestData(notificationCount);
        var handler = _serviceProvider.GetRequiredService<DeleteAllNotificationsCommandHandler>();
        var command = new DeleteAllNotificationsCommand();

        // Act
        _stopwatch.Start();
        var result = await handler.Handle(command, CancellationToken.None);
        _stopwatch.Stop();

        // Assert
        result.IsSuccess.Should().BeTrue();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Should complete within 2 seconds
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public async Task FilterAndSort_ShouldPerformEfficiently_WithLargeDataset(int notificationCount)
    {
        // Arrange
        await SetupTestData(notificationCount);
        var handler = _serviceProvider.GetRequiredService<GetUserNotificationsQueryHandler>();
        var query = new GetUserNotificationsQuery(
            1, 10, false, "test", "createdat", true);

        // Act
        _stopwatch.Start();
        var result = await handler.Handle(query, CancellationToken.None);
        _stopwatch.Stop();

        // Assert
        result.IsSuccess.Should().BeTrue();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public async Task ConcurrentOperations_ShouldHandleEfficiently(int notificationCount)
    {
        // Arrange
        await SetupTestData(notificationCount);
        var tasks = new List<Task<bool>>();

        // Act
        _stopwatch.Start();
        for (var i = 0; i < 10; i++)
        {
            tasks.Add(ExecuteConcurrentOperations());
        }
        await Task.WhenAll(tasks);
        _stopwatch.Stop();

        // Assert
        tasks.All(t => t.Result).Should().BeTrue();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    private async Task<bool> ExecuteConcurrentOperations()
    {
        try
        {
            var getHandler = _serviceProvider.GetRequiredService<GetUserNotificationsQueryHandler>();
            var markHandler = _serviceProvider.GetRequiredService<MarkAllNotificationsAsReadCommandHandler>();

            var getResult = await getHandler.Handle(new GetUserNotificationsQuery(1, 10), CancellationToken.None);
            var markResult = await markHandler.Handle(new MarkAllNotificationsAsReadCommand(), CancellationToken.None);

            return getResult.IsSuccess && markResult.IsSuccess;
        }
        catch
        {
            return false;
        }
    }

    private async Task SetupTestData(int count)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clear existing data
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Notifications");

        // Generate test data
        var notifications = Enumerable.Range(1, count)
            .Select(i => new Notification
            {
                UserId = 1,
                Title = $"Test Notification {i}",
                Message = $"Test Message {i}",
                Type = NotificationType.AuctionCreated,
                IsRead = i % 2 == 0,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                ActionUrl = $"/test/{i}",
                ImageUrl = $"/images/{i}.jpg"
            })
            .ToList();

        // Insert in batches to improve performance
        const int batchSize = 1000;
        for (var i = 0; i < notifications.Count; i += batchSize)
        {
            var batch = notifications.Skip(i).Take(batchSize);
            await context.Notifications.AddRangeAsync(batch);
            await context.SaveChangesAsync();
        }
    }
}
