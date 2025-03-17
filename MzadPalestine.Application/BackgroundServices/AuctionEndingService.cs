using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MzadPalestine.Application.Features.Auctions.Commands.EndAuction;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.BackgroundServices;

public class AuctionEndingService : BackgroundService
{
    private readonly ILogger<AuctionEndingService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PeriodicTimer _timer;

    public AuctionEndingService(
        ILogger<AuctionEndingService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        // Check for ended auctions every minute
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction Ending Service is starting");

        try
        {
            while (await _timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessEndedAuctionsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Auction Ending Service is stopping");
        }
    }

    private async Task ProcessEndedAuctionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Get all active auctions that have ended
            var endedAuctions = await unitOfWork.Repository<Auction>()
                .ListAsync(x => x.Status == AuctionStatus.Active &&
                              x.EndTime <= DateTime.UtcNow &&
                              x.Bids.Any());

            foreach (var auction in endedAuctions)
            {
                try
                {
                    _logger.LogInformation("Processing ended auction: {AuctionId} - {Title}", 
                        auction.Id, auction.Title);

                    // End the auction using the EndAuctionCommand
                    var result = await mediator.Send(
                        new EndAuctionCommand(auction.Id), 
                        cancellationToken);

                    if (!result.IsSuccess)
                    {
                        _logger.LogWarning(
                            "Failed to end auction {AuctionId}: {Error}",
                            auction.Id,
                            result.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error processing ended auction {AuctionId}",
                        auction.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error in Auction Ending Service");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Auction Ending Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
