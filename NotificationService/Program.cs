using Marten;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Handlers;
using NotificationService.Application.Handlers.Impls;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Repos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Db Context

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region RabbitMQ

var factory = new RabbitMQ.Client.ConnectionFactory { HostName = "localhost" };
var connection = await factory.CreateConnectionAsync();
builder.Services.AddSingleton(connection);

#endregion

#region Marten (Event Store)

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
builder.Services.AddMarten(options =>
{
    options.Connection(connectionString);
    options.DatabaseSchemaName = "notification_service";
    options.Events.DatabaseSchemaName = "notification_service";

})
.UseLightweightSessions();

#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Register Services/Repos

builder.Services.AddScoped<INotificationHandler, NotificationHandler>();
builder.Services.AddScoped<INotificationService, NotificationsService>();
builder.Services.AddScoped<INotificationRepo, NotificationRepo>();
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
