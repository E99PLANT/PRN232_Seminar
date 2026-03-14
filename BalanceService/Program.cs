using BalanceService.Application.Handlers;
using BalanceService.Application.Handlers.Impls;
using BalanceService.Application.Interfaces;
using BalanceService.Application.Services;
using BalanceService.Domain.Interfaces;
using BalanceService.Infrastructure.Data;
using BalanceService.Infrastructure.Messaging;
using BalanceService.Infrastructure.Repos;
using Marten;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region RabbitMQ

var factory = new RabbitMQ.Client.ConnectionFactory { HostName = "localhost" };
var connection = await factory.CreateConnectionAsync();
builder.Services.AddSingleton(connection);

#endregion

#region Db Context

builder.Services.AddDbContext<BalanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Marten (Event Store)

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
builder.Services.AddMarten(options =>
{
    options.Connection(connectionString);
    options.DatabaseSchemaName = "balance_service";
    options.Events.DatabaseSchemaName = "balance_service";

})
.UseLightweightSessions();

#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Register Services/Repos

builder.Services.AddScoped<IBalanceHandler, BalanceHandler>();
builder.Services.AddScoped<IBalanceService, BalancesService>();
builder.Services.AddScoped<IBalanceRepository, BalanceRepository>();
builder.Services.AddHostedService<TransactionApprovedConsumer>();

#endregion

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
