using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web2.Data;
using Web2.Models.Domain;
using Web2.Models.DTO;
using Web2.Repositories;
using Web2.CustomActionFilters;

namespace Web2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IBookRepository _bookRepository;

        public BooksController(AppDbContext dbContext, IBookRepository bookRepository)
        {
            _dbContext = dbContext;
            _bookRepository = bookRepository;
        }

        // GET http://localhost:port/api/books/get-all-books
        [HttpGet("get-all-books")]
        public IActionResult GetAll([FromQuery] string? filterOn, [FromQuery] string? filterQuery)
        {
            var allBooks = _bookRepository.GetAllBooks(filterOn, filterQuery);
            return Ok(allBooks);
        }

        // GET http://localhost:port/api/books/get-book-by-id/1
        [HttpGet]
        [Route("get-book-by-id/{id:int}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            var bookWithIdDTO = _bookRepository.GetBookById(id);
            return Ok(bookWithIdDTO);
        }

        // POST http://localhost:port/api/books/add-book
        [HttpPost("add-book")]
        // [ValidateModel] // Bỏ dấu // ở đầu dòng này nếu bạn đã tạo CustomActionFilter
        public IActionResult AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            // Gọi hàm kiểm tra dữ liệu trước khi xử lý
            if (!ValidateAddBook(addBookRequestDTO))
            {
                return BadRequest(ModelState);
            }

            var bookAdd = _bookRepository.AddBook(addBookRequestDTO);
            return Ok(bookAdd);
        }

        // PUT http://localhost:port/api/books/update-book-by-id/{id}
        [HttpPut("update-book-by-id/{id}")]
        public IActionResult UpdateBookById(int id, [FromBody] AddBookRequestDTO bookDTO)
        {
            var updateBook = _bookRepository.UpdateBookById(id, bookDTO);
            return Ok(updateBook);
        }

        // DELETE http://localhost:port/api/books/delete-book-by-id/{id}
        [HttpDelete("delete-book-by-id/{id}")]
        public IActionResult DeleteBookById(int id)
        {
            var deleteBook = _bookRepository.DeleteBookById(id);
            return Ok(deleteBook);
        }

        #region Private methods
        private bool ValidateAddBook(AddBookRequestDTO addBookRequestDTO)
        {
            if (addBookRequestDTO == null)
            {
                ModelState.AddModelError(nameof(addBookRequestDTO), "Please add book data");
                return false;
            }

            // Kiểm tra Description không được rỗng
            if (string.IsNullOrEmpty(addBookRequestDTO.Description))
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.Description),
                    $"{nameof(addBookRequestDTO.Description)} cannot be null");
            }

            // Kiểm tra Rating phải từ 0 đến 5
            if (addBookRequestDTO.Rate < 0 || addBookRequestDTO.Rate > 5)
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.Rate),
                    $"{nameof(addBookRequestDTO.Rate)} cannot be less than 0 and more than 5");
            }

            if (ModelState.ErrorCount > 0)
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}