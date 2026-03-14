using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Consumers;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. CẤU HÌNH CORS (Để FE Gateway gọi vào không bị chặn)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// 2. CẤU HÌNH CONTROLLERS & SWAGGER
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. CẤU HÌNH MASSTRANSIT & RABBITMQ
builder.Services.AddMassTransit(x =>
{
    // Đăng ký 2 nhân viên gác cổng lắng nghe từ RabbitMQ
    x.AddConsumer<InventoryReservedConsumer>();
    x.AddConsumer<StockReservationFailedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rabbitConfig["Host"], "/", h =>
        {
            h.Username(rabbitConfig["Username"]);
            h.Password(rabbitConfig["Password"]);
        });

        cfg.ConfigureEndpoints(context); // Lệnh thần thánh tự tạo Queue
    });
});

// 4. CẤU HÌNH DATABASE
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 5. DEPENDENCY INJECTION
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderAppService, OrderAppService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();