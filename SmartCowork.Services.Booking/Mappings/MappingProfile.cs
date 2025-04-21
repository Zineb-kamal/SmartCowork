using AutoMapper;
using SmartCowork.Services.Booking.DTOs;

namespace SmartCowork.Services.Booking.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<BookingCreateDto, Models.Booking>();

            // Mapping amélioré pour la mise à jour qui ignore certaines propriétés
            CreateMap<BookingUpdateDto, Models.Booking>()
                // Ignorer les propriétés qui ne devraient pas être modifiées via l'API
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Sera défini dans le service
                                                                        // Option pour ignorer les valeurs null (si une propriété est null dans le DTO, 
                                                                        // elle ne remplacera pas la valeur existante)
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
                    srcMember != null));

            CreateMap<Models.Booking, BookingResponseDto>();
        }
    }

}
