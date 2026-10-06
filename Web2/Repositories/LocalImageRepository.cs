using Web2.Data;
using Web2.Models.Domain;

namespace Web2.Repositories
{
    public class LocalImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _webHostEnvironment;   
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;

        public LocalImageRepository(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public Image Upload(Image image)
        {
            // 1. Xác định đường dẫn tới thư mục Images
            var folderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Images");

            // 2. QUAN TRỌNG: Kiểm tra và tự động tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 3. Định nghĩa đường dẫn lưu file chi tiết
            var localFilePath = Path.Combine(folderPath, $"{image.FileName}{image.FileExtension}");

            // 4. Lưu file vào thư mục local
            using var stream = new FileStream(localFilePath, FileMode.Create);
            image.File.CopyTo(stream);

            // 5. Tạo đường dẫn URL
            var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Images/{image.FileName}{image.FileExtension}";
            image.FilePath = urlFilePath;

            // 6. Lưu thông tin vào CSDL
            _dbContext.Images.Add(image);
            _dbContext.SaveChanges();

            return image;
        }

        public List<Image> GetAllInfoImages()
        {
            return _dbContext.Images.ToList();
        }

        public (byte[], string, string) DownloadFile(int Id)
        {
            var FileById = _dbContext.Images.Where(x => x.Id == Id).FirstOrDefault();
            if (FileById == null) throw new Exception("File not found");

            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Images", $"{FileById.FileName}{FileById.FileExtension}");
            var stream = File.ReadAllBytes(path);
            var fileName = FileById.FileName + FileById.FileExtension;

            return (stream, "application/octet-stream", fileName);
        }
    }
}