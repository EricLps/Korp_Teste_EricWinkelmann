using BillingService.Dtos;
using BillingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    public InvoicesController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<ActionResult<List<InvoiceDto>>> GetInvoices()
    {
        try
        {
            var result = await _invoiceService.GetInvoicesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem(title: "Erro ao buscar notas fiscais", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] CreateInvoiceRequest request)
    {
        try
        {
            var result = await _invoiceService.CreateInvoiceAsync(request);
            return CreatedAtAction(nameof(GetInvoices), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Dados inválidos da nota fiscal",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            return Problem(title: "Erro ao criar nota fiscal", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
