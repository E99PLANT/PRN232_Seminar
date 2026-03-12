using InventoryService.Application.Interfaces;
using InventoryService.Application.Services;
using InventoryService.Domain.Interfaces;
using InventoryService.Infrastructure.Data;
using InventoryService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- THÊM ĐOẠN NÀY (1): Cấu hình chính sách CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Cho phép tất cả các nguồn (hoặc điền http://localhost:3000)
              .AllowAnyMethod()   // Cho phép GET, POST, PUT, DELETE...
              .AllowAnyHeader();  // Cho phép tất cả các Header
    });
});
// --------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryAppService, InventoryAppService>();
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- THÊM ĐOẠN NÀY (2): Kích hoạt Middleware CORS ---
// Lưu ý: Phải đặt UseCors TRƯỚC UseAuthorization và MapControllers
app.UseCors("AllowAll");
// --------------------------------------------------

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();