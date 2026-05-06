# Backend Flow — MultiClientPlatform API

## Case Study Overview

MultiClientPlatform is a multi-role marketplace API built with ASP.NET Core (.NET). It supports two types of users — **Customers** and **Merchants**. Merchants can set up a store and list products. Customers can browse those products, add them to a cart, place orders, and complete payments. The system enforces strict ownership rules so each user only ever sees and modifies their own data.

**Tech Stack:**
- ASP.NET Core Web API
- Entity Framework Core with PostgreSQL (Npgsql)
- JWT Bearer Authentication
- BCrypt password hashing
- Swagger UI for API exploration

---

## Architecture Pattern

The project uses a **Feature Folder** structure. Each feature (Auth, Merchants, Products, Cart, Orders, Payments) is self-contained with its own:

```
Features/
  FeatureName/
    Entities/       — EF Core model (maps to DB table)
    Dtos/           — Request and Response shapes (what the API accepts/returns)
    Interfaces/     — IRepository and IService contracts
    Controller      — HTTP layer, reads JWT claims, calls service
    Service         — Business logic, orchestrates repositories
    Repository      — Database queries via EF Core DbContext
```

This separation means the Controller never touches the DB directly, and the Repository never contains business logic. The Service sits in between and owns all decisions.

**Dependency Injection** wires everything together in `Program.cs`:
```
AddScoped<IAuthRepository, AuthRepository>()
AddScoped<IAuthService, AuthService>()
```
`Scoped` means one instance per HTTP request — safe for DbContext usage.

---

## Entities and Their Fields

### User
```
Id, FullName, Email, PasswordHash, Role ("Customer" | "Merchant"), CreatedAt
```
The central identity entity. Role is stored as a plain string and embedded in the JWT as a claim.

### Merchant
```
Id, UserId (FK → User), BusinessName, Description, CreatedAt
```
A Merchant is a separate profile entity linked to a User via `UserId`. A User with Role=Merchant can create exactly one Merchant profile. This separation means the User table stays clean — it only holds identity, while Merchant holds store-specific data.

### Product
```
Id, MerchantId (FK → Merchant), Name, Description, Price, CreatedAt
```
Products belong to a Merchant (not directly to a User). This is important — ownership checks go through `User → Merchant → Product`, not `User → Product`.

### CartItem
```
Id, UserId (FK → User), ProductId (FK → Product), Quantity, AddedAt
```
There is no `Cart` entity. The cart is just a collection of `CartItem` rows filtered by `UserId`. Each row represents one product line in the customer's cart.

### Order
```
Id, UserId (FK → User), TotalAmount, Status ("Pending" | "Paid"), PlacedAt
Navigation: List<OrderItem> Items
```
Created at checkout. Status starts as `Pending` and moves to `Paid` when payment completes.

### OrderItem
```
Id, OrderId (FK → Order), ProductId, MerchantId, ProductName, UnitPrice, Quantity, LineTotal
```
Key design decision: `ProductName` and `UnitPrice` are **snapshot values** copied at checkout time. If a merchant later changes a product's price, old orders are unaffected. `MerchantId` is stored here so merchants can query their own order items without joining through Order.

### Payment
```
Id, OrderId (FK → Order), Status ("Pending" | "Completed" | "Failed"), PaymentUrl, InitiatedAt, CompletedAt?
```
One-to-one with Order. `PaymentUrl` is a dummy gateway URL generated with a GUID. `CompletedAt` is nullable — only set when payment is completed.

---

## Entity Relationship Map
 
```
User (1) ──────────────── (0..1) Merchant (1) ──── (many) Product
 │                                                          │
 │                                                          │
(many) CartItem ──────────────────────────────────── (FK) ProductId
 │
(many) Order (1) ──── (many) OrderItem ──── (FK) MerchantId → Merchant
          │
        (1) Payment
```

- `User → Merchant`: one-to-one (enforced in service layer, not DB constraint)
- `Merchant → Product`: one-to-many
- `User → CartItem`: one-to-many (the "cart" is just these rows)
- `User → Order`: one-to-many
- `Order → OrderItem`: one-to-many (EF navigation property with `.Include()`)
- `Order → Payment`: one-to-one
- `OrderItem.MerchantId`: denormalized FK to Merchant for efficient merchant order queries

---

## Program.cs — Middleware Pipeline

The order of middleware in `Program.cs` matters:

```
UseHttpsRedirection
UseCors("AllowAngular")       ← must be before auth
UseAuthentication             ← reads and validates JWT
UseAuthorization              ← enforces [Authorize] attributes
MapControllers
```

CORS is configured to allow `http://localhost:4200` (Angular dev server) with any header and method.

JWT is configured with:
- `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey` all true
- Key, Issuer, Audience read from `appsettings.json`
- Debug events (`OnAuthenticationFailed`, `OnTokenValidated`, `OnMessageReceived`) log to console

---

## Feature 1 — Authentication

### Concept: JWT (JSON Web Token)
A JWT is a self-contained token with three base64-encoded parts: `header.payload.signature`. The payload contains **claims** — pieces of data embedded in the token itself. The server never stores the token; it just validates the signature on every request.

Claims embedded in this app's tokens:
- `ClaimTypes.NameIdentifier` = `userId` (integer)
- `ClaimTypes.Name` = `email`
- `ClaimTypes.Role` = `"Customer"` or `"Merchant"`

The `[Authorize(Roles = "Merchant")]` attribute checks the Role claim automatically. Controllers extract `userId` with:
```csharp
int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
```

### Concept: BCrypt Password Hashing
Passwords are never stored in plain text. BCrypt hashes include a random salt, so two identical passwords produce different hashes. Verification uses `BCrypt.Verify(plainText, storedHash)`.

### Endpoints

**POST /api/auth/register**
- Body: `{ fullName, email, password, role }`
- Validates email uniqueness and role value ("Customer" or "Merchant" only)
- Hashes password with BCrypt
- Saves User to DB
- Generates JWT immediately — user is logged in right after registering
- Returns: `{ token, fullName, email, role }`

**POST /api/auth/login**
- Body: `{ email, password }`
- Looks up User by email
- Verifies password against stored BCrypt hash
- Generates JWT
- Returns: `{ token, fullName, email, role }`

### Flow: Register
```
POST /api/auth/register
  → AuthController.Register()
  → AuthService.RegisterAsync()
      → AuthRepository.EmailExistsAsync()   [SELECT EXISTS]
      → BCrypt.HashPassword()
      → AuthRepository.AddUserAsync()       [INSERT INTO Users]
      → JwtHelper.GenerateToken()
  ← 200 OK { token, fullName, email, role }
```

### Flow: Login
```
POST /api/auth/login
  → AuthController.Login()
  → AuthService.LoginAsync()
      → AuthRepository.GetByEmailAsync()    [SELECT * FROM Users WHERE Email = ?]
      → BCrypt.Verify(password, hash)
      → JwtHelper.GenerateToken()
  ← 200 OK { token, fullName, email, role }
  or 401 Unauthorized
```

### JwtHelper.GenerateToken()
Reads `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes` from `appsettings.json`. Creates a `SymmetricSecurityKey` from the key string, builds claims list, constructs `JwtSecurityToken`, and serializes it to the `xxxxx.yyyyy.zzzzz` string format.

---

## Feature 2 — Merchant Profile

### Concept: Role-Based Authorization
`[Authorize(Roles = "Merchant")]` on an endpoint means the JWT must be present AND the Role claim must equal "Merchant". If the token is missing → 401. If the token is valid but role is wrong → 403.

`[AllowAnonymous]` overrides any controller-level `[Authorize]` and makes the endpoint public.

### Endpoints

**GET /api/merchant** — `[AllowAnonymous]`
- Returns all merchant profiles
- Used by the catalog page to list stores

**GET /api/merchant/{id}** — `[AllowAnonymous]`
- Returns a single merchant by ID

**GET /api/merchant/{id}/products** — `[AllowAnonymous]`
- Returns all products for a given merchant
- Validates merchant exists first, then fetches products

**POST /api/merchant/profile** — `[Authorize(Roles = "Merchant")]`
- Body: `{ businessName, description }`
- Extracts `userId` from JWT
- Checks if a profile already exists for this user (one profile per user enforced)
- Creates Merchant row with `UserId = userId`
- Returns 201 Created with the new profile

**GET /api/merchant/my-profile** — `[Authorize(Roles = "Merchant")]`
- Extracts `userId` from JWT
- Queries `Merchants WHERE UserId = userId`
- Returns the merchant's own profile
- Returns 404 if no profile created yet

### How a Merchant Sees Only Their Own Profile
The `my-profile` endpoint does `GetByUserIdAsync(userId)` — it filters by the `UserId` extracted from the JWT. There is no way to pass a different userId; it always comes from the validated token. So Merchant A cannot retrieve Merchant B's profile through this endpoint.

### Flow: Create Profile
```
POST /api/merchant/profile  [Bearer token required, Role=Merchant]
  → MerchantController.CreateProfile()
      → GetUserId() from JWT claim
  → MerchantService.CreateProfileAsync(userId, request)
      → MerchantRepository.ExistsByUserIdAsync(userId)   [check duplicate]
      → MerchantRepository.AddAsync(merchant)            [INSERT]
  ← 201 Created { id, userId, businessName, description, createdAt }
```

---

## Feature 3 — Products

### Concept: Ownership Chain
Products are owned by a Merchant, not directly by a User. So when a merchant creates or updates a product, the service must:
1. Look up the Merchant by `userId` (from JWT)
2. Verify the product's `MerchantId` matches that merchant's `Id`

This two-step ownership check prevents Merchant A from editing Merchant B's products even if they know the product ID.

### Endpoints

**GET /api/product** — `[AllowAnonymous]`
- Returns all products across all merchants

**GET /api/product/{id}** — `[AllowAnonymous]`
- Returns a single product

**POST /api/product** — `[Authorize(Roles = "Merchant")]`
- Body: `{ name, description, price }`
- Looks up merchant by `userId` from JWT
- Returns 400 if no merchant profile exists yet
- Creates product with `MerchantId = merchant.Id`

**PUT /api/product/{id}** — `[Authorize(Roles = "Merchant")]`
- Body: `{ name, description, price }`
- Fetches product by ID
- Fetches merchant by `userId` from JWT
- Checks `product.MerchantId == merchant.Id` — if not, returns 403 Forbid
- Updates and saves

### Flow: Update Product (with ownership check)
```
PUT /api/product/5  [Bearer token, Role=Merchant]
  → ProductController.Update(id=5, request)
      → GetUserId() = 12 (from JWT)
  → ProductService.UpdateAsync(userId=12, productId=5, request)
      → ProductRepository.GetByIdAsync(5)          [fetch product]
      → MerchantRepository.GetByUserIdAsync(12)    [fetch this user's merchant]
      → product.MerchantId == merchant.Id ?
          YES → update and save
          NO  → return (true, false, null) → 403 Forbid
```

---

## Feature 4 — Cart

### Concept: No Cart Entity
There is no `Cart` table. The cart is implicitly defined as all `CartItem` rows where `UserId = currentUser`. This is simpler and avoids a redundant parent entity.

### Concept: Idempotent Add
If a customer adds a product that's already in their cart, the service increments the existing item's quantity instead of creating a duplicate row. This is handled by `GetItemByProductAsync(userId, productId)`.

### Concept: Controller-Level Authorization
`[Authorize(Roles = "Customer")]` is on the controller class, not individual methods. This means every endpoint in `CartController` requires a valid Customer JWT — no `[AllowAnonymous]` overrides exist here.

### Endpoints

**GET /api/cart** — `[Authorize(Roles = "Customer")]`
- Fetches all CartItems for this user
- For each item, fetches the Product to get current name and price
- Computes `LineTotal = UnitPrice * Quantity` per item
- Computes `GrandTotal = sum of all LineTotals`
- Returns `{ items: [...], grandTotal }`

**POST /api/cart** — `[Authorize(Roles = "Customer")]`
- Body: `{ productId, quantity }`
- Validates product exists
- If product already in cart → increments quantity
- Otherwise → creates new CartItem

**PUT /api/cart/{cartItemId}** — `[Authorize(Roles = "Customer")]`
- Body: `{ quantity }`
- Fetches CartItem by ID
- Checks `cartItem.UserId == userId` — if not, 403
- Updates quantity

**DELETE /api/cart/{cartItemId}** — `[Authorize(Roles = "Customer")]`
- Fetches CartItem by ID
- Checks ownership
- Deletes the row
- Returns 204 No Content

### Flow: Add to Cart
```
POST /api/cart  [Bearer token, Role=Customer]
  → CartController.AddItem(request)
      → GetUserId() = 7
  → CartService.AddItemAsync(userId=7, request)
      → ProductRepository.GetByIdAsync(productId)       [validate product exists]
      → CartRepository.GetItemByProductAsync(7, productId)
          EXISTS → existing.Quantity += request.Quantity → UpdateAsync
          NOT EXISTS → new CartItem { UserId=7, ProductId, Quantity } → AddAsync
  ← 200 OK CartItemResponse { id, productId, productName, unitPrice, quantity, lineTotal }
```

---

## Feature 5 — Orders (Checkout)

### Concept: Checkout as a Transaction
Checkout does several things atomically (within one SaveChanges call per repository operation):
1. Reads all CartItems for the user
2. For each item, fetches the Product and snapshots its name and price
3. Creates an Order with all OrderItems
4. Clears the cart

The price snapshot is critical — `OrderItem.UnitPrice` is set to `product.Price` at the moment of checkout. Future price changes by the merchant do not affect this order.

### Concept: MerchantId Denormalization
`OrderItem.MerchantId` is stored explicitly even though you could derive it via `Product.MerchantId`. This denormalization makes the merchant order query efficient — `WHERE MerchantId = ?` on OrderItems directly, no joins needed.

### Endpoints

**POST /api/order/checkout** — `[Authorize(Roles = "Customer")]`
- Reads cart for this user
- Returns 400 if cart is empty
- Creates Order + OrderItems (with price snapshots)
- Clears cart
- Returns the full order with all items

**GET /api/order** — `[Authorize(Roles = "Customer")]`
- Returns all orders for this customer
- Uses `.Include(o => o.Items)` to load order items in one query (EF Core eager loading)

**GET /api/order/{id}** — `[Authorize(Roles = "Customer")]`
- Returns a single order
- Checks `order.UserId == userId` — if not, 403

**GET /api/order/merchant-items** — `[Authorize(Roles = "Merchant")]`
- Looks up merchant by `userId` from JWT
- Queries `OrderItems WHERE MerchantId = merchant.Id`
- Returns only the line items that belong to this merchant's products
- A merchant never sees the full order or other merchants' items

### How a Merchant Sees Only Their Own Order Items
```
GET /api/order/merchant-items  [Bearer token, Role=Merchant]
  → OrderController.GetMerchantOrderItems()
      → GetUserId() = 15
      → MerchantRepository.GetByUserIdAsync(15)   → merchant.Id = 3
  → OrderService.GetMyOrderItemsAsync(merchantId=3)
      → OrderRepository.GetItemsByMerchantIdAsync(3)
          SELECT * FROM OrderItems WHERE MerchantId = 3
  ← Only items where the product was sold by Merchant #3
```
Merchant A (id=3) cannot see Merchant B's (id=4) items because the query is always filtered by the `merchantId` derived from the JWT — not from any user-supplied parameter.

### Flow: Checkout
```
POST /api/order/checkout  [Bearer token, Role=Customer]
  → OrderController.Checkout()
      → GetUserId() = 7
  → OrderService.CheckoutAsync(userId=7)
      → CartRepository.GetByUserIdAsync(7)         [get all cart items]
      → for each cartItem:
          ProductRepository.GetByIdAsync(productId) [get current price]
          build OrderItem { MerchantId, ProductName, UnitPrice (snapshot), Quantity, LineTotal }
      → OrderRepository.AddAsync(order)             [INSERT Order + OrderItems]
      → CartRepository.ClearCartAsync(7)            [DELETE CartItems WHERE UserId=7]
  ← 200 OK OrderResponse { id, userId, totalAmount, status="Pending", placedAt, items:[...] }
```

---

## Feature 6 — Payments

### Concept: Simulated Payment Gateway
Real payment gateways (Stripe, Razorpay) redirect users to an external URL, process payment, then call a webhook back. This app simulates that with:
1. **Initiate** — generates a dummy URL with a GUID reference, saves a `Payment` row with `Status=Pending`
2. **Complete** — simulates the gateway callback, marks Payment as `Completed` and Order as `Paid`

### Concept: Idempotent Initiate
If `POST /api/payment/initiate/{orderId}` is called again for an already-initiated (but not completed) payment, it returns the existing Payment record instead of creating a duplicate. If already completed, it returns 409 Conflict.

### Endpoints

**POST /api/payment/initiate/{orderId}** — `[Authorize(Roles = "Customer")]`
- Fetches Order, checks ownership
- Checks if payment already exists:
  - Completed → 409 Conflict
  - Pending → returns existing payment (idempotent)
  - None → creates new Payment with dummy URL
- Returns: `{ paymentId, orderId, status, paymentUrl, initiatedAt }`

**POST /api/payment/complete/{paymentId}** — `[Authorize(Roles = "Customer")]`
- Fetches Payment by ID
- Fetches Order, checks `order.UserId == userId`
- Sets `payment.Status = "Completed"`, `payment.CompletedAt = now`
- Sets `order.Status = "Paid"`
- Returns: `{ paymentId, orderId, paymentStatus, orderStatus, initiatedAt, completedAt }`

**GET /api/payment/status/{orderId}** — `[Authorize(Roles = "Customer")]`
- Fetches Order (ownership check), then Payment by orderId
- Returns current payment and order status

### Flow: Complete Payment
```
POST /api/payment/complete/4  [Bearer token, Role=Customer]
  → PaymentController.Complete(paymentId=4)
      → GetUserId() = 7
  → PaymentService.CompleteAsync(userId=7, paymentId=4)
      → PaymentRepository.GetByIdAsync(4)           [fetch payment]
      → OrderRepository.GetByIdAsync(payment.OrderId) [fetch order]
      → order.UserId == 7 ? YES → proceed
      → payment.Status = "Completed", payment.CompletedAt = now
      → PaymentRepository.UpdateAsync(payment)       [UPDATE Payments]
      → OrderRepository.UpdateStatusAsync(order, "Paid") [UPDATE Orders SET Status='Paid']
  ← 200 OK { paymentId, orderId, paymentStatus="Completed", orderStatus="Paid", completedAt }
```

---

## Full User Journey — Customer

```
1. POST /api/auth/register  → get JWT (Role=Customer)
2. GET  /api/merchant       → browse all merchants
3. GET  /api/merchant/3/products → see products for merchant #3
4. POST /api/cart           → add product to cart (JWT required)
5. GET  /api/cart           → view cart with totals
6. PUT  /api/cart/2         → update quantity
7. DELETE /api/cart/2       → remove item
8. POST /api/order/checkout → convert cart to order, cart cleared
9. POST /api/payment/initiate/1  → get payment URL
10. POST /api/payment/complete/1 → simulate payment, order becomes Paid
11. GET  /api/order         → view all orders with Paid status
```

## Full User Journey — Merchant

```
1. POST /api/auth/register  → get JWT (Role=Merchant)
2. GET  /api/merchant/my-profile → 404 (no profile yet)
3. POST /api/merchant/profile    → create store profile
4. POST /api/product             → add first product
5. PUT  /api/product/5           → edit product (ownership enforced)
6. GET  /api/order/merchant-items → see all order items for their products
```

---

## Security Summary

| Concern | Mechanism |
|---|---|
| Password storage | BCrypt hash with salt |
| Identity proof | JWT signed with HMAC-SHA256 |
| Role enforcement | `[Authorize(Roles = "...")]` checks Role claim |
| Ownership enforcement | Service layer compares JWT userId to entity's userId/merchantId |
| Cross-user data access | All queries filtered by userId/merchantId from JWT, never from request body |
| Token expiry | Configurable via `Jwt:ExpiryMinutes` in appsettings.json |
| CORS | Restricted to `http://localhost:4200` only |
