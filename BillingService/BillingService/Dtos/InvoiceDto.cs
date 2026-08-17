namespace BillingService.Dtos;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}

public class InvoiceItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid ReservationId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
