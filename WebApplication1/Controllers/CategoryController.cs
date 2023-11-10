using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dto;
using WebApplication1.Interface;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Category>))]
        public IActionResult GetCategories()
        {
            var categories = _mapper.Map<List<CategoryDto>>(_categoryRepository.GetCategories());
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(categories);
        }
        [HttpGet("{catId}")]
        [ProducesResponseType(200, Type = typeof(Category))]
        [ProducesResponseType(400)]
        public IActionResult GetCategory(int catId)
        {
            if (!_categoryRepository.CategoryExists(catId))
                return NotFound();
            var category = _mapper.Map<CategoryDto>(_categoryRepository.GetCategories(catId));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(category);
        }

        [HttpGet("{categoryId}/pokemon")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonByCategory(int categoryId)
        {
            if (!_categoryRepository.CategoryExists(categoryId))
                return NotFound();
            var category = _mapper.Map<List<PokemonDto>>(_categoryRepository.GetPokemonByCategory(categoryId));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(category);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateCategory([FromBody] CategoryDto categoryCreate)
        {
            if(categoryCreate == null)
                return BadRequest(ModelState);
            var category = _categoryRepository.GetCategories()
                .Where(c => c.Name.Trim().ToUpper() ==  categoryCreate.Name.TrimEnd().ToUpper())
                .FirstOrDefault();
            if (category != null)
            {
                ModelState.AddModelError("", "Category already exists");
                return StatusCode(402, ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var categoryMap = _mapper.Map<Category>(categoryCreate);
            var categoryCreated = _categoryRepository.CreateCategory(categoryMap);
            if (!categoryCreated)
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }
            return Ok("Successfully created");
            
        }
        [HttpPut("{categoryId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateCategory(int categoryId, [FromBody] CategoryDto updateCategory)
        {
            if (updateCategory == null)
                return BadRequest(ModelState);
            if(categoryId != updateCategory.Id)
                return BadRequest(ModelState);
            if (!_categoryRepository.CategoryExists(categoryId))
                return NotFound();
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var categoryMap = _mapper.Map<Category>(updateCategory);
            var categoryUpdated = _categoryRepository.UpdateCategory(categoryMap);
            if (!categoryUpdated)
            {
                ModelState.AddModelError("", "Something went wrong updating category");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{categoryId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteCategory(int categoryId)
        {
            var isExistCategory = _categoryRepository.CategoryExists(categoryId);
            if(!isExistCategory)
                return NotFound();
            var categoryToDelete = _categoryRepository.GetCategories(categoryId);
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var deletedCategory = _categoryRepository.DeleteCategory(categoryToDelete);
            if (!deletedCategory)
            {
                ModelState.AddModelError("", "Something went wrong deleting category");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }   

    }
}
