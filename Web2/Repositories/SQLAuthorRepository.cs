using Microsoft.EntityFrameworkCore;
using Web2.Data;
using Web2.Models.Domain;
using Web2.Models.DTO;

namespace Web2.Repositories
{
    public class SQLAuthorRepository : IAuthorRepository
    {
        private readonly AppDbContext _dbContext;

        public SQLAuthorRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<AuthorDTO> GellAllAuthors(string? filterOn = null, string? filterQuery = null,
                                      string? sortBy = null, bool isAscending = true,
                                      int pageNumber = 1, int pageSize = 1000)
        {
            var allAuthors = _dbContext.Authors.Select(author => new AuthorDTO()
            {
                Id = author.Id,
                FullName = author.FullName
            }).AsQueryable();

            // 1. Filtering (Lọc dữ liệu theo tên tác giả)
            if (string.IsNullOrWhiteSpace(filterOn) == false && string.IsNullOrWhiteSpace(filterQuery) == false)
            {
                if (filterOn.Equals("fullname", StringComparison.OrdinalIgnoreCase))
                {
                    allAuthors = allAuthors.Where(x => x.FullName.Contains(filterQuery));
                }
            }

            // 2. Sorting (Sắp xếp dữ liệu)
            if (string.IsNullOrWhiteSpace(sortBy) == false)
            {
                if (sortBy.Equals("fullname", StringComparison.OrdinalIgnoreCase))
                {
                    allAuthors = isAscending ? allAuthors.OrderBy(x => x.FullName) : allAuthors.OrderByDescending(x => x.FullName);
                }
            }

            // 3. Pagination (Phân trang)
            var skipResults = (pageNumber - 1) * pageSize;
            return allAuthors.Skip(skipResults).Take(pageSize).ToList();
        }

        public AuthorNoIdDTO GetAuthorById(int id)
        {
            var authorWithIdDomain = _dbContext.Authors.FirstOrDefault(x => x.Id == id);
            if (authorWithIdDomain == null)
            {
                return null;
            }
            var authorNoIdDTO = new AuthorNoIdDTO
            {
                FullName = authorWithIdDomain.FullName,
            };
            return authorNoIdDTO;
        }

        public AddAuthorRequestDTO AddAuthor(AddAuthorRequestDTO addAuthorRequestDTO)
        {
            var authorDomainModel = new Author
            {
                FullName = addAuthorRequestDTO.FullName,
            };
            _dbContext.Authors.Add(authorDomainModel);
            _dbContext.SaveChanges();
            return addAuthorRequestDTO;
        }

        public AuthorNoIdDTO UpdateAuthorById(int id, AuthorNoIdDTO authorNoIdDTO)
        {
            var authorDomain = _dbContext.Authors.FirstOrDefault(n => n.Id == id);
            if (authorDomain != null)
            {
                authorDomain.FullName = authorNoIdDTO.FullName;
                _dbContext.SaveChanges();
                return authorNoIdDTO;
            }
            return null;
        }

        public Author? DeleteAuthorById(int id)
        {
            var authorDomain = _dbContext.Authors.FirstOrDefault(n => n.Id == id);
            if (authorDomain != null)
            {
                _dbContext.Authors.Remove(authorDomain);
                _dbContext.SaveChanges();
                return authorDomain;
            }
            return null;
        }

        // Bài tập mở rộng: Lấy danh sách sách của tác giả
        public List<BookWithAuthorAndPublisherDTO> GetBooksByAuthorId(int id)
        {
            var books = _dbContext.Books_Authors
                .Where(ba => ba.AuthorId == id)
                .Select(ba => ba.Book)
                .Select(book => new BookWithAuthorAndPublisherDTO
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    IsRead = book.IsRead,
                    DateRead = book.DateRead,
                    Rate = book.Rate,
                    Genre = book.Genre,
                    CoverUrl = book.CoverUrl,
                    PublisherName = book.Publisher != null ? book.Publisher.Name : "Unknown",
                    AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
                }).ToList();

            return books;
        }

        public List<AuthorDTO> GellAllAuthors()
        {
            // forward to the parameterized overload using the same default values
            return GellAllAuthors(null, null, null, true, 1, 1000);
        }
    }
}