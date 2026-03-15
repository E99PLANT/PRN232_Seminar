using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace AuthService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();

            builder.Services.AddScoped<IEventStoreService, EventStoreService>();
            builder.Services.AddScoped<IAuthService, AuthService.Application.Services.AuthService>();

            builder.Services.AddHttpClient<IEmailSender, ResendEmailSender>();
           // builder.Services.AddHttpClient<IProfileProvisionClient, ProfileProvisionClient>();
            builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}