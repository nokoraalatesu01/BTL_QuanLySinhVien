using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Data;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Services;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<EnrollmentProtectedDeletionService>();


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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/StudentsPages", "AdminOnly");
    options.Conventions.AuthorizeFolder("/TeachersPages", "AdminOnly");
    options.Conventions.AuthorizeFolder("/CoursesPages", "AdminOnly");
});
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
    await SeedData.Initialize(scope.ServiceProvider, app.Configuration);
}

app.Run();
