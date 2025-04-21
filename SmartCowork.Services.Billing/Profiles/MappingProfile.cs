using AutoMapper;
using SmartCowork.Services.Billing.Models;
using SmartCowork.Services.Billing.Models.DTOs;

namespace SmartCowork.Services.Billing.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Invoice mappings
            CreateMap<Invoice, InvoiceDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // InvoiceItem mappings
            CreateMap<InvoiceItem, InvoiceItemDto>();

            // Transaction mappings
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
