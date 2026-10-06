using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web2.Data;
using Web2.Models.Domain;
using Web2.Models.DTO;
using Web2.Repositories;
using Web2.CustomActionFilters;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization; // Thư viện bắt buộc cho tính năng phân quyền

namespace Web2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BooksController> _logger;

        // Đã sửa hàm khởi tạo để nhận ILogger
        public BooksController(AppDbContext dbContext, IBookRepository bookRepository, ILogger<BooksController> logger)
        {
            _dbContext = dbContext;
            _bookRepository = bookRepository;
            _logger = logger;
        }

        // GET http://localhost:port/api/books/get-all-books
        [HttpGet("get-all-books")]
        [Authorize(Roles = "Read")] // Bắt buộc đăng nhập tài khoản có quyền Read
        public IActionResult GetAll([FromQuery] string? filterOn, [FromQuery] string? filterQuery)
        {
            // Bắt đầu ghi log
            _logger.LogInformation("GetAll Book Action method was invoked");
            _logger.LogWarning("This is a warning log");
            _logger.LogError("This is a error log");

            var allBooks = _bookRepository.GetAllBooks(filterOn, filterQuery);

            // Ghi log kết quả truy xuất dưới dạng chuỗi JSON
            _logger.LogInformation($"Finished GetAllBook request with data {JsonSerializer.Serialize(allBooks)}");

            return Ok(allBooks);
        }

        // GET http://localhost:port/api/books/get-book-by-id/1
        [HttpGet]
        [Route("get-book-by-id/{id:int}")]
        [Authorize(Roles = "Read")] // Giới hạn quyền Đọc
        public IActionResult GetBookById([FromRoute] int id)
        {
            var bookWithIdDTO = _bookRepository.GetBookById(id);
            return Ok(bookWithIdDTO);
        }

        // POST http://localhost:port/api/books/add-book
        [HttpPost("add-book")]
        [Authorize(Roles = "Write")] // Giới hạn quyền Ghi
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
        [Authorize(Roles = "Write")] // Giới hạn quyền Ghi
        public IActionResult UpdateBookById(int id, [FromBody] AddBookRequestDTO bookDTO)
        {
            var updateBook = _bookRepository.UpdateBookById(id, bookDTO);
            return Ok(updateBook);
        }

        // DELETE http://localhost:port/api/books/delete-book-by-id/{id}
        [HttpDelete("delete-book-by-id/{id}")]
        [Authorize(Roles = "Write")] // Giới hạn quyền Ghi
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

            // --- BỔ SUNG GIẢI BÀI TẬP PHẦN 6 ---

            // Bài tập 4 & 13: Kiểm tra PublisherID có tồn tại trong CSDL không
            var publisherExists = _dbContext.Publishers.Any(p => p.Id == addBookRequestDTO.PublisherID);
            if (!publisherExists)
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.PublisherID),
                    "Nhà xuất bản không tồn tại! Vui lòng nhập ID hợp lệ.");
            }

            // Bài tập 9: Kiểm tra sách bắt buộc phải có ít nhất 1 tác giả
            if (addBookRequestDTO.AuthorIds == null || !addBookRequestDTO.AuthorIds.Any())
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.AuthorIds),
                    "Mỗi cuốn sách phải có ít nhất 1 tác giả.");
            }
            else
            {
                // Bài tập 5: Kiểm tra xem các AuthorID gửi lên có thực sự tồn tại không
                foreach (var authorId in addBookRequestDTO.AuthorIds)
                {
                    var authorExists = _dbContext.Authors.Any(a => a.Id == authorId);
                    if (!authorExists)
                    {
                        ModelState.AddModelError(nameof(addBookRequestDTO.AuthorIds),
                            $"Tác giả với ID = {authorId} không tồn tại trong hệ thống!");
                    }
                }
            }

            // Bài tập 12: Kiểm tra Title không được trùng trong cùng 1 Publisher
            var isDuplicateTitle = _dbContext.Books.Any(b => b.Title == addBookRequestDTO.Title && b.PublisherID == addBookRequestDTO.PublisherID);
            if (isDuplicateTitle)
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.Title),
                    "Tên sách này đã tồn tại trong cùng một nhà xuất bản. Vui lòng chọn tên khác!");
            }

            // ------------------------------------

            if (ModelState.ErrorCount > 0)
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}