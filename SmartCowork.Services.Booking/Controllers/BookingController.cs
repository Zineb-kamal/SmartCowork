using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCowork.Services.Booking.DTOs;
using SmartCowork.Services.Booking.Services;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IMapper _mapper;

    public BookingController(IBookingService bookingService, IMapper mapper)
    {
        _bookingService = bookingService;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetAll()
    {
        var bookings = await _bookingService.GetAllBookingsAsync();
        return Ok(_mapper.Map<IEnumerable<BookingResponseDto>>(bookings));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<BookingResponseDto>> Get(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
            return NotFound();

        // Vérifier si l'utilisateur actuel est le propriétaire de la réservation ou un Admin/Staff
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (booking.UserId.ToString() != userId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            return Forbid();

        return Ok(_mapper.Map<BookingResponseDto>(booking));
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetByUser(Guid userId)
    {
        // Vérifier si l'utilisateur demande ses propres réservations ou si c'est un Admin/Staff
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId.ToString() != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            return Forbid();

        var bookings = await _bookingService.GetBookingsByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<BookingResponseDto>>(bookings));

    }

    [HttpGet("space/{spaceId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetBySpace(Guid spaceId)
    {
        var bookings = await _bookingService.GetBookingsBySpaceAsync(spaceId);
        return Ok(_mapper.Map<IEnumerable<BookingResponseDto>>(bookings));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BookingResponseDto>> Create([FromBody] BookingCreateDto dto)
    {
        // Vérifier si l'utilisateur crée une réservation pour lui-même ou si c'est un Admin/Staff
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (dto.UserId.ToString() != userId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            return Forbid();

        var booking = _mapper.Map<SmartCowork.Services.Booking.Models.Booking>(dto);
        var createdBooking = await _bookingService.CreateBookingAsync(booking);
        var responseDto = _mapper.Map<BookingResponseDto>(createdBooking);
        return CreatedAtAction(nameof(Get), new { id = responseDto.Id }, responseDto);

    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] BookingUpdateDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var existingBooking = await _bookingService.GetBookingByIdAsync(id);
        if (existingBooking == null)
            return NotFound();

        // Vérification d'autorisation
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (existingBooking.UserId.ToString() != userId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            return Forbid();

        // Mettre à jour l'entité existante directement au lieu d'en créer une nouvelle
        _mapper.Map(dto, existingBooking);

        // Conserver certaines propriétés que l'utilisateur ne devrait pas pouvoir modifier
        if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
        {
            existingBooking.UserId = Guid.Parse(userId); // Empêche la modification du propriétaire
        }

        // Certaines valeurs pourraient être préservées quelle que soit l'autorisation
        existingBooking.CreatedAt = existingBooking.CreatedAt; // Préserver la date de création

        await _bookingService.UpdateBookingAsync(existingBooking);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
            return NotFound();

        // Vérifier si l'utilisateur annule sa propre réservation ou si c'est un Admin/Staff
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (booking.UserId.ToString() != userId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            return Forbid();

        await _bookingService.DeleteBookingAsync(id);
        return NoContent();
    }

    [HttpGet("check-availability")]
    public async Task<ActionResult<bool>> CheckAvailability(
        [FromQuery] Guid spaceId,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime)
    {
        var isAvailable = await _bookingService.CheckSpaceAvailabilityAsync(spaceId, startTime, endTime);
        return Ok(isAvailable);
    }

}
