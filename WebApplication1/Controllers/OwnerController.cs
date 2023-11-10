using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dto;
using WebApplication1.Interface;
using WebApplication1.Models;
using WebApplication1.Repository;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : Controller
    {
        private readonly IOwnerRepository _ownerRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;

        public OwnerController(IOwnerRepository ownerRepository, ICountryRepository countryRepository, IMapper mapper)
        {
            _ownerRepository = ownerRepository;
            _countryRepository = countryRepository;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Owner>))]
        public IActionResult GetOwners()
        {
            var owners = _mapper.Map<List<OwnerDto>>(_ownerRepository.GetOwners());
            if(!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(owners);

        }
        [HttpGet("{ownerId}")]
        [ProducesResponseType(200, Type = typeof(Owner))]
        [ProducesResponseType(400)]
        public IActionResult GetOwner(int ownerId) 
        {
            if (!_ownerRepository.OwnerExists(ownerId)) return NotFound() ;
            var owner = _mapper.Map<OwnerDto>(_ownerRepository.GetOwner(ownerId));
            if(!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(owner);
        }

        [HttpGet("{ownerId}/pokemon")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Owner>))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonByOwner(int ownerId)
        {
            if (!_ownerRepository.OwnerExists(ownerId)) return NotFound();
            var pokemonByOwner = _mapper.Map<List<PokemonDto>>(_ownerRepository.GetPokemonByOwner(ownerId));
            if(!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(pokemonByOwner);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateOwner([FromQuery] int countryId,[FromBody] OwnerDto ownerCreate)
        {
            if (ownerCreate == null)
                return BadRequest(ModelState);
            var oldOwner = _ownerRepository.GetOwners()
                .Where(o => o.FirstName.Trim().ToUpper() == ownerCreate.FirstName.TrimEnd().ToUpper() 
                && o.LastName.Trim().ToUpper() == ownerCreate.LastName.TrimEnd().ToUpper())
                .FirstOrDefault();
            if (oldOwner != null)
            {
                ModelState.AddModelError("", "Owner already exists");
                return StatusCode(402, ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var ownerMap = _mapper.Map<Owner>(ownerCreate);
            ownerMap.Country = _countryRepository.GetCountry(countryId);
            var ownerCreated = _ownerRepository.CreateOwner(ownerMap);
            if (!ownerCreated)
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }
            return Ok("Successfully created");

        }

        [HttpPut("{ownerId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateCountry(int ownerId, [FromBody] OwnerDto updateOwner)
        {
            if (updateOwner == null)
                return BadRequest(ModelState);
            if (ownerId != updateOwner.Id)
                return BadRequest(ModelState);
            if (!_ownerRepository.OwnerExists(ownerId))
                return NotFound();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ownerMap = _mapper.Map<Owner>(updateOwner);
            var ownerUpdated = _ownerRepository.UpdateOwner(ownerMap);
            if (!ownerUpdated)
            {
                ModelState.AddModelError("", "Something went wrong updating owner");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{ownerId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteCountry(int ownerId)
        {
            var isExistOwner = _ownerRepository.OwnerExists(ownerId);
            if (!isExistOwner)
                return NotFound();
            var OwnerDelete = _ownerRepository.GetOwner(ownerId);
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var deletedOwner = _ownerRepository.DeleteOwner(OwnerDelete);
            if (!deletedOwner)
            {
                ModelState.AddModelError("", "Something went wrong deleting owner");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

    }
}
