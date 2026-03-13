using InventoryService.Application.Consumers;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Services;
using InventoryService.Domain.Interfaces;
using InventoryService.Infrastructure.Data;
using InventoryService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CẤU HÌNH CORS
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Cho phép tất cả các nguồn
              .AllowAnyMethod()   // Cho phép tất cả các Method (GET, POST...)
              .AllowAnyHeader();  // Cho phép tất cả các Header
    });
});

// ==========================================
// 2. ĐĂNG KÝ DEPENDENCY INJECTION & DATABASE
// ==========================================
builder.Services.AddControllers();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryAppService, InventoryAppService>();

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==========================================
// 3. CẤU HÌNH MASSTRANSIT & RABBITMQ
// ==========================================
builder.Services.AddMassTransit(x =>
{
    // [ĐỢI LỆNH] - Bước 3 sẽ mở comment và thêm Consumer vào đây
     x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");

        cfg.Host(rabbitConfig["Host"], "/", h =>
        {
            h.Username(rabbitConfig["Username"]);
            h.Password(rabbitConfig["Password"]);
        });

        // Tự động tạo Queue và map với Consumer tương ứng
        cfg.ConfigureEndpoints(context);
    });
});

// ==========================================
// 4. SWAGGER & API EXPLORER
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Kích hoạt CORS (Bắt buộc phải nằm trước Authorization và MapControllers)
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();