using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Data;
using BTL_QuanLySinhVien.Models;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
  
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI(); 

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Bật xác thực và phân quyền (Thứ tự cực kỳ quan trọng: Authentication phải đứng trước Authorization)
app.UseAuthentication();
app.UseAuthorization();

// Map các Endpoint cho Razor Pages và Web API Controllers
app.MapRazorPages();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Gọi hàm khởi tạo dữ liệu mẫu
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã xảy ra lỗi khi khởi tạo cơ sở dữ liệu.");
    }
}

app.Run();
