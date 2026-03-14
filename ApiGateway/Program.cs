using ApiGateway.Middlewares;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
// Docker dùng ocelot.docker.json (routing qua container name)
// Local dùng ocelot.json (routing qua localhost)
var isDocker = Environment.GetEnvironmentVariable("DOCKER_ENV") == "true";
var ocelotFile = isDocker ? "ocelot.docker.json" : "ocelot.json";
builder.Configuration.AddJsonFile(ocelotFile, optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Health Checks
builder.Services.AddHealthChecks();

// Add Ocelot + Swagger for Ocelot
builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

// Global Exception Handler (must be first)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Request Logging
app.UseMiddleware<RequestLoggingMiddleware>();

// CORS
app.UseCors("AllowAll");

// Swagger aggregation — gom tất cả service Swagger vào 1 trang
app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs";
});

// Health Check endpoints
app.MapHealthChecks("/health");

// Map Controllers (for HealthController)
app.MapControllers();

// Use Ocelot middleware (must be last)
await app.UseOcelot();

app.Run();
