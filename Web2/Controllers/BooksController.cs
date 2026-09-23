using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web2.Data;
using Web2.Models.DTO;
using Web2.Models.Domain;

namespace Web2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public BooksController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET http://localhost:port/api/books/get-all-books
        [HttpGet("get-all-books")]
        public IActionResult GetAll()
        {
            var allBooksDomain = _dbContext.Books;

            var allBooksDTO = allBooksDomain.Select(Books => new BookWithAuthorAndPublisherDTO()
            {
                Id = Books.Id,
                Title = Books.Title,
                Description = Books.Description,
                IsRead = Books.IsRead,
                DateRead = Books.IsRead ? Books.DateRead.Value : null,
                Rate = Books.IsRead ? Books.Rate.Value : null,
                Genre = Books.Genre,
                CoverUrl = Books.CoverUrl,
                PublisherName = Books.Publisher.Name,
                AuthorNames = Books.Book_Authors.Select(n => n.Author.FullName).ToList()
            }).ToList();

            return Ok(allBooksDTO);
        }

        // GET http://localhost:port/api/books/get-book-by-id/1
        [HttpGet]
        [Route("get-book-by-id/{id:int}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            // get bookDomain object from DB
            var bookDomain = _dbContext.Books.Include(b => b.Publisher).Include(b => b.Book_Authors).ThenInclude(ba => ba.Author).FirstOrDefault(b => b.Id == id);

            if (bookDomain == null)
            {
                return NotFound(new { message = "Không tìm thấy sách" });
            }

            // map bookDomain object to DTO
            var bookDTO = new BookWithAuthorAndPublisherDTO()
            {
                Id = bookDomain.Id,
                Title = bookDomain.Title,
                Description = bookDomain.Description,
                IsRead = bookDomain.IsRead,
                DateRead = bookDomain.DateRead,
                Rate = bookDomain.Rate,
                Genre = bookDomain.Genre,
                CoverUrl = bookDomain.CoverUrl,
                DateAdded = bookDomain.DateAdded,
                PublisherName = bookDomain.Publisher != null ? bookDomain.Publisher.Name : "Unknown",
                AuthorNames = bookDomain.Book_Authors?.Where(y => y.Author != null).Select(y => y.Author.FullName).ToList() ?? new List<string>()
            };

            return Ok(bookDTO);
        }

        // POST http://localhost:port/api/books/add-book
        [HttpPost("add-book")]
        public ActionResult AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            // check if publisher exists or not
            var publisherDomain = _dbContext.Publishers.FirstOrDefault(x => x.Id == addBookRequestDTO.PublisherID);
            if (publisherDomain == null)
            {
                return NotFound(new { message = "Không tìm thấy NXB" });
            }

            // create a new book domain object
            var bookDomain = new Book()
            {
                Title = addBookRequestDTO.Title,
                Description = addBookRequestDTO.Description,
                IsRead = addBookRequestDTO.IsRead,
                DateRead = addBookRequestDTO.DateRead,
                Rate = addBookRequestDTO.Rate,
                Genre = addBookRequestDTO.Genre,
                CoverUrl = addBookRequestDTO.CoverUrl,
                DateAdded = addBookRequestDTO.DateAdded,
                PublisherID = publisherDomain.Id
            };

            _dbContext.Books.Add(bookDomain);
            _dbContext.SaveChanges();

            // check authors if they exist and then add to Book_Author table
            foreach (var authorId in addBookRequestDTO.AuthorIds)
            {
                var authorDomain = _dbContext.Authors.FirstOrDefault(x => x.Id == authorId);
                if (authorDomain == null)
                {
                    return NotFound(new { message = "Không tìm thấy tác giả" });
                }

                var bookAuthorDomain = new Book_Author()
                {
                    BookId = bookDomain.Id,
                    AuthorId = authorDomain.Id
                };

                _dbContext.Books_Authors.Add(bookAuthorDomain);
                _dbContext.SaveChanges();
            }

            return Ok();
        }
    }
}