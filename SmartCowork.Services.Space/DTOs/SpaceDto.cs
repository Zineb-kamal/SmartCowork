using SmartCowork.Services.Space.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartCowork.Services.Space.DTOs
{
    public class SpaceCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Capacity { get; set; }

        [Required]
        public SpaceType Type { get; set; }

        [Required]
        public SpaceStatus Status { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        [Range(0, 10000)]
        public decimal PricePerHour { get; set; }

        [Range(0, 50000)]
        public decimal PricePerDay { get; set; }

        [Range(0, 200000)]
        public decimal PricePerWeek { get; set; }

        [Range(0, 1000000)]
        public decimal PricePerMonth { get; set; }

        [Range(0, 10000000)]
        public decimal PricePerYear { get; set; }
    }

    public class SpaceUpdateDto : SpaceCreateDto
    {
        [Required]
        public Guid Id { get; set; }
    }

    public class SpaceResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public SpaceType Type { get; set; }
        public SpaceStatus Status { get; set; }
        public string ImageUrl { get; set; }
        public decimal PricePerHour { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal PricePerYear { get; set; }
    }
}
