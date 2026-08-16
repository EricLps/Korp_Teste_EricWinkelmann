using Microsoft.AspNetCore.Mvc;
using Stockservice.Dtos;
using Stockservice.Services;

namespace Stockservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        try
        {
            var products = await _productService.GetProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            return Problem(title: "Erro ao consultar produtos", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            var product = await _productService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, new ProductDto
            {
                Id = product.Id,
                Code = product.Code,
                Description = product.Description,
                Balance = product.Balance,
                AvailableBalance = product.Balance
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Dados inválidos",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            return Problem(title: "Erro ao cadastrar produto", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("{id:guid}/reserve")]
    public async Task<ActionResult<ReservationResultDto>> ReserveStock(Guid id, [FromBody] ReserveProductRequest request)
    {
        try
        {
            var result = await _productService.ReserveStockAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Produto não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Reserva inválida",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            return Problem(title: "Erro ao reservar estoque", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("{id:guid}/confirm-reservation")]
    public async Task<ActionResult<int>> ConfirmReservation(Guid id, [FromBody] ReservationActionRequest request)
    {
        try
        {
            var result = await _productService.ConfirmReservationAsync(id, request.ReservationId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Reserva ou produto não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Confirmação inválida",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            return Problem(title: "Erro ao confirmar reserva", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("{id:guid}/cancel-reservation")]
    public async Task<ActionResult<int>> CancelReservation(Guid id, [FromBody] ReservationActionRequest request)
    {
        try
        {
            var result = await _productService.CancelReservationAsync(id, request.ReservationId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Reserva ou produto não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Cancelamento inválido",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            return Problem(title: "Erro ao cancelar reserva", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
