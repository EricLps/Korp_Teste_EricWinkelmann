using BillingService.Dtos;
using BillingService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Services;

public partial class InvoiceService
{
    public async Task<InvoiceDto> PrintInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _repository.GetByIdAsync(invoiceId);
        if (invoice is null)
            throw new KeyNotFoundException("Nota fiscal não encontrada.");

        if (invoice.Status != InvoiceStatus.Open)
            throw new InvalidOperationException("Apenas notas em estado Aberto podem ser impressas.");

        if (invoice.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidOperationException("A nota está expirada e não pode ser impressa.");

        var confirmed = new List<(Guid ProductId, Guid ReservationId)>();

        try
        {
            foreach (var item in invoice.Items)
            {
                var payload = new { ReservationId = item.ReservationId };
                var response = await _stockHttpClient.ConfirmReservationAsync(item.ProductId, payload);

                if (!response.IsSuccessStatusCode)
                {
                    // Lê os detalhes do problema
                    var problem = await response.Content.ReadFromJsonAsync<ProblemDetails?>();
                    var msg = problem?.Detail ?? problem?.Title ?? "Falha ao confirmar reserva no serviço de estoque.";

                    // Se o servico estiver indisponível lanca uma excecao diferente pro cliente saber que é um problema temporario
                    if ((int)response.StatusCode >= 500)
                        throw new InvalidOperationException("Serviço de estoque indisponível. Tente novamente.");

                    throw new InvalidOperationException(msg);
                }

                confirmed.Add((item.ProductId, item.ReservationId));
            }

            invoice.Status = InvoiceStatus.Closed;
            await _repository.SaveChangesAsync();

            return MapToDto(invoice);
        }
        catch
        {
            // best effort: tenta cancelar todas as reservas que foram confirmadas antes de lançar a exceção
            foreach (var c in confirmed)
            {
                try
                {
                    var cancelPayload = new { ReservationId = c.ReservationId };
                    await _stockHttpClient.CancelReservationAsync(c.ProductId, cancelPayload);
                }
                catch
                {
                    // ignora falhas no cancelamento
                }
            }

            throw;
        }
    }
}
