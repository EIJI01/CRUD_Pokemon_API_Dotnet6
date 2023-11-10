using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using WebApplication1.Dto;
using WebApplication1.Interface;
using WebApplication1.Models;
using WebApplication1.Repository;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : Controller
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IReviewerRepository _reviewerRepository;
        private readonly IMapper _mapper;

        public ReviewController(IReviewRepository reviewRepository, IPokemonRepository pokemonRepository, IReviewerRepository reviewerRepository, IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _pokemonRepository = pokemonRepository;
            _reviewerRepository = reviewerRepository;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Review>))]
        public IActionResult GetReviews()
        {
            var reviews = _mapper.Map<List<ReviewDto>>(_reviewRepository.GetReviews());
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(reviews);
        }
        [HttpGet("{reviewId}")]
        [ProducesResponseType(200, Type = typeof(Review))]
        [ProducesResponseType(400)]
        public IActionResult GetReview(int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
                return NotFound();
            var review = _mapper.Map<ReviewDto>(_reviewRepository.GetReview(reviewId));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(review);
        }

        [HttpGet("{pokemonId}/reviews")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Review>))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewOfAPokemon(int pokemonId)
        {
            var reviewOfApokemon = _mapper.Map<List<ReviewDto>>(_reviewRepository.GetReviewsOfAPokemon(pokemonId));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(reviewOfApokemon);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateReview([FromQuery] int reviewerId, [FromQuery] int pokemonId, [FromBody] ReviewDto reviewCreate)
        {
            if (reviewCreate == null)
                return BadRequest(ModelState);
            var oldReview = _reviewRepository.GetReviews()
                .Where(r => r.Title.Trim().ToUpper() == reviewCreate.Title.TrimEnd().ToUpper())
                .FirstOrDefault();
            if (oldReview != null)
            {
                ModelState.AddModelError("", "Title already exists");
                return StatusCode(402, ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var reviewMap = _mapper.Map<Review>(reviewCreate);
            reviewMap.Pokemon = _pokemonRepository.GetPokemon(pokemonId);
            reviewMap.Reviewer = _reviewerRepository.GetReviewer(reviewerId);
            var reviewCreated = _reviewRepository.CreateReview(reviewMap);
            if (!reviewCreated)
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }
            return Ok("Successfully created");

        }

        [HttpPut("{reviewId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateReview(int reviewId, [FromBody] ReviewDto updateReview)
        {
            if (updateReview == null)
                return BadRequest(ModelState);
            if (reviewId != updateReview.Id)
                return BadRequest(ModelState);
            if (!_reviewRepository.ReviewExists(reviewId))
                return NotFound();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var reviewMap = _mapper.Map<Review>(updateReview);
            var reviewUpdated = _reviewRepository.UpdateReview(reviewMap);
            if (!reviewUpdated)
            {
                ModelState.AddModelError("", "Something went wrong updating review");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{reviewId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteReview(int reviewId)
        {
            var isExistReview = _reviewRepository.ReviewExists(reviewId);
            if (!isExistReview)
                return NotFound();
            
            var reviewToDelete = _reviewRepository.GetReview(reviewId);
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var DeletedReview = _reviewRepository.DeleteReview(reviewToDelete);
            if (!DeletedReview)
            {
                ModelState.AddModelError("", "Something went wrong when deleting review");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }


    }
}
