using Microsoft.AspNetCore.Mvc;
using SmartCowork.Services.Billing.Models.DTOs;
using SmartCowork.Services.Billing.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly IBillingService _billingService;

    public InvoiceController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAllInvoices()
    {
        var invoices = await _billingService.GetAllInvoicesAsync();
        return Ok(invoices);
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetUserInvoices(Guid userId)
    {
        // Vérifier si l'utilisateur demande ses propres factures ou si c'est un admin
        var currentUserId = User.FindFirst("sub")?.Value;
        if (currentUserId != userId.ToString() && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var invoices = await _billingService.GetUserInvoicesAsync(userId);
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
    {
        var invoice = await _billingService.GetInvoiceByIdAsync(id);
        if (invoice == null)
            return NotFound();

        // Vérifier si l'utilisateur peut accéder à cette facture
        var currentUserId = User.FindFirst("sub")?.Value;
        if (invoice.UserId.ToString() != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return Ok(invoice);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] CreateInvoiceDto createInvoiceDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var invoice = await _billingService.CreateInvoiceAsync(createInvoiceDto);
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateInvoice(Guid id, [FromBody] UpdateInvoiceDto updateInvoiceDto)
    {
        if (id != updateInvoiceDto.Id)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var invoice = await _billingService.UpdateInvoiceAsync(updateInvoiceDto);
        if (invoice == null)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteInvoice(Guid id)
    {
        var result = await _billingService.DeleteInvoiceAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
    [HttpGet("{id}/pdf")]
    [Authorize]
    public async Task<IActionResult> GetInvoicePdf(Guid id)
    {
        var invoice = await _billingService.GetInvoiceByIdAsync(id);
        if (invoice == null)
            return NotFound();

        // Vérifier si l'utilisateur peut accéder à cette facture
        var currentUserId = User.FindFirst("sub")?.Value;
        if (invoice.UserId.ToString() != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // Générer le PDF
        var pdfBytes = await _billingService.GenerateInvoicePdfAsync(id);
        if (pdfBytes == null || pdfBytes.Length == 0)
            return NotFound("Impossible de générer le PDF");

        // Retourner le PDF comme un fichier téléchargeable
        return File(pdfBytes, "application/pdf", $"facture-{id}.pdf");
    }
}