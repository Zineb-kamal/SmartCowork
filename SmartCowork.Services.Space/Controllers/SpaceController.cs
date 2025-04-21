using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCowork.Services.Space.DTOs;
using SmartCowork.Services.Space.Models;
using SmartCowork.Services.Space.Services;

namespace SmartCowork.Services.Space.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpaceController : ControllerBase
    {
        private readonly ISpaceService _spaceService;
        private readonly IMapper _mapper;

        public SpaceController(ISpaceService spaceService, IMapper mapper)
        {
            _spaceService = spaceService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpaceResponseDto>>> GetAll()
        {
            var spaces = await _spaceService.GetAllSpacesAsync();
            return Ok(_mapper.Map<IEnumerable<SpaceResponseDto>>(spaces));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SpaceResponseDto>> Get(Guid id)
        {
            var space = await _spaceService.GetSpaceByIdAsync(id);
            if (space == null)
                return NotFound();
            return Ok(_mapper.Map<SpaceResponseDto>(space));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<SpaceResponseDto>> Create([FromBody] SpaceCreateDto dto)
        {
            var space = _mapper.Map<Models.Space>(dto);
            var createdSpace = await _spaceService.CreateSpaceAsync(space);
            var responseDto = _mapper.Map<SpaceResponseDto>(createdSpace);
            return CreatedAtAction(nameof(Get), new { id = responseDto.Id }, responseDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SpaceUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var space = _mapper.Map<Models.Space>(dto);
            await _spaceService.UpdateSpaceAsync(space);
            return NoContent();
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<SpaceResponseDto>>> GetAvailable()
        {
            var spaces = await _spaceService.GetAvailableSpacesAsync();
            return Ok(_mapper.Map<IEnumerable<SpaceResponseDto>>(spaces));
        }
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<SpaceResponseDto>>> GetByType(SpaceType type)
        {
            var spaces = await _spaceService.GetSpacesByTypeAsync(type);
            return Ok(_mapper.Map<IEnumerable<SpaceResponseDto>>(spaces));
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var space = await _spaceService.GetSpaceByIdAsync(id);
            if (space == null)
                return NotFound();

            await _spaceService.DeleteSpaceAsync(id);
            return NoContent();
        }
    }
}
