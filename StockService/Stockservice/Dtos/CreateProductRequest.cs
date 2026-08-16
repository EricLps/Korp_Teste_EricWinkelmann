namespace Stockservice.Dtos;

public class CreateProductRequest
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Balance { get; set; }
}
