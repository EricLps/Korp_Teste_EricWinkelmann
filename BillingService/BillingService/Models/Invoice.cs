using System.ComponentModel.DataAnnotations;
using BillingService.Enums;

namespace BillingService.Models;

public class Invoice
{
    public Guid Id { get; set; }

    public int Number { get; set; }

    public InvoiceStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}
