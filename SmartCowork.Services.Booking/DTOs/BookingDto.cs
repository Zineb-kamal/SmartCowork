using SmartCowork.Services.Booking.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartCowork.Services.Booking.DTOs
{
    public class BookingCreateDto
    {
        [Required]
        public Guid SpaceId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public string? Purpose { get; set; }
        public int? AttendeesCount { get; set; } = 0;
    }

    public class BookingUpdateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public BookingStatus Status { get; set; }
    }

    public class BookingResponseDto
    {
        public Guid Id { get; set; }
        public Guid SpaceId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}