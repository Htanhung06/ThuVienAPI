using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using Web2.Data;
using Web2.Repositories;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Khởi tạo cấu hình Serilog
var _logger = new LoggerConfiguration()
    .WriteTo.Console() // Ghi ra console
    .WriteTo.File("Logs/Book_log.txt", rollingInterval: RollingInterval.Minute) // Ghi ra file lưu trong thư mục Logs[cite: 34]
    .MinimumLevel.Information()
    .CreateLogger();

// Xóa các log mặc định và thêm Serilog
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(_logger);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ==========================================
// CẤU HÌNH SWAGGER VÀ BẢO MẬT JWT (PHẦN 8)
// ==========================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Book API", Version = "v1" });

    // Định nghĩa phương thức xác thực Bearer Token
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Nhập 'Bearer' [khoảng trắng] và theo sau là chuỗi token của bạn. Ví dụ: Bearer eyJhbGci..."
    });

    // Yêu cầu Swagger gắn Token vào header khi gọi API
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                },
                Scheme = "Oauth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// ==========================================
// CẤU HÌNH CƠ SỞ DỮ LIỆU & IDENTITY (PHẦN 8)
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Database chuyên biệt quản lý User & Phân quyền
builder.Services.AddDbContext<BookAuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BookAuthConnection")));

builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("Book")
    .AddEntityFrameworkStores<BookAuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(option =>
{
    option.Password.RequireDigit = false;
    option.Password.RequireLowercase = false;
    option.Password.RequireNonAlphanumeric = false;
    option.Password.RequireUppercase = false;
    option.Password.RequiredLength = 6;
    option.Password.RequiredUniqueChars = 1;
});

// ==========================================
// ĐĂNG KÝ REPOSITORIES
// ==========================================
builder.Services.AddScoped<IBookRepository, SQLBookRepository>();
builder.Services.AddScoped<IAuthorRepository, SQLAuthorRepository>();
builder.Services.AddScoped<IPublisherRepository, SQLPublisherRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// ==========================================   
// CẤU HÌNH XÁC THỰC JWT (PHẦN 8)
// ==========================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// BẮT BUỘC ĐẶT TRƯỚC AUTHORIZATION
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();