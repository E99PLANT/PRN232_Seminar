using Microsoft.EntityFrameworkCore;
using UserBehaviorService.Application.Consumers;
using UserBehaviorService.Application.Interfaces;
using UserBehaviorService.Application.Services;
using UserBehaviorService.Infrastructure;
using UserBehaviorService.Infrastructure.Messaging;
using UserBehaviorService.Infrastructure.Repositories;

namespace UserBehaviorService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IUserBehaviorRepository, UserBehaviorRepository>();
            builder.Services.AddScoped<IUserLoginHistoryRepository, UserLoginHistoryRepository>();
            builder.Services.AddScoped<IConsumedMessageRepository, ConsumedMessageRepository>();

            builder.Services.AddScoped<IUserBehaviorAnalyticsService, UserBehaviorAnalyticsService>();

            builder.Services.AddHostedService<UserBehaviorConsumer>();

            builder.Services.AddScoped<IUserBehaviorEventReplayService, UserBehaviorEventReplayService>();
            builder.Services.AddSingleton<IRabbitMqRpcClient, RabbitMqRpcClient>();

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
        }
    }
}
