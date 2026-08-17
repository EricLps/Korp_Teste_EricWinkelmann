using Microsoft.Extensions.Hosting;
using Stockservice.Services;

namespace Stockservice.Services;

public class ReservationExpiryBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public ReservationExpiryBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ProductService>();

            try
            {
                await service.ExpireReservationsAsync();
            }
            catch
            {
                // tratar exceção, logar, etc.
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
