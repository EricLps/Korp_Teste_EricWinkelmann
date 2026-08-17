using BillingService.Services;

namespace BillingService.Services;

public class InvoiceExpiryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InvoiceExpiryBackgroundService> _logger;

    //Implementaçao do TRATAMENTO DE OCORRENCIA. A cada minuto, o servico verifica se existem notas abertas que estao vencidas e expira elas
    
    public InvoiceExpiryBackgroundService(IServiceProvider serviceProvider, ILogger<InvoiceExpiryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var invoiceService = scope.ServiceProvider.GetRequiredService<InvoiceService>();
                
                await invoiceService.CancelExpiredInvoicesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao expirar notas fiscais antigas.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
