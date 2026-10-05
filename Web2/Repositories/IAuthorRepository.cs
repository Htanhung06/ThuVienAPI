using Web2.Models.Domain;
using Web2.Models.DTO;

namespace Web2.Repositories
{
    public interface IAuthorRepository
    {
        List<AuthorDTO> GellAllAuthors();
        AuthorNoIdDTO GetAuthorById(int id);
        AddAuthorRequestDTO AddAuthor(AddAuthorRequestDTO addAuthorRequestDTO);
        AuthorNoIdDTO UpdateAuthorById(int id, AuthorNoIdDTO authorNoIdDTO);
        Author? DeleteAuthorById(int id);
        List<BookWithAuthorAndPublisherDTO> GetBooksByAuthorId(int id);
        List<AuthorDTO> GellAllAuthors(string? filterOn = null, string? filterQuery = null,
                               string? sortBy = null, bool isAscending = true,
                               int pageNumber = 1, int pageSize = 1000);
    }
}
