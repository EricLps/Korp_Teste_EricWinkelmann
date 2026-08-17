using System.ComponentModel.DataAnnotations;

namespace BillingService.Models;

public class InvoiceItem
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }

    public Guid ProductId { get; set; }

    // reservationid do stock service
    public Guid ReservationId { get; set; }

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public Invoice Invoice { get; set; } = null!;

}