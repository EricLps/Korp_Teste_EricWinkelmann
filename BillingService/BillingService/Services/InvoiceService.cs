using BillingService.Dtos;
using BillingService.Models;
using BillingService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Services;

public partial class InvoiceService
{
    private readonly InvoiceRepository _repository;
    private readonly StockHttpClient _stockHttpClient;

    public InvoiceService(InvoiceRepository repository, StockHttpClient stockHttpClient)
    {
        _repository = repository;
        _stockHttpClient = stockHttpClient;
    }

    public async Task<List<InvoiceDto>> GetInvoicesAsync()
    {
        var invoices = await _repository.GetAllAsync();
        return invoices.Select(MapToDto).ToList();
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new InvalidOperationException("A nota fiscal deve conter pelo menos um item.");
        }

        foreach (var item in request.Items)
        {
            if (item.ProductId == Guid.Empty)
            {
                throw new InvalidOperationException("Produto inválido para a nota fiscal.");
            }

            if (string.IsNullOrWhiteSpace(item.ProductName))
            {
                throw new InvalidOperationException("Nome do produto obrigatório para cada item.");
            }

            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException("A quantidade de cada item deve ser maior que zero.");
            }
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Status = InvoiceStatus.Open,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            Items = new List<InvoiceItem>()
        };

        var reservedMap = new Dictionary<Guid, Guid>();

        try
        {
            foreach (var item in request.Items)
            {
                var reservePayload = new { InvoiceId = invoice.Id, Quantity = item.Quantity };
                var response = await _stockHttpClient.ReserveAsync(item.ProductId, reservePayload);

                if (!response.IsSuccessStatusCode)
                {
                    var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                    var message = problem?.Detail ?? problem?.Title ?? "Não foi possível reservar o estoque.";
                    throw new InvalidOperationException(message);
                }

                var reservation = await response.Content.ReadFromJsonAsync<ReservationResponseDto>();
                if (reservation is null)
                {
                    throw new InvalidOperationException("Resposta inválida do serviço de estoque.");
                }

                        reservedMap[item.ProductId] = reservation.ReservationId;
            }

            foreach (var item in request.Items)
            {
                invoice.Items.Add(new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    ProductId = item.ProductId,
                            ReservationId = reservedMap.ContainsKey(item.ProductId) ? reservedMap[item.ProductId] : Guid.Empty,
                            ProductName = item.ProductName.Trim(),
                            Quantity = item.Quantity,
                            Invoice = invoice
                        });
                    }

                    await _repository.AddAsync(invoice);
                    return MapToDto(invoice);
                }
        catch
        {
            // besteffort rollback: tenta cancelar reservas feitas antes de falhar
            foreach (var kv in reservedMap)
            {
                try
                {
                    var cancelPayload = new { ReservationId = kv.Value };
                    var cancelResponse = await _stockHttpClient.CancelReservationAsync(kv.Key, cancelPayload);
                    if (!cancelResponse.IsSuccessStatusCode)
                    {
                        // mantem o best-effort rollback apenas, nao lanca excecao
                    }
                }
                catch
                {
                    //mantem o best-effort rollback apenas
                }
            }

            throw;
        }
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            Number = invoice.Number,
            Status = invoice.Status.ToString(),
            CreatedAt = invoice.CreatedAt,
            ExpiresAt = invoice.ExpiresAt,
            Items = invoice.Items.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                            ReservationId = item.ReservationId,
                            ProductName = item.ProductName,
                            Quantity = item.Quantity
                        }).ToList()
        };
    public async Task CancelExpiredInvoicesAsync()
    {
        var now = DateTime.UtcNow;
        var expiredInvoices = await _repository.GetExpiredInvoicesAsync(now);

        foreach (var invoice in expiredInvoices)
        {
            invoice.Status = InvoiceStatus.Expired;
        }

        if (expiredInvoices.Count > 0)
        {
            await _repository.SaveChangesAsync();
        }
    }
}
