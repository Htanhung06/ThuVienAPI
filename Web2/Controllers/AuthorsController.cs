using Microsoft.AspNetCore.Mvc;
using Web2.Data;
using Web2.Models.DTO;
using Web2.Repositories;

namespace Web2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IAuthorRepository _authorRepository;

        public AuthorsController(AppDbContext dbContext, IAuthorRepository authorRepository)
        {
            _dbContext = dbContext;
            _authorRepository = authorRepository;
        }

        [HttpGet("get-all-author")]
        public IActionResult GetAllAuthor(
    [FromQuery] string? filterOn, [FromQuery] string? filterQuery,
    [FromQuery] string? sortBy, [FromQuery] bool isAscending,
    [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 1000)
        {
            var allAuthors = _authorRepository.GellAllAuthors(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
            return Ok(allAuthors);
        }

        [HttpGet("get-author-by-id/{id}")]
        public IActionResult GetAuthorById(int id)
        {
            var authorWithId = _authorRepository.GetAuthorById(id);
            if (authorWithId == null)
            {
                return NotFound(new { message = "Không tìm thấy tác giả" });
            }
            return Ok(authorWithId);
        }

        [HttpPost("add-author")]
        public IActionResult AddAuthors([FromBody] AddAuthorRequestDTO addAuthorRequestDTO)
        {
            var authorAdd = _authorRepository.AddAuthor(addAuthorRequestDTO);
            return Ok(authorAdd);
        }

        [HttpPut("update-author-by-id/{id}")]
        public IActionResult UpdateAuthorById(int id, [FromBody] AuthorNoIdDTO authorDTO)
        {
            var authorUpdate = _authorRepository.UpdateAuthorById(id, authorDTO);
            if (authorUpdate == null)
            {
                return NotFound(new { message = "Không tìm thấy tác giả để cập nhật" });
            }
            return Ok(authorUpdate);
        }

        [HttpDelete("delete-author-by-id/{id}")]
        public IActionResult DeleteAuthorById(int id)
        {
            var authorDelete = _authorRepository.DeleteAuthorById(id);
            if (authorDelete == null)
            {
                return NotFound(new { message = "Không tìm thấy tác giả để xóa" });
            }
            return Ok(authorDelete);
        }

        // BÀI TẬP MỞ RỘNG: Lấy danh sách sách do tác giả này viết
        [HttpGet("{id}/books")]
        public IActionResult GetBooksByAuthorId(int id)
        {
            var books = _authorRepository.GetBooksByAuthorId(id);
            return Ok(books);
        }
    }
}