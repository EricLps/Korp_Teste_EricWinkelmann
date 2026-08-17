using System.ComponentModel.DataAnnotations;

namespace Stockservice.Models;

public class Product
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string Description { get; set; } = string.Empty;

    [ConcurrencyCheck]
    public int Balance { get; set; }

    public ICollection<StockReservation> Reservations { get; set; } = new List<StockReservation>();
}
