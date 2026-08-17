namespace BillingService.Dtos;

public class ReservationResponseDto
{
    public Guid ProductId { get; set; }
    public Guid ReservationId { get; set; }
    public Guid InvoiceId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int AvailableBalance { get; set; }
}
