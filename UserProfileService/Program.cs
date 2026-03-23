using Microsoft.EntityFrameworkCore;
using UserProfileService.Application.Interfaces;
using UserProfileService.Application.Services;
using UserProfileService.Infrastructure;
using UserProfileService.Infrastructure.Consumers;
using UserProfileService.Infrastructure.Messaging;
using UserProfileService.Infrastructure.Repositories;
using UserProfileService.Infrastructure.Services;

namespace UserProfileService
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

            builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();

            builder.Services.AddScoped<IEventStoreService, EventStoreService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService.Application.Services.UserProfileService>();

            builder.Services.AddHostedService<UserVerifiedConsumer>();
            builder.Services.AddHostedService<UserProfileReplayRpcConsumer>();
            builder.Services.AddHostedService<UserProfileRawEventsRpcConsumer>();
            builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

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
