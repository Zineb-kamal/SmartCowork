using Microsoft.AspNetCore.Mvc;
using SmartCowork.Services.Billing.Models.DTOs;
using SmartCowork.Services.Billing.Services;
using Microsoft.AspNetCore.Authorization;



[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IBillingService _billingService;

    public PaymentController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TransactionDto>> ProcessPayment([FromBody] CreateTransactionDto createTransactionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Vérifier si l'utilisateur peut payer cette facture
        var invoice = await _billingService.GetInvoiceByIdAsync(createTransactionDto.InvoiceId);
        if (invoice == null)
            return NotFound("Invoice not found");

        var currentUserId = User.FindFirst("sub")?.Value;
        if (invoice.UserId.ToString() != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var transaction = await _billingService.ProcessPaymentAsync(createTransactionDto);
        if (transaction == null)
            return BadRequest("Payment processing failed");

        return Ok(transaction);
    }
}
