# 🎬 Kịch Bản Demo — Giao Dịch Ví (Nguyễn Gia Khánh)

> **Chủ đề**: Event Sourcing + Phát hiện giao dịch bất thường + RabbitMQ
> **Services liên quan**: PaymentService ↔ UserService
> **VPS**: `14.225.207.221`

---

## Kiến Trúc Tổng Quan

```
UserService                              PaymentService
    │                                         │
    │  POST /api/users (tạo user)             │
    │ ──── UserCreatedEvent ────────────────► │
    │       (qua RabbitMQ)                    │ Tự động tạo Account + Wallet
    │                                         │ + ghi WalletEvent "WalletCreated"
    │                                         │
    │                                         │ Nạp/Rút tiền → ghi WalletEvent
    │                                         │ Phát hiện bất thường ↓
    │ ◄──── SuspiciousActivityDetected ────── │
    │        (qua RabbitMQ)                   │
    │  Log cảnh báo                           │
```

---

## Bước 1: Tạo User → Ví tự động tạo (RabbitMQ)

**Swagger**: `http://14.225.207.221:5092/swagger` → chọn **User Service**

```http
POST /api/users
```
```json
{
  "username": "khanh_demo",
  "email": "khanh@demo.com",
  "password": "123456",
  "fullName": "Nguyen Gia Khanh",
  "phoneNumber": "0909123456"
}
```

**Nói**: "Khi tạo user, UserService publish `UserCreatedEvent` qua RabbitMQ → PaymentService nhận event → tự động tạo Account + Wallet cho user. KHÔNG cần gọi API trực tiếp."

**Chứng minh**: Chuyển sang **Payment Service** → `GET /accounts/{accountId}` → thấy ví đã tạo với 0 VND.

---

## Bước 2: Nạp tiền vào ví

**Swagger**: chọn **Payment Service**

```http
POST /payment-service/api/khanh-wallet/deposit
```
```json
{ "accountId": "...", "amount": 1000000, "description": "Nạp tiền lần 1" }
```
Nạp thêm:
```json
{ "accountId": "...", "amount": 500000, "description": "Nạp tiền lần 2" }
```

**Nói**: "Mỗi giao dịch đều ghi lại event theo pattern Event Sourcing."

---

## Bước 3: Rút tiền bình thường

```http
POST /payment-service/api/khanh-wallet/withdraw
```
```json
{ "accountId": "...", "amount": 200000, "description": "Rút tiền mua đồ" }
```

Ví hiện tại: 1,500,000 - 200,000 = **1,300,000 VND**

---

## Bước 4: ⚠️ Tạo giao dịch BẤT THƯỜNG

```http
POST /payment-service/api/khanh-wallet/withdraw
```
```json
{ "accountId": "...", "amount": 1200000, "description": "Rút tiền gấp" }
```

**Nói**: "Rút 1.2 triệu / 1.3 triệu = **92% số dư** → vượt ngưỡng 90% → hệ thống TỰ ĐỘNG đánh dấu bất thường + gửi cảnh báo qua RabbitMQ tới UserService."

Response sẽ có:
```json
{
  "warning": "⚠️ CẢNH BÁO: Rút 92.3% số dư (> 90% ngưỡng cảnh báo)"
}
```

---

## Bước 5: Tra cứu giao dịch bất thường

```http
GET /payment-service/api/khanh-wallet/suspicious
```

**Nói**: "API này trả về TẤT CẢ giao dịch bị gắn cờ bất thường, kèm lý do cụ thể."

### 3 thuật toán phát hiện:

| Rule | Tiêu chí | Ngưỡng |
|------|----------|--------|
| 1 | Giao dịch lớn bất thường | Số tiền > **5x** trung bình 30 ngày |
| 2 | Tần suất giao dịch cao | > **10** giao dịch trong 1 giờ |
| 3 | Rút gần hết ví | Rút > **90%** số dư hiện tại |

---

## Bước 6: 📝 Event Sourcing — Replay lịch sử

```http
GET /payment-service/api/khanh-wallet/events/{walletId}
```

**Nói**: "Đây là Event Sourcing — mọi thay đổi trạng thái ví đều ghi lại dạng event immutable, có thể replay lại toàn bộ."

Kết quả:
```
#1  WalletCreated       → Balance: 0           (Source: RabbitMQ)
#2  Deposited           → +1,000,000           → Balance: 1,000,000
#3  Deposited           → +500,000             → Balance: 1,500,000
#4  Withdrawn           → -200,000             → Balance: 1,300,000
#5  Withdrawn           → -1,200,000           → Balance: 100,000
#6  SuspiciousDetected  → Rút 92% số dư
```

**Nói**: "So với cách truyền thống chỉ lưu số dư cuối cùng, Event Sourcing cho phép audit toàn bộ lịch sử — biết chính xác AI làm GÌ, KHI NÀO, BAO NHIÊU."

---

## Bonus: RabbitMQ Dashboard

Mở: `http://14.225.207.221:15672` (guest/guest)

→ Vào tab **Queues** → thấy các queue tự tạo bởi MassTransit:
- `user-created-consumer` (PaymentService lắng nghe)
- `suspicious-activity-consumer` (UserService lắng nghe)

---

## Keyword khi trình bày

| Khái niệm | Giải thích |
|------------|-----------|
| **Event Sourcing** | Lưu mọi sự kiện thay đổi, không chỉ trạng thái cuối |
| **Aggregate** | WalletId — mỗi ví là 1 aggregate root |
| **Event Store** | Bảng `wallet_events` trong schema `khanh_wallet` |
| **RabbitMQ** | Message broker — service giao tiếp bất đồng bộ |
| **MassTransit** | Framework .NET wrap RabbitMQ |
| **Consumer** | "Nhân viên" lắng nghe event từ queue |
| **Publish** | Gửi event lên RabbitMQ |
| **Suspicious Detection** | 3 thuật toán phát hiện bất thường |
