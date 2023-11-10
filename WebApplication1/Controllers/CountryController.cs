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
    public class CountryController : Controller
    {
        private readonly ICountryRepository _contryRepository;
        private readonly IMapper _mapper;

        public CountryController(ICountryRepository contryRepository, IMapper mapper)
        {
            _contryRepository = contryRepository;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Country>))]
        public IActionResult GetCountries()
        {
            var contries = _mapper.Map<List<CountryDto>>(_contryRepository.GetCountries());
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(contries);
        }
        [HttpGet("{countryId}")]
        [ProducesResponseType(200, Type = typeof(Country))]
        [ProducesResponseType(400)]
        public IActionResult GetCountry(int countryId)
        {
            if(!_contryRepository.CountryExists(countryId))
                return NotFound();
            var country = _mapper.Map<CountryDto>(_contryRepository.GetCountry(countryId));
            if(!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(country);
        }

        [HttpGet("owner/{ownerId}")]
        [ProducesResponseType(200, Type = typeof(Country))]
        [ProducesResponseType(400)]
        public IActionResult GetCountryByOwner(int ownerId)
        {
            var contryByOwner = _mapper.Map<CountryDto>(_contryRepository.GetCountryByOwner(ownerId));
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(contryByOwner);
        }
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateCountry([FromBody] CountryDto countryCreate)
        {
            if (countryCreate == null)
                return BadRequest(ModelState);
            var country = _contryRepository.GetCountries()
                .Where(c => c.Name.Trim().ToUpper() == countryCreate.Name.TrimEnd().ToUpper())
                .FirstOrDefault();
            if (country != null)
            {
                ModelState.AddModelError("", "Country already exists");
                return StatusCode(402, ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var countryMap = _mapper.Map<Country>(countryCreate);
            var countryCreated = _contryRepository.CreateCountry(countryMap);
            if (!countryCreated)
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }
            return Ok("Successfully created");

        }
        [HttpPut("{countryId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateCountry(int countryId, [FromBody] CountryDto updateCountry)
        {
            if (updateCountry == null)
                return BadRequest(ModelState);
            if (countryId != updateCountry.Id)
                return BadRequest(ModelState);
            if (!_contryRepository.CountryExists(countryId))
                return NotFound();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var countryMap = _mapper.Map<Country>(updateCountry);
            var countryUpdated = _contryRepository.UpdateCountry(countryMap);
            if (!countryUpdated)
            {
                ModelState.AddModelError("", "Something went wrong updating country");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{countryId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteCountry(int countryId)
        {
            var isExistCountry = _contryRepository.CountryExists(countryId);
            if (!isExistCountry)
                return NotFound();
            var countryoDelete = _contryRepository.GetCountry(countryId);
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var deletedCountry = _contryRepository.DeleteCountry(countryoDelete);
            if (!deletedCountry)
            {
                ModelState.AddModelError("", "Something went wrong deleting country");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

    }
}
