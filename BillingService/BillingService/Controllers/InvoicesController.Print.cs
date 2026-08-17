using BillingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Controllers;

public partial class InvoicesController
{
    [HttpPost("{id:guid}/print")]
    public async Task<ActionResult> PrintInvoice(Guid id)
    {
        try
        {
            var result = await _invoiceService.PrintInvoiceAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Nota não encontrada",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Impressão inválida",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (HttpRequestException)
        {
            return Problem(title: "Serviço de estoque indisponível. Tente novamente.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (Exception ex)
        {
            return Problem(title: "Erro ao imprimir nota fiscal", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
