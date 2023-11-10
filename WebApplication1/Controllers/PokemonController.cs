using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Interface;
using WebApplication1.Models;
using WebApplication1.Repository;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : Controller
    {
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;

        public PokemonController(IPokemonRepository pokemonRepository,IReviewRepository reviewRepository , IMapper mapper)
        {
            _pokemonRepository = pokemonRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        public IActionResult GetPokemons()
        {
            var pokemons = _mapper.Map<List<PokemonDto>>(_pokemonRepository.GetPokemons());
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(pokemons);
        }

        [HttpGet("{pokeId}")]
        [ProducesResponseType(200, Type = typeof(Pokemon))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemon(int pokeId) 
        {
            if (!_pokemonRepository.PokemonExists(pokeId))
                return NotFound();
            var pokemon = _mapper.Map<PokemonDto>(_pokemonRepository.GetPokemon(pokeId));
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(pokemon);
        }

        [HttpGet("{pokeId}/rating")]
        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonRating(int pokeId)
        {
            if(!_pokemonRepository.PokemonExists(pokeId))
                return NotFound();
            var pokemonRating = _pokemonRepository.GetPokemonRating(pokeId);
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(pokemonRating);

        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreatePokemon([FromQuery] int ownerId, [FromQuery] int countryId, [FromBody] PokemonDto pokemonCreate)
        {
            if (pokemonCreate == null)
                return BadRequest(ModelState);
            var oldPokemon = _pokemonRepository.GetPokemons()
                .Where(p => p.Name.Trim().ToUpper() == pokemonCreate.Name.TrimEnd().ToUpper())
                .FirstOrDefault();
            if (oldPokemon != null)
            {
                ModelState.AddModelError("", "Pokemon already exists");
                return StatusCode(402, ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var pokemonMap = _mapper.Map<Pokemon>(pokemonCreate);
            var ownerCreated = _pokemonRepository.CreatePokemon(ownerId, countryId,pokemonMap);
            if (!ownerCreated)
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }
            return Ok("Successfully created");

        }

        [HttpPut("{pokemonId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdatePokemon(int pokemonId, [FromQuery] int ownerId, [FromQuery] int categoryId, [FromBody] PokemonDto updatePokemon)
        {
            if (updatePokemon == null)
                return BadRequest(ModelState);
            if (pokemonId != updatePokemon.Id)
                return BadRequest(ModelState);
            if (!_pokemonRepository.PokemonExists(pokemonId))
                return NotFound();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var pokemonMap = _mapper.Map<Pokemon>(updatePokemon);
            var pokemonUpdated = _pokemonRepository.UpdatePokemon(ownerId, categoryId, pokemonMap);
            if (!pokemonUpdated)
            {
                ModelState.AddModelError("", "Something went wrong updating owner");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{pokemonId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeletePokemon(int pokemonId)
        {
            var isExistPokemon = _pokemonRepository.PokemonExists(pokemonId);
            if (!isExistPokemon)
                return NotFound();
            var reviewsToDelete = _reviewRepository.GetReviewsOfAPokemon(pokemonId);
            var pokemonDelete = _pokemonRepository.GetPokemon(pokemonId);
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var DeletedReviews = _reviewRepository.DeleteReviews(reviewsToDelete.ToList());
            if(!DeletedReviews)
            {
                ModelState.AddModelError("", "Something went wrong when deleting reviews");
                return StatusCode(500, ModelState);
            }
            var deletedPokemon = _pokemonRepository.DeletePokemon(pokemonDelete);
            if (!deletedPokemon)
            {
                ModelState.AddModelError("", "Something went wrong deleting pokemon");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
