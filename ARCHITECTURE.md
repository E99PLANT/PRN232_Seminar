# 🏗️ Kiến Trúc Hệ Thống Ví Điện Tử — Event Sourcing

## Tổng quan

Hệ thống được xây dựng theo kiến trúc **Microservice**, gồm 2 service độc lập giao tiếp qua **RabbitMQ**.

```
┌──────────────┐       RabbitMQ        ┌──────────────────┐
│  UserService │ ◄═══════════════════► │  PaymentService  │
│  (Quản lý    │    Message Broker     │  (Quản lý Ví,    │
│   User)      │                       │   Giao dịch,     │
│              │                       │   Event Sourcing) │
└──────┬───────┘                       └──────┬───────────┘
       │                                      │
       │            API Gateway               │
       └──────────► (Port 5092) ◄─────────────┘
                        ▲
                        │ HTTP
                   ┌────┴────┐
                   │   FE    │
                   │ Next.js │
                   └─────────┘
```

---

## Luồng 1: Tạo User & Ví (RabbitMQ)

> FE → UserService → **RabbitMQ** → PaymentService

```
Bước 1: FE gửi POST /user-service/api/users
        { username, email, password, fullName, phoneNumber }

Bước 2: UserService lưu user vào DB
        → Publish "UserCreatedEvent" qua RabbitMQ

Bước 3: RabbitMQ truyền message đến PaymentService

Bước 4: PaymentService nhận event (UserCreatedConsumer)
        → Tạo Account
        → Tạo Wallet (số dư = 0)
        → Ghi WalletEvent "WalletCreated" (Event Sourcing)
```

### Tại sao dùng RabbitMQ ở đây?
- 2 service **hoàn toàn độc lập** — không gọi API trực tiếp
- Nếu PaymentService chết → message nằm chờ trong queue → sống lại là xử lý
- **Loose coupling** — thêm/bỏ service không ảnh hưởng nhau

---

## Luồng 2: Nạp/Rút Tiền (API trực tiếp)

> FE → API Gateway → PaymentService → DB

```
Bước 1: FE gửi POST /payment-service/api/khanh-wallet/deposit (hoặc /withdraw)
        { accountId, amount, description }

Bước 2: PaymentService xử lý:
        → Kiểm tra ví tồn tại
        → Cập nhật số dư
        → Ghi Transaction (lịch sử giao dịch)
        → Ghi WalletEvent "Deposited"/"Withdrawn" (Event Sourcing)
        → Kiểm tra giao dịch bất thường (3 thuật toán)

Bước 3: Nếu bất thường:
        → Ghi WalletEvent "SuspiciousDetected"
        → Publish "SuspiciousActivityDetected" qua RabbitMQ → UserService
```

### Tại sao KHÔNG dùng RabbitMQ ở đây?
- Chỉ **1 service** xử lý (PaymentService) → gọi trực tiếp cho nhanh
- Chỉ khi phát hiện bất thường mới cần báo cho UserService qua RabbitMQ

---

## Luồng 3: Phát Hiện Giao Dịch Bất Thường

> PaymentService → **RabbitMQ** → UserService

### 3 thuật toán phát hiện:

| Rule | Điều kiện | Ví dụ |
|------|-----------|-------|
| **Rule 1** | Số tiền > **5x** trung bình 30 ngày | TB 10,000 → giao dịch 60,000 |
| **Rule 2** | > **10** giao dịch trong 1 giờ | Spam 11 lần trong 60 phút |
| **Rule 3** | Rút > **90%** số dư ví | Ví 100,000 → rút 95,000 |

---

## Event Sourcing — Ghi lại mọi sự kiện

Mỗi thao tác tạo 1 **WalletEvent** không thể sửa/xóa:

| EventType | Khi nào | Dữ liệu |
|-----------|---------|----------|
| `WalletCreated` | Tạo ví mới | AccountId, Username, Email |
| `Deposited` | Nạp tiền | Amount, BalanceBefore, BalanceAfter |
| `Withdrawn` | Rút tiền | Amount, BalanceBefore, BalanceAfter |
| `SuspiciousDetected` | Phát hiện bất thường | Rule, Reason |

### Hash Chain (Bảo vệ tính toàn vẹn)

```
Event #1: WalletCreated
  PreviousHash = "GENESIS"
  Hash = SHA256("GENESIS" + data)  →  A3F2B8...

Event #2: Deposited
  PreviousHash = "A3F2B8..."  (hash của Event #1)
  Hash = SHA256("A3F2B8..." + data)  →  7C1DE4...

Event #3: Withdrawn
  PreviousHash = "7C1DE4..."  (hash của Event #2)
  Hash = SHA256("7C1DE4..." + data)  →  F9A0C1...
```

> Nếu ai đó sửa Event #1 → hash tính lại ≠ hash lưu trữ → **PHÁT HIỆN giả mạo!**

### DB Trigger (Chặn sửa/xóa)

```sql
CREATE TRIGGER trg_prevent_wallet_event_modification
BEFORE UPDATE OR DELETE ON khanh_wallet."WalletEvents"
FOR EACH ROW
EXECUTE FUNCTION khanh_wallet.prevent_event_modification();
-- ERROR: Wallet events are IMMUTABLE — cannot UPDATE or DELETE
```

---

## Tóm tắt: Khi nào dùng RabbitMQ?

| Thao tác | RabbitMQ? | Lý do |
|----------|-----------|-------|
| Tạo User → Tạo Ví | ✅ | 2 service phối hợp |
| Nạp tiền | ❌ | Chỉ PaymentService |
| Rút tiền | ❌ | Chỉ PaymentService |
| Phát hiện bất thường → Báo UserService | ✅ | 2 service phối hợp |
| Tìm ví | ❌ | Chỉ PaymentService |
| Xem events | ❌ | Chỉ PaymentService |
| Verify integrity | ❌ | Chỉ PaymentService |

---

## Công nghệ sử dụng

| Thành phần | Công nghệ |
|------------|-----------|
| Backend | .NET 8, Entity Framework Core |
| Database | PostgreSQL (Supabase) |
| Message Broker | RabbitMQ + MassTransit |
| API Gateway | Ocelot |
| Frontend | Next.js, Zustand, Axios |
| Hash | SHA256 (System.Security.Cryptography) |
