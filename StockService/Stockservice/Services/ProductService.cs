using Stockservice.Dtos;
using Stockservice.Enums;
using Stockservice.Models;
using Stockservice.Repositories;

namespace Stockservice.Services;

public class ProductService
{
    private readonly ProductRepository _repository;

    public ProductService(ProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        var products = await _repository.GetAllAsync();
        var now = DateTime.UtcNow;

        return products
            .Select(product => new ProductDto
            {
                Id = product.Id,
                Code = product.Code,
                Description = product.Description,
                Balance = product.Balance,
                AvailableBalance = GetAvailableBalance(product, now)
            })
            .ToList();
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw new InvalidOperationException("O código do produto é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new InvalidOperationException("A descrição do produto é obrigatória.");
        }

        if (request.Balance < 0)
        {
            throw new InvalidOperationException("O saldo não pode ser negativo.");
        }

        var normalizedCode = request.Code.Trim();
        var existingProduct = await _repository.GetByCodeAsync(normalizedCode);
        if (existingProduct is not null)
        {
            throw new InvalidOperationException("Já existe um produto com este código.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Description = request.Description.Trim(),
            Balance = request.Balance
        };

        await _repository.AddAsync(product);
        return product;
    }

    public async Task<ReservationResultDto> ReserveStockAsync(Guid productId, ReserveProductRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("A quantidade deve ser maior que zero.");
        }

        var product = await _repository.GetByIdAsync(productId);
        if (product is null)
        {
            throw new KeyNotFoundException("Produto não encontrado.");
        }

        var availableBalance = GetAvailableBalance(product, DateTime.UtcNow);
        if (request.Quantity > availableBalance)
        {
            throw new InvalidOperationException("Saldo insuficiente para reservar este item.");
        }

        var reservation = new StockReservation
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            InvoiceId = request.InvoiceId,
            Quantity = request.Quantity,
            ReservedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            Status = ReservationStatus.Active,
            Product = product
        };

        await _repository.AddReservationAsync(reservation);

        return new ReservationResultDto
        {
            ProductId = product.Id,
            ReservationId = reservation.Id,
            InvoiceId = request.InvoiceId,
            Quantity = request.Quantity,
            ExpiresAt = reservation.ExpiresAt,
            AvailableBalance = GetAvailableBalance(product, DateTime.UtcNow) - request.Quantity
        };
    }

    public async Task<int> ConfirmReservationAsync(Guid productId, Guid reservationId)
    {
        var product = await _repository.GetByIdAsync(productId);
        if (product is null)
        {
            throw new KeyNotFoundException("Produto não encontrado.");
        }

        var reservation = product.Reservations
            .FirstOrDefault(item => item.Id == reservationId);

        if (reservation is null)
        {
            throw new KeyNotFoundException("Reserva não encontrada.");
        }

        if (reservation.Status != ReservationStatus.Active)
        {
            throw new InvalidOperationException("A reserva não está ativa para confirmação.");
        }

        if (reservation.ExpiresAt <= DateTime.UtcNow)
        {
            reservation.Status = ReservationStatus.Expired;
            await _repository.SaveChangesAsync();
            throw new InvalidOperationException("A reserva expirou e não pode mais ser confirmada.");
        }

        product.Balance -= reservation.Quantity;
        reservation.Status = ReservationStatus.Confirmed;

        await _repository.SaveChangesAsync();
        return product.Balance;
    }

    public async Task<int> CancelReservationAsync(Guid productId, Guid reservationId)
    {
        var product = await _repository.GetByIdAsync(productId);
        if (product is null)
        {
            throw new KeyNotFoundException("Produto não encontrado.");
        }

        var reservation = product.Reservations
            .FirstOrDefault(item => item.Id == reservationId);

        if (reservation is null)
        {
            throw new KeyNotFoundException("Reserva não encontrada.");
        }

        if (reservation.Status != ReservationStatus.Active)
        {
            throw new InvalidOperationException("A reserva não está ativa para cancelamento.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        await _repository.SaveChangesAsync();

        return GetAvailableBalance(product, DateTime.UtcNow);
    }

    public async Task<int> ExpireReservationsAsync()
    {
        var now = DateTime.UtcNow;
        var reservations = await _repository.GetExpiredReservationsAsync(now);

        foreach (var reservation in reservations)
        {
            reservation.Status = ReservationStatus.Expired;
        }

        if (reservations.Count > 0)
        {
            await _repository.SaveChangesAsync();
        }

        return reservations.Count;
    }

    private static int GetAvailableBalance(Product product, DateTime now)
    {
        var activeReservations = product.Reservations
            .Where(reservation => reservation.Status == ReservationStatus.Active && reservation.ExpiresAt > now)
            .Sum(reservation => reservation.Quantity);

        return product.Balance - activeReservations;
    }
}
