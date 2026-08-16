using System.ComponentModel.DataAnnotations;
using Stockservice.Enums;

namespace Stockservice.Models;

public class StockReservation
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid InvoiceId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public DateTime ReservedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public ReservationStatus Status { get; set; }

    public Product Product { get; set; } = null!;
}
