# 🚀 PRN232 Microservices — API Documentation

> **VPS**: `14.225.207.221` (Vietnix Linux)
> **CI/CD**: Push lên `main` → GitHub Actions tự động deploy
> **Database**: Supabase PostgreSQL

---

## 📋 Tổng Quan Hệ Thống

```
Client → API Gateway (port 5092) → Route đến Service phù hợp
                                    ├── UserService      (port 5209)
                                    ├── OrderService     (port 5267)
                                    ├── InventoryService (port 5254)
                                    └── PaymentService   (port 5253)
```

---

## 📖 Swagger UI — Test API trực tiếp

| Service | Swagger URL |
|---------|-------------|
| 💳 PaymentService | http://14.225.207.221:5253/swagger |
| 📦 InventoryService | http://14.225.207.221:5254/swagger |
| 👤 UserService | http://14.225.207.221:5209/swagger |
| 🛒 OrderService | http://14.225.207.221:5267/swagger |

---

## 🚪 API Gateway — Cổng Vào Chung (port 5092)

Tất cả request đều đi qua API Gateway, format:

```
http://14.225.207.221:5092/{tên-service}/api/{endpoint}
```

---

### 💳 PaymentService — Giao Dịch Ví (Nguyễn Gia Khánh)

**Schema**: `khanh_wallet` trên Supabase

| Chức năng | Method | Endpoint |
|-----------|--------|----------|
| Tạo Account + Wallet | `POST` | `/payment-service/api/khanh-wallet/accounts` |
| Xem thông tin Wallet | `GET` | `/payment-service/api/khanh-wallet/accounts/{accountId}` |
| Nạp tiền | `POST` | `/payment-service/api/khanh-wallet/deposit` |
| Rút tiền | `POST` | `/payment-service/api/khanh-wallet/withdraw` |
| Lịch sử giao dịch | `GET` | `/payment-service/api/khanh-wallet/transactions/{walletId}` |
| **Tra cứu bất thường** | `GET` | `/payment-service/api/khanh-wallet/suspicious` |

#### Body mẫu — Tạo Account:
```json
{
  "username": "khanh_test",
  "email": "khanh@example.com"
}
```

#### Body mẫu — Nạp/Rút tiền:
```json
{
  "accountId": "guid-của-account",
  "amount": 500000,
  "description": "Nạp tiền lần đầu"
}
```

#### Thuật toán phát hiện bất thường:

| Rule | Tiêu chí | Ngưỡng |
|------|----------|--------|
| 1 | Giao dịch lớn bất thường | Số tiền > **5x** trung bình 30 ngày |
| 2 | Tần suất cao | > **10** giao dịch trong 1 giờ |
| 3 | Rút gần hết ví | Rút > **90%** số dư hiện tại |

---

### 👤 UserService

| Chức năng | Method | Endpoint |
|-----------|--------|----------|
| Danh sách users | `GET` | `/user-service/api/users` |
| Xem user | `GET` | `/user-service/api/users/{id}` |
| Tạo user | `POST` | `/user-service/api/users` |
| Cập nhật user | `PUT` | `/user-service/api/users/{id}` |
| Xóa user | `DELETE` | `/user-service/api/users/{id}` |

---

### 📦 InventoryService (Danh — Event Sourcing)

**Schema**: `danh_inventory` trên Supabase

| Chức năng | Method | Endpoint |
|-----------|--------|----------|
| Xem tồn kho | `GET` | `/inventory-service/api/danh-inventory/stock/{productId}` |
| Nhập kho | `POST` | `/inventory-service/api/danh-inventory/import` |
| Simulate order | `POST` | `/inventory-service/api/danh-inventory/simulate-order` |
| Xem event history | `GET` | `/inventory-service/api/danh-inventory/events/{aggregateId}` |

---

### 🛒 OrderService

> ⚠️ Đang chờ dev khác implement. Dockerfile và Docker Compose đã sẵn sàng.

---

## 🐳 Docker Containers

| Container | Image | Port |
|-----------|-------|------|
| prn232-api-gateway | ApiGateway | 5092 |
| prn232-user-service | UserService | 5209 |
| prn232-order-service | OrderService | 5267 |
| prn232-inventory-service | InventoryService | 5254 |
| prn232-payment-service | PaymentService | 5253 |

### Lệnh quản lý trên VPS:

```bash
# SSH vào VPS
ssh root@14.225.207.221

# Xem containers đang chạy
cd /opt/prn232-seminar
docker-compose ps

# Xem logs
docker-compose logs --tail 20 payment-service

# Restart tất cả
docker-compose down
docker-compose up --build -d
```

---

## 🔄 CI/CD Pipeline

```
Push code lên main → GitHub Actions → SSH vào VPS → git pull → docker-compose up --build -d
```

**GitHub Actions**: https://github.com/E99PLANT/PRN232_Seminar/actions

---

## 👥 Phân công

| Dev | Service | Schema |
|-----|---------|--------|
| Nguyễn Gia Khánh | PaymentService (Giao Dịch Ví) | `khanh_wallet` |
| Danh | InventoryService (Event Sourcing) | `danh_inventory` |
| - | UserService | `UserServiceDb` (local) |
| - | OrderService | Chưa implement |
