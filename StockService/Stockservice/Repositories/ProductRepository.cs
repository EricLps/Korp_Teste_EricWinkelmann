using Microsoft.EntityFrameworkCore;
using Stockservice.Data;
using Stockservice.Enums;
using Stockservice.Models;

namespace Stockservice.Repositories;

public class ProductRepository
{
    private readonly StockDbContext _context;

    public ProductRepository(StockDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(product => product.Reservations)
            .OrderBy(product => product.Code)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(product => product.Reservations)
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    public async Task<Product?> GetByCodeAsync(string code)
    {
        return await _context.Products
            .FirstOrDefaultAsync(product => product.Code == code);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task AddReservationAsync(StockReservation reservation)
    {
        await _context.StockReservations.AddAsync(reservation);
        await _context.SaveChangesAsync();
    }

    public async Task<List<StockReservation>> GetExpiredReservationsAsync(DateTime now)
    {
        return await _context.StockReservations
            .Include(reservation => reservation.Product)
            .Where(reservation => reservation.Status == ReservationStatus.Active && reservation.ExpiresAt <= now)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
