namespace Stockservice.Dtos;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Balance { get; set; }
    public int AvailableBalance { get; set; }
}
