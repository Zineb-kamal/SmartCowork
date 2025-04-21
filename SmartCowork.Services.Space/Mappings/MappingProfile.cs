using AutoMapper;
using SmartCowork.Services.Space.DTOs;

namespace SmartCowork.Services.Space.Mappings
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<SpaceCreateDto, Models.Space>();
            CreateMap<SpaceUpdateDto, Models.Space>();
            CreateMap<Models.Space, SpaceResponseDto>();
        }
    }

}
