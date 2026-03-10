# Microservices E-Commerce Project

## 📋 Tổng quan

Dự án Microservices với Clean Architecture sử dụng .NET 8.0, API Gateway (Ocelot), và PostgreSQL.

## 🏗️ Kiến trúc

### Microservices:
- **ApiGateway** (Port 5092) - Ocelot API Gateway
- **UserService** (Port 5209) - Quản lý users ✅ **HOÀN CHỈNH - MẪU**
- **OrderService** (Port 5267) - Quản lý orders
- **InventoryService** (Port 5254) - Quản lý kho
- **PaymentService** (Port 5253) - Xử lý thanh toán

### Clean Architecture Structure:
```
Service/
├── Domain/              # Core Domain Layer
│   ├── Entities/       # Domain entities
│   └── Interfaces/     # Repository interfaces
├── Application/         # Business Logic Layer
│   ├── DTOs/           # Data Transfer Objects
│   ├── Services/       # Application services
│   └── Interfaces/     # Service interfaces
├── Infrastructure/      # Infrastructure Layer
│   ├── Data/           # DbContext
│   └── Repositories/   # Repository implementations
└── Controllers/         # Presentation Layer
```

## 🛠️ Tech Stack

- **.NET 8.0** - Framework
- **EF Core 8.0.11** - ORM
- **PostgreSQL** - Database
- **Ocelot 24.1.0** - API Gateway
- **Swagger** - API Documentation
- **BCrypt.Net** - Password Hashing

## 🚀 Getting Started

### Prerequisites

1. **.NET 8.0 SDK**
2. **PostgreSQL** (hoặc update connection string trong `appsettings.json`)
3. **Visual Studio 2022** hoặc **VS Code**

### Setup Database

1. Cài đặt PostgreSQL
2. Tạo database cho mỗi service:
   ```sql
   CREATE DATABASE UserServiceDb;
   CREATE DATABASE OrderServiceDb;
   CREATE DATABASE InventoryServiceDb;
   CREATE DATABASE PaymentServiceDb;
   ```

3. Update connection string trong `appsettings.json` của mỗi service:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=UserServiceDb;Username=postgres;Password=YOUR_PASSWORD"
   }
   ```

### Run Migrations (UserService mẫu)

```bash
cd UserService
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Chạy Solution

**Cách 1: Chạy tất cả cùng lúc (PowerShell)**
```powershell
# Terminal 1 - API Gateway
cd ApiGateway
dotnet run

# Terminal 2 - UserService
cd UserService
dotnet run

# Terminal 3 - OrderService
cd OrderService
dotnet run

# Terminal 4 - InventoryService
cd InventoryService
dotnet run

# Terminal 5 - PaymentService
cd PaymentService
dotnet run
```

**Cách 2: Visual Studio**
- Set multiple startup projects
- Right-click Solution → Properties → Multiple Startup Projects
- Chọn "Start" cho tất cả services

## 📚 API Endpoints

### API Gateway Routes

- `http://localhost:5092/user-service/api/*` → UserService
- `http://localhost:5092/order-service/api/*` → OrderService
- `http://localhost:5092/inventory-service/api/*` → InventoryService
- `http://localhost:5092/payment-service/api/*` → PaymentService

### UserService Example (HOÀN CHỈNH)

**Direct access:** `http://localhost:5209/swagger`
**Via Gateway:** `http://localhost:5092/user-service/api/users`

#### Endpoints:
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/username/{username}` - Get user by username
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

#### Create User Example:
```json
POST /api/users
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123",
  "fullName": "John Doe",
  "phoneNumber": "+1234567890"
}
```

## 📁 Project Status

| Service | Middlewares | Health Check | Routes | Controller | Status |
|---------|-------------|--------------|--------|------------|--------|
| **ApiGateway** | ✅ | ✅ | ✅ | ✅ | **HOÀN CHỈNH - MẪU** |

| Service | Domain | Application | Infrastructure | Controller | Status |
|---------|--------|-------------|----------------|------------|--------|
| UserService | ✅ | ✅ | ✅ | ✅ | **HOÀN CHỈNH - MẪU** |
| OrderService | 📁 | 📁 | 📁 | ❌ | Chưa implement |
| InventoryService | 📁 | 📁 | 📁 | ❌ | Chưa implement |
| PaymentService | 📁 | 📁 | 📁 | ❌ | Chưa implement |

📁 = Folder structure sẵn sàng (có .gitkeep)

### ApiGateway Features:
- ✅ **Ocelot Routing** - Routes tới 4 microservices
- ✅ **Global Exception Handler** - Xử lý lỗi toàn cục
- ✅ **Request Logging** - Log request/response
- ✅ **CORS Configuration** - Allow cross-origin requests
- ✅ **Health Checks** - `/health` và `/api/health`
- ✅ **Swagger** - API documentation

## 🔨 Next Steps

1. **Implement OrderService** - Tương tự UserService
2. **Implement InventoryService** - Products, Stock management
3. **Implement PaymentService** - Payment processing
4. **Add Authentication** - JWT tokens
5. **Add Message Queue** - RabbitMQ/Azure Service Bus
6. **Add Docker** - Containerization
7. **Add Logging** - Serilog
8. **Add Unit Tests** - xUnit

## 📖 Tài liệu tham khảo

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microservices Pattern](https://microservices.io/)
- [Ocelot Documentation](https://ocelot.readthedocs.io/)
- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)

## 👨‍💻 Development

**Quy tắc khi implement service mới:**
1. Tạo Entities trong `Domain/Entities`
2. Tạo Repository Interface trong `Domain/Interfaces`
3. Tạo DTOs trong `Application/DTOs`
4. Tạo Service Interface trong `Application/Interfaces`
5. Implement Repository trong `Infrastructure/Repositories`
6. Implement Service trong `Application/Services`
7. Tạo DbContext trong `Infrastructure/Data`
8. Tạo Controller trong `Controllers`
9. Config DI trong `Program.cs`
10. Run migrations và update database

---

**Happy Coding! 🚀**
