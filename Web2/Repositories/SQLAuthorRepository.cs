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

        public List<AuthorDTO> GellAllAuthors()
        {
            var allAuthorsDomain = _dbContext.Authors.ToList();
            var allAuthorDTO = new List<AuthorDTO>();
            foreach (var authorDomain in allAuthorsDomain)
            {
                allAuthorDTO.Add(new AuthorDTO()
                {
                    Id = authorDomain.Id,
                    FullName = authorDomain.FullName
                });
            }
            return allAuthorDTO;
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
    }
}