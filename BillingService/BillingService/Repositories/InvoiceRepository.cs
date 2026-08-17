using BillingService.Data;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Repositories;

public class InvoiceRepository
{
    private readonly BillingDbContext _context;

    public InvoiceRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .Include(invoice => invoice.Items)
            .OrderByDescending(invoice => invoice.CreatedAt)
            .ToListAsync();
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        return await _context.Invoices
            .Include(invoice => invoice.Items)
            .FirstOrDefaultAsync(invoice => invoice.Id == id);
    }

    public async Task AddAsync(Invoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
        await _context.SaveChangesAsync();
    }
    
    public async Task<List<Invoice>> GetExpiredInvoicesAsync(DateTime now)
    {
        return await _context.Invoices
            .Include(invoice => invoice.Items)
            .Where(invoice => invoice.Status == InvoiceStatus.Open && invoice.ExpiresAt <= now)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
