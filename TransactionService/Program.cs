using Marten;
using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Projectors;
using TransactionService.Application.Services;
using TransactionService.Domain.Interfaces;
using TransactionService.Infrastructure.Data;
using TransactionService.Infrastructure.Repos;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Marten;
//using Wolverine;
//using Wolverine.EntityFrameworkCore;
//using Wolverine.Marten;
//using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Db Context

builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Wolverine

builder.Host.UseWolverine(opts =>
{
    // Tự động tích hợp Transactional Outbox với Marten
    opts.Policies.AutoApplyTransactions();

    // Tích hợp với EF Core để cùng quản lý Transaction
    opts.UseEntityFrameworkCoreTransactions();

    opts.Discovery.IncludeAssembly(typeof(TransactionProjector).Assembly);
});

#endregion


#region Marten (Event Store)

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

builder.Services.AddMarten(options =>
{
    options.Connection(connectionString);
    options.DatabaseSchemaName = "transaction_service";
    options.Events.DatabaseSchemaName = "transaction_service";
    // Cấu hình lưu trữ Event
    options.Events.MetadataConfig.HeadersEnabled = true;
})
.IntegrateWithWolverine()
.UseLightweightSessions();

#endregion



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Register Services/Repos

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionsService>();

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
