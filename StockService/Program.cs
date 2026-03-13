using JasperFx;
using Marten;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Marten Setup

// 1. Lấy Connection String từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Cấu hình Marten
builder.Services.AddMarten(options =>
{
    if (connectionString.IsEmpty()) return;

    options.Connection(connectionString);

    // Đặt toàn bộ các bảng của Marten vào schema 'stockservice' trên Supabase
    options.Events.DatabaseSchemaName = "stockservice";
    options.DatabaseSchemaName = "stockservice";

    // Tự động tạo bảng nếu chưa tồn tại
    options.AutoCreateSchemaObjects = AutoCreate.All;
})
.UseLightweightSessions();

#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
