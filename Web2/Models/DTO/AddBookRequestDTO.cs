namespace Web2.Models.DTO
{
    public class AddBookRequestDTO
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public int? Rate { get; set; }
        public string? Genre { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime DateAdded { get; set; }

        // Cần có ID của Nhà xuất bản và danh sách ID của các Tác giả để tạo liên kết
        public int PublisherID { get; set; }
        public List<int> AuthorIds { get; set; }
    }
}
