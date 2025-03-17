using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MzadPalestine.API;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Notifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Infrastructure.Data;
using Xunit;

namespace MzadPalestine.Tests.Load.Features.Notifications;

public class NotificationLoadTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly Stopwatch _stopwatch;

    public NotificationLoadTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _serviceProvider = _factory.Services;
        _stopwatch = new Stopwatch();
    }

    [Theory]
    [InlineData(10, 100)]   // 10 concurrent users, 100 requests each
    [InlineData(50, 50)]    // 50 concurrent users, 50 requests each
    [InlineData(100, 20)]   // 100 concurrent users, 20 requests each
    public async Task GetNotifications_ShouldHandleConcurrentUsers(int concurrentUsers, int requestsPerUser)
    {
        // Arrange
        await SetupTestData(1000); // Setup base test data
        var tasks = new List<Task>();
        var successfulRequests = 0;
        var failedRequests = 0;
        var totalRequests = concurrentUsers * requestsPerUser;
        var responseTimeMs = new List<long>();

        // Act
        _stopwatch.Start();
        for (var user = 0; user < concurrentUsers; user++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (var request = 0; request < requestsPerUser; request++)
                {
                    try
                    {
                        var requestStopwatch = Stopwatch.StartNew();
                        var response = await _client.GetAsync("/api/notifications?pageNumber=1&pageSize=10");
                        requestStopwatch.Stop();

                        lock (responseTimeMs)
                        {
                            responseTimeMs.Add(requestStopwatch.ElapsedMilliseconds);
                        }

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Interlocked.Increment(ref successfulRequests);
                        }
                        else
                        {
                            Interlocked.Increment(ref failedRequests);
                        }
                    }
                    catch
                    {
                        Interlocked.Increment(ref failedRequests);
                    }
                }
            }));
        }
        await Task.WhenAll(tasks);
        _stopwatch.Stop();

        // Assert
        var averageResponseTime = responseTimeMs.Average();
        var percentile95 = responseTimeMs.OrderBy(t => t).ElementAt((int)(responseTimeMs.Count * 0.95));
        var percentile99 = responseTimeMs.OrderBy(t => t).ElementAt((int)(responseTimeMs.Count * 0.99));

        // Success rate should be at least 95%
        ((double)successfulRequests / totalRequests).Should().BeGreaterOrEqualTo(0.95);

        // Average response time should be under 500ms
        averageResponseTime.Should().BeLessThan(500);

        // 95th percentile should be under 1000ms
        percentile95.Should().BeLessThan(1000);

        // 99th percentile should be under 2000ms
        percentile99.Should().BeLessThan(2000);
    }

    [Theory]
    [InlineData(10, 50)]    // 10 concurrent users, 50 requests each
    [InlineData(25, 20)]    // 25 concurrent users, 20 requests each
    [InlineData(50, 10)]    // 50 concurrent users, 10 requests each
    public async Task MarkAsRead_ShouldHandleConcurrentUsers(int concurrentUsers, int requestsPerUser)
    {
        // Arrange
        await SetupTestData(1000);
        var tasks = new List<Task>();
        var successfulRequests = 0;
        var failedRequests = 0;
        var totalRequests = concurrentUsers * requestsPerUser;
        var responseTimeMs = new List<long>();

        // Act
        _stopwatch.Start();
        for (var user = 0; user < concurrentUsers; user++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (var request = 0; request < requestsPerUser; request++)
                {
                    try
                    {
                        var requestStopwatch = Stopwatch.StartNew();
                        var response = await _client.PutAsync($"/api/notifications/{request + 1}/read", null);
                        requestStopwatch.Stop();

                        lock (responseTimeMs)
                        {
                            responseTimeMs.Add(requestStopwatch.ElapsedMilliseconds);
                        }

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Interlocked.Increment(ref successfulRequests);
                        }
                        else
                        {
                            Interlocked.Increment(ref failedRequests);
                        }
                    }
                    catch
                    {
                        Interlocked.Increment(ref failedRequests);
                    }
                }
            }));
        }
        await Task.WhenAll(tasks);
        _stopwatch.Stop();

        // Assert
        var averageResponseTime = responseTimeMs.Average();
        var percentile95 = responseTimeMs.OrderBy(t => t).ElementAt((int)(responseTimeMs.Count * 0.95));

        // Success rate should be at least 95%
        ((double)successfulRequests / totalRequests).Should().BeGreaterOrEqualTo(0.95);

        // Average response time should be under 200ms
        averageResponseTime.Should().BeLessThan(200);

        // 95th percentile should be under 500ms
        percentile95.Should().BeLessThan(500);
    }

    [Theory]
    [InlineData(5, 20)]     // 5 concurrent users, 20 requests each
    [InlineData(10, 10)]    // 10 concurrent users, 10 requests each
    [InlineData(20, 5)]     // 20 concurrent users, 5 requests each
    public async Task DeleteNotifications_ShouldHandleConcurrentUsers(int concurrentUsers, int requestsPerUser)
    {
        // Arrange
        await SetupTestData(1000);
        var tasks = new List<Task>();
        var successfulRequests = 0;
        var failedRequests = 0;
        var totalRequests = concurrentUsers * requestsPerUser;
        var responseTimeMs = new List<long>();

        // Act
        _stopwatch.Start();
        for (var user = 0; user < concurrentUsers; user++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (var request = 0; request < requestsPerUser; request++)
                {
                    try
                    {
                        var requestStopwatch = Stopwatch.StartNew();
                        var response = await _client.DeleteAsync($"/api/notifications/{request + 1}");
                        requestStopwatch.Stop();

                        lock (responseTimeMs)
                        {
                            responseTimeMs.Add(requestStopwatch.ElapsedMilliseconds);
                        }

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Interlocked.Increment(ref successfulRequests);
                        }
                        else
                        {
                            Interlocked.Increment(ref failedRequests);
                        }
                    }
                    catch
                    {
                        Interlocked.Increment(ref failedRequests);
                    }
                }
            }));
        }
        await Task.WhenAll(tasks);
        _stopwatch.Stop();

        // Assert
        var averageResponseTime = responseTimeMs.Average();
        var percentile95 = responseTimeMs.OrderBy(t => t).ElementAt((int)(responseTimeMs.Count * 0.95));

        // Success rate should be at least 95%
        ((double)successfulRequests / totalRequests).Should().BeGreaterOrEqualTo(0.95);

        // Average response time should be under 300ms
        averageResponseTime.Should().BeLessThan(300);

        // 95th percentile should be under 750ms
        percentile95.Should().BeLessThan(750);
    }

    [Fact]
    public async Task EventHandling_ShouldHandleHighVolume()
    {
        // Arrange
        const int eventCount = 1000;
        var tasks = new List<Task>();
        var successfulEvents = 0;
        var failedEvents = 0;
        var responseTimeMs = new List<long>();

        // Act
        _stopwatch.Start();
        for (var i = 0; i < eventCount; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var requestStopwatch = Stopwatch.StartNew();
                    
                    // Simulate auction creation event
                    var response = await _client.PostAsJsonAsync("/api/auctions", new
                    {
                        Title = $"Test Auction {i}",
                        Description = "Test Description",
                        StartingPrice = 100,
                        Duration = TimeSpan.FromDays(7)
                    });

                    requestStopwatch.Stop();

                    lock (responseTimeMs)
                    {
                        responseTimeMs.Add(requestStopwatch.ElapsedMilliseconds);
                    }

                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        Interlocked.Increment(ref successfulEvents);
                    }
                    else
                    {
                        Interlocked.Increment(ref failedEvents);
                    }
                }
                catch
                {
                    Interlocked.Increment(ref failedEvents);
                }
            }));
        }
        await Task.WhenAll(tasks);
        _stopwatch.Stop();

        // Assert
        var averageResponseTime = responseTimeMs.Average();
        var percentile95 = responseTimeMs.OrderBy(t => t).ElementAt((int)(responseTimeMs.Count * 0.95));

        // Success rate should be at least 95%
        ((double)successfulEvents / eventCount).Should().BeGreaterOrEqualTo(0.95);

        // Average response time should be under 500ms
        averageResponseTime.Should().BeLessThan(500);

        // 95th percentile should be under 1000ms
        percentile95.Should().BeLessThan(1000);
    }

    private async Task SetupTestData(int count)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clear existing data
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Generate test data in batches
        const int batchSize = 100;
        for (var i = 0; i < count; i += batchSize)
        {
            var batch = Enumerable.Range(i, Math.Min(batchSize, count - i))
                .Select(j => new Notification
                {
                    UserId = 1,
                    Title = $"Test Notification {j}",
                    Message = $"Test Message {j}",
                    Type = NotificationType.AuctionCreated,
                    IsRead = j % 2 == 0,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-j),
                    ActionUrl = $"/test/{j}",
                    ImageUrl = $"/images/{j}.jpg"
                });

            await context.Notifications.AddRangeAsync(batch);
            await context.SaveChangesAsync();
        }
    }
}
