namespace Stockservice.Dtos;

public class ReserveProductRequest
{
    public Guid InvoiceId { get; set; }
    public int Quantity { get; set; }
}
