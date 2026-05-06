# MultiClientPlatform API — Endpoint Reference

> Base URL: `https://localhost:{PORT}`
> Auth: All protected endpoints require `Authorization: Bearer {token}` header.
> Roles: `Customer` | `Merchant`

---

## Auth

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | Public | Register a new user (Customer or Merchant) |
| POST | `/api/auth/login` | Public | Login and receive JWT token |

### Use Cases
- **User signs up as Customer** → `POST /api/auth/register` with `"role": "Customer"`
- **User signs up as Merchant** → `POST /api/auth/register` with `"role": "Merchant"`
- **Any user logs in** → `POST /api/auth/login` → copy token → click Authorize in Swagger

### Request: Register
```json
{
  "fullName": "Jane Doe",
  "email": "jane@example.com",
  "password": "Password123!",
  "role": "Customer"
}
```

### Request: Login
```json
{
  "email": "jane@example.com",
  "password": "Password123!"
}
```

### Response (both)
```json
{
  "token": "eyJ...",
  "fullName": "Jane Doe",
  "email": "jane@example.com",
  "role": "Customer"
}
```

---

## Merchant

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| POST | `/api/merchant/profile` | Merchant | Create merchant profile |
| GET | `/api/merchant/profile` | Merchant | View own merchant profile |
| GET | `/api/merchant` | Public | Browse all merchants |
| GET | `/api/merchant/{id}` | Public | View a single merchant by ID |
| GET | `/api/merchant/{id}/products` | Public | View all products listed by a merchant |

### Use Cases
- **Merchant sets up their store** → `POST /api/merchant/profile`
- **Merchant checks their profile** → `GET /api/merchant/profile`
- **Customer/visitor browses all stores** → `GET /api/merchant`
- **Customer/visitor views a specific store** → `GET /api/merchant/{id}`
- **Customer/visitor browses products of a store** → `GET /api/merchant/{id}/products`

### Request: Create Profile
```json
{
  "businessName": "Smith's Electronics",
  "description": "Premium electronics at affordable prices."
}
```

---

## Products

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| POST | `/api/product` | Merchant | Create a new product |
| PUT | `/api/product/{id}` | Merchant | Update own product (ownership enforced) |
| GET | `/api/product` | Public | Browse all products across all merchants |
| GET | `/api/product/{id}` | Public | View a single product by ID |

### Use Cases
- **Merchant lists a new product** → `POST /api/product`
- **Merchant edits a product** → `PUT /api/product/{id}`
- **Customer browses all products** → `GET /api/product`
- **Customer views product detail** → `GET /api/product/{id}`
- **Frontend product listing page** → `GET /api/product` or `GET /api/merchant/{id}/products`

### Request: Create / Update Product
```json
{
  "name": "Wireless Headphones",
  "description": "Noise cancelling, 30hr battery.",
  "price": 89.99
}
```

---

## Cart

> All cart endpoints require `Customer` role.

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| GET | `/api/cart` | Customer | View full cart with line totals and grand total |
| POST | `/api/cart` | Customer | Add a product to cart (merges if already exists) |
| PUT | `/api/cart/{cartItemId}` | Customer | Update quantity of a cart item |
| DELETE | `/api/cart/{cartItemId}` | Customer | Remove a product from cart |

### Use Cases
- **Customer opens cart page** → `GET /api/cart`
- **Customer adds product to cart** → `POST /api/cart`
- **Customer changes quantity** → `PUT /api/cart/{cartItemId}`
- **Customer removes item** → `DELETE /api/cart/{cartItemId}`
- **Cart supports multiple merchants** → add products from different merchants freely

### Request: Add to Cart
```json
{
  "productId": 1,
  "quantity": 2
}
```

### Request: Update Quantity
```json
{
  "quantity": 5
}
```

### Response: Get Cart
```json
{
  "items": [
    {
      "id": 1,
      "productId": 3,
      "productName": "Wireless Headphones",
      "unitPrice": 89.99,
      "quantity": 2,
      "lineTotal": 179.98
    }
  ],
  "grandTotal": 179.98
}
```

---

## Orders

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| POST | `/api/order/checkout` | Customer | Convert cart into an order, clears cart |
| GET | `/api/order` | Customer | View all own orders |
| GET | `/api/order/{id}` | Customer | View a single order (ownership enforced) |
| GET | `/api/order/merchant-items` | Merchant | View all order items belonging to this merchant |

### Use Cases
- **Customer checks out** → `POST /api/order/checkout` → cart is cleared, order is created
- **Customer views order history** → `GET /api/order`
- **Customer views order detail** → `GET /api/order/{id}`
- **Merchant sees what was ordered from them** → `GET /api/order/merchant-items`

### Response: Checkout / Order Detail
```json
{
  "id": 1,
  "userId": 2,
  "totalAmount": 179.98,
  "status": "Pending",
  "placedAt": "2025-01-01T10:00:00Z",
  "items": [
    {
      "id": 1,
      "productId": 3,
      "productName": "Wireless Headphones",
      "merchantId": 1,
      "unitPrice": 89.99,
      "quantity": 2,
      "lineTotal": 179.98
    }
  ]
}
```

---

## Payments

> All payment endpoints require `Customer` role.

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| POST | `/api/payment/initiate/{orderId}` | Customer | Initiate payment, returns dummy gateway URL |
| POST | `/api/payment/complete/{paymentId}` | Customer | Simulate gateway callback, marks order as Paid |
| GET | `/api/payment/status/{orderId}` | Customer | Check payment and order status |

### Use Cases
- **Customer initiates payment after checkout** → `POST /api/payment/initiate/{orderId}` → receive `paymentUrl`
- **Simulate payment gateway returning** → `POST /api/payment/complete/{paymentId}`
- **Customer checks if payment went through** → `GET /api/payment/status/{orderId}`
- **Frontend polls payment status** → `GET /api/payment/status/{orderId}`

### Response: Initiate Payment
```json
{
  "paymentId": 1,
  "orderId": 1,
  "status": "Pending",
  "paymentUrl": "https://dummy-gateway.example.com/pay?orderId=1&amount=179.98&ref=abc123",
  "initiatedAt": "2025-01-01T10:05:00Z"
}
```

### Response: Status
```json
{
  "paymentId": 1,
  "orderId": 1,
  "paymentStatus": "Completed",
  "orderStatus": "Paid",
  "initiatedAt": "2025-01-01T10:05:00Z",
  "completedAt": "2025-01-01T10:06:00Z"
}
```

---

## Full End-to-End Flow

### Customer Journey
```
1. POST /api/auth/register          → register as Customer
2. POST /api/auth/login             → get JWT token
3. GET  /api/merchant               → browse merchants
4. GET  /api/merchant/{id}/products → browse products of a merchant
5. POST /api/cart                   → add products to cart
6. GET  /api/cart                   → review cart
7. POST /api/order/checkout         → place order (cart cleared)
8. POST /api/payment/initiate/{id}  → initiate payment
9. POST /api/payment/complete/{id}  → simulate payment success
10. GET /api/payment/status/{id}    → confirm Paid status
```

### Merchant Journey
```
1. POST /api/auth/register          → register as Merchant
2. POST /api/auth/login             → get JWT token
3. POST /api/merchant/profile       → create store profile
4. POST /api/product                → list products
5. PUT  /api/product/{id}           → update product
6. GET  /api/order/merchant-items   → view orders received
```

---

## Status Values

| Entity | Field | Possible Values |
|--------|-------|-----------------|
| Order | Status | `Pending`, `Paid` |
| Payment | Status | `Pending`, `Completed` |

---

## Role Enforcement Summary

| Feature | Customer | Merchant | Public |
|---------|----------|----------|--------|
| Auth | ✅ | ✅ | ✅ |
| Browse Merchants | — | — | ✅ |
| Browse Products | — | — | ✅ |
| Manage Products | ❌ | ✅ (own only) | — |
| Manage Merchant Profile | ❌ | ✅ (own only) | — |
| Cart | ✅ | ❌ | — |
| Orders (place/view) | ✅ | ❌ | — |
| Orders (merchant view) | ❌ | ✅ (own items) | — |
| Payments | ✅ | ❌ | — |
