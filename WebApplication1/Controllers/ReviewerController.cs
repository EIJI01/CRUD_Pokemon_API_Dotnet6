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
    public class ReviewerController : Controller
    {
        private readonly IReviewerRepository _reviewerRepository;
        private readonly IMapper _mapper;

        public ReviewerController(IReviewerRepository reviewerRepository, IMapper mapper)
        {
            _reviewerRepository = reviewerRepository;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Reviewer>))]
        public IActionResult GetReviewers()
        {
            var reviewers = _mapper.Map<List<ReviewerDto>>(_reviewerRepository.GetReviewers());
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(reviewers);
        }

        [HttpGet("{reviewerId}")]
        [ProducesResponseType(200, Type = typeof(Reviewer))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewer(int reviewerId)
        {
            if (!_reviewerRepository.ReviewerExists(reviewerId))
                return NotFound();
            var reviewer = _mapper.Map<ReviewerDto>(_reviewerRepository.GetReviewer(reviewerId));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(reviewer);
        }

        [HttpGet("{reviewerId}/reviews")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Review>))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewsByReviewer(int reviewerId)
        {
            if (!_reviewerRepository.ReviewerExists(reviewerId))
                return NotFound();
            var reviewByReviewer = _mapper.Map<List<ReviewDto>>(_reviewerRepository.GetReviewsByReviewer(reviewerId));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(reviewByReviewer);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateCountry([FromBody] ReviewerDto reviewerCreate)
        {
            if (reviewerCreate == null)
                return BadRequest(ModelState);
            var oldReviewer = _reviewerRepository.GetReviewers()
                .Where(r => r.FirstName.Trim().ToUpper() == reviewerCreate.FirstName.TrimEnd().ToUpper()
                && r.LastName.Trim().ToUpper() == reviewerCreate.LastName.TrimEnd().ToUpper())
                .FirstOrDefault();
            if (oldReviewer != null)
            {
                ModelState.AddModelError("", "Reviewer already exists");
                return StatusCode(402, ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var reviewerMap = _mapper.Map<Reviewer>(reviewerCreate);
            var reviewerCreated = _reviewerRepository.CreateReviewer(reviewerMap);
            if (!reviewerCreated)
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }
            return Ok("Successfully created");

        }

        [HttpPut("{reviewerId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateReviewer(int reviewerId, [FromBody] ReviewerDto updateReviewer)
        {
            if (updateReviewer == null)
                return BadRequest(ModelState);
            if (reviewerId != updateReviewer.Id)
                return BadRequest(ModelState);
            if (!_reviewerRepository.ReviewerExists(reviewerId))
                return NotFound();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var reviewerMap = _mapper.Map<Reviewer>(updateReviewer);
            var reviewerUpdated = _reviewerRepository.UpdateReviewer(reviewerMap);
            if (!reviewerUpdated)
            {
                ModelState.AddModelError("", "Something went wrong updating reviewer");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{reviewerId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteReviewer(int reviewerId)
        {
            var isExistReviewer = _reviewerRepository.ReviewerExists(reviewerId);
            if (!isExistReviewer)
                return NotFound();
            var reviewerDelete = _reviewerRepository.GetReviewer(reviewerId);
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var deletedReviewer = _reviewerRepository.DeleteReviewer(reviewerDelete);
            if (!deletedReviewer)
            {
                ModelState.AddModelError("", "Something went wrong deleting reviewer");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
