using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Handlers;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Projectors;
using TransactionService.Application.Services;
using TransactionService.Domain.Interfaces;
using TransactionService.Infrastructure.Data;
using TransactionService.Infrastructure.Messaging;
using TransactionService.Infrastructure.Repos;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Register Services/Repos

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionsService>();
builder.Services.AddScoped<IEventBus, RabbitMqEventBus>();
builder.Services.AddSingleton<TransactionToRabbitMqProjection>();

#endregion

#region Db Context

builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Wolverine

builder.Host.UseWolverine(opts =>
{
    // Quét Handler
    opts.Discovery.IncludeAssembly(typeof(TransactionHandler).Assembly);
});

#endregion

#region RabbitMQ

// 1. Lấy thông tin từ file cấu hình (appsettings.json)
var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
var rabbitHost = rabbitConfig["Host"] ?? "localhost";
var rabbitUser = rabbitConfig["Username"] ?? "guest";
var rabbitPass = rabbitConfig["Password"] ?? "guest";

// 2. Thiết lập Factory với đầy đủ thông tin xác thực
var factory = new RabbitMQ.Client.ConnectionFactory
{
    HostName = rabbitHost,
    UserName = rabbitUser,
    Password = rabbitPass
};

var connection = await factory.CreateConnectionAsync();
builder.Services.AddSingleton(connection);

#endregion

#region Marten (Event Store)

builder.Logging.SetMinimumLevel(LogLevel.Debug);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
builder.Services.AddMarten(options =>
{
    options.Connection(connectionString);
    options.DatabaseSchemaName = "transaction_service";
    options.Events.DatabaseSchemaName = "transaction_service";

    // Đăng ký Projection nhưng cấu hình chạy ASYNC
    options.Projections.Add(
               builder.Services.BuildServiceProvider().GetRequiredService<TransactionToRabbitMqProjection>(),
               ProjectionLifecycle.Async
           );


})
.UseLightweightSessions()
.AddAsyncDaemon(DaemonMode.HotCold); // Kích hoạt trình chạy ngầm

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
