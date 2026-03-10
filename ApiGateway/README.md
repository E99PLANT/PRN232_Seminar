# API Gateway

## 📋 Mô tả

API Gateway sử dụng Ocelot để route requests tới các microservices backend.

## 🛠️ Tech Stack

- **Ocelot 24.1.0** - API Gateway framework
- **Swagger** - API documentation
- **ASP.NET Core 8.0** - Web framework

## 🚀 Endpoints

### Health Check
- `GET /health` - Basic health check
- `GET /api/health` - Detailed health with downstream services info
- `GET /api/health/detailed` - Full health status

### Swagger
- `GET /swagger` - Swagger UI

### Gateway Routes

All requests are forwarded to downstream services with the following pattern:

- `/user-service/api/*` → UserService (localhost:5209)
- `/order-service/api/*` → OrderService (localhost:5267)
- `/inventory-service/api/*` → InventoryService (localhost:5254)
- `/payment-service/api/*` → PaymentService (localhost:5253)

## 📁 Structure

```
ApiGateway/
├── Controllers/
│   └── HealthController.cs      # Health check endpoints
├── Middlewares/
│   ├── GlobalExceptionHandlerMiddleware.cs  # Global error handling
│   └── RequestLoggingMiddleware.cs         # Request/Response logging
├── Properties/
│   └── launchSettings.json
├── ocelot.json                  # Ocelot routing configuration
├── Program.cs                   # Application entry point
└── appsettings.json            # Configuration
```

## ⚙️ Configuration

### ocelot.json
Defines routing rules for all downstream services:

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "localhost", "Port": 5209 }
      ],
      "UpstreamPathTemplate": "/user-service/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
    }
  ]
}
```

### appsettings.json
Contains logging levels and downstream service URLs.

## 🔧 Middlewares

### GlobalExceptionHandlerMiddleware
- Catches all unhandled exceptions
- Returns standardized error responses
- Logs errors with details

### RequestLoggingMiddleware
- Logs incoming requests
- Logs response status and duration
- Tracks client IP addresses

## 📊 Usage Examples

### Via API Gateway (Recommended)

```bash
# Create user via Gateway
POST http://localhost:5092/user-service/api/users
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123",
  "fullName": "John Doe"
}

# Get users via Gateway
GET http://localhost:5092/user-service/api/users

# Health check
GET http://localhost:5092/health
```

### Direct Service Access (Development)

```bash
# Bypass Gateway - directly to UserService
GET http://localhost:5209/api/users
```

## 🚦 Health Check Response

```json
{
  "status": "Healthy",
  "service": "API Gateway",
  "timestamp": "2026-03-10T20:45:00Z",
  "environment": "Development"
}
```

## 🔒 CORS Policy

Currently configured to allow all origins, methods, and headers for development.

**Production:** Update CORS policy in `Program.cs` to restrict origins.

## 📝 Logging

Logs are written to console with the following levels:
- **Information**: Request/Response logs
- **Warning**: Deprecated usage
- **Error**: Exceptions and failures

## 🐛 Error Handling

All errors are caught by `GlobalExceptionHandlerMiddleware` and returned as:

```json
{
  "statusCode": 500,
  "message": "An error occurred while processing your request.",
  "detailed": "Exception details",
  "timestamp": "2026-03-10T20:45:00Z"
}
```

## 🚀 Running

```bash
cd ApiGateway
dotnet run
```

Access at: `http://localhost:5092`

## 📌 Notes

- API Gateway must be started BEFORE accessing microservices through it
- All downstream services should be running for full functionality
- Check health endpoints to verify downstream services status
- Review logs in console for request tracking

## 🔜 Future Enhancements

- [ ] JWT Authentication
- [ ] Rate Limiting per client
- [ ] Request/Response caching
- [ ] Circuit Breaker pattern
- [ ] Service Discovery (Consul)
- [ ] Aggregation endpoints
- [ ] API versioning
