using SmartCowork.Services.Billing.Models.DTOs;

namespace SmartCowork.Services.Billing.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(Guid userId);
    }
}
