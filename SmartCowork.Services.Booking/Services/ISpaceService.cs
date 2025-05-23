﻿// Services/ISpaceService.cs
using System;
using System.Threading.Tasks;

namespace SmartCowork.Services.Booking.Services
{
    public interface ISpaceService
    {
        Task<SpaceDetailsDto> GetSpaceDetailsAsync(Guid spaceId);
    }

    public class SpaceDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }
        public decimal PricePerHour { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal PricePerYear { get; set; }
    }
}