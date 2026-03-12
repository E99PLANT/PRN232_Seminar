using ApiGateway.Middlewares;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Gateway",
        Version = "v1",
        Description = "API Gateway for Microservices E-Commerce System"
    });
});

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

// Add Ocelot
builder.Services.AddOcelot();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Global Exception Handler (must be first)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Request Logging
app.UseMiddleware<RequestLoggingMiddleware>();

// CORS
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
    });
}

// Health Check endpoints
app.MapHealthChecks("/health");

// Map Controllers (for HealthController)
app.MapControllers();

// Use Ocelot middleware (must be last)
await app.UseOcelot();

app.Run();
