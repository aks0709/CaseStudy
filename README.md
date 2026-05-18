# 🛒 MultiClientPlatform — Full-Stack Marketplace

> A production-ready multi-role marketplace built with **ASP.NET Core (.NET 10)** and **Angular 21**, backed by **PostgreSQL** and fully containerized with **Docker**.

---

## 📌 Table of Contents

- [Overview](#-overview)
- [Tech Stack](#-tech-stack)
- [Key Features](#-key-features)
- [Architecture](#-architecture)
- [Database Schema](#-database-schema)
- [API Endpoints](#-api-endpoints)
- [Features & Functionality](#-features--functionality)
- [Docker Setup](#-docker-setup)
- [Local Development](#-local-development)
- [Environment Variables](#-environment-variables)
- [Security](#-security)
- [Project Structure](#-project-structure)

---

## 🌐 Overview

**MultiClientPlatform** is a full-stack marketplace application supporting two distinct user roles:

- 🧑‍💼 **Merchants** — Create a store, list products, and view incoming orders
- 🛍️ **Customers** — Browse merchants, manage a cart, place orders, and complete payments

The system enforces strict ownership rules — every user can only access and modify their own data, enforced at both the API and database query level.

---

## 🧰 Tech Stack

| Layer | Technology |
|---|---|
| 🖥️ Backend | ASP.NET Core Web API (.NET 10) |
| 🎨 Frontend | Angular 21 (NgModule, Lazy Loading) |
| 🗄️ Database | PostgreSQL 16 |
| 🔐 Auth | JWT Bearer Authentication + BCrypt |
| 🗃️ ORM | Entity Framework Core 10 + Npgsql |
| 📖 API Docs | Swagger / OpenAPI (Swashbuckle) |
| 🐳 Container | Docker + Docker Compose |
| 🌐 Web Server | Nginx (frontend) |

---

## ✨ Key Features

- 🔐 **JWT Authentication** — Secure login/register with role-based access (`Customer` / `Merchant`)
- 🔑 **BCrypt Password Hashing** — Passwords never stored in plain text
- 🏪 **Merchant Store Management** — Create store profiles, list and edit products
- 🛒 **Smart Cart** — Idempotent add-to-cart (merges duplicates), quantity updates, item removal
- 📦 **Order Checkout** — Converts cart to order with price snapshots at checkout time
- 💳 **Payment Simulation** — Simulated payment gateway with initiate → complete flow
- 🔒 **Ownership Enforcement** — All queries filtered by JWT claims, never by user-supplied IDs
- 📊 **Merchant Order Dashboard** — Merchants see only their own order line items
- 🚀 **Auto Migrations** — EF Core migrations run automatically on startup
- 📱 **SPA Frontend** — Angular 21 with lazy-loaded modules, reactive forms, and JWT interceptor

---

## 🏗️ Architecture

### Backend — Feature Folder Pattern

Each feature is fully self-contained:

```
Features/
  FeatureName/
    Entities/       ← EF Core model (DB table)
    Dtos/           ← Request & Response shapes
    Interfaces/     ← IRepository & IService contracts
    Controller      ← HTTP layer, reads JWT claims
    Service         ← Business logic
    Repository      ← EF Core DB queries
```

> Controller → Service → Repository → Database. The controller never touches the DB directly.

### Frontend — Angular 21 Module Structure

```
src/app/
  auth/         ← Login + Register
  catalog/      ← Browse merchants & products
  cart/         ← Cart management
  order/        ← Order history
  payment/      ← Payment flow
  merchant/     ← Merchant dashboard + orders received
  shared/       ← Guards, Interceptors, Services, Models
```

---

## 🗄️ Database Schema

### 👤 Users
| Column | Type | Notes |
|---|---|---|
| `Id` | int (PK) | Auto-increment |
| `FullName` | string | Display name |
| `Email` | string | Unique identifier |
| `PasswordHash` | string | BCrypt hashed |
| `Role` | string | `"Customer"` or `"Merchant"` |
| `CreatedAt` | datetime | UTC timestamp |

### 🏪 Merchants
| Column | Type | Notes |
|---|---|---|
| `Id` | int (PK) | Auto-increment |
| `UserId` | int (FK → Users) | One-to-one with User |
| `BusinessName` | string | Store display name |
| `Description` | string | Store description |
| `CreatedAt` | datetime | UTC timestamp |

### 📦 Products
| Column | Type | Notes |
|---|---|---|
| `Id` | int (PK) | Auto-increment |
| `MerchantId` | int (FK → Merchants) | Ownership chain |
| `Name` | string | Product name |
| `Description` | string | Product details |
| `Price` | decimal | Current price |
| `CreatedAt` | datetime | UTC timestamp |

### 🛒 CartItems
| Column | Type | Notes |
|---|---|---|
| `Id` | int (PK) | Auto-increment |
| `UserId` | int (FK → Users) | Cart owner |
| `ProductId` | int (FK → Products) | Product in cart |
| `Quantity` | int | Item count |
| `AddedAt` | datetime | UTC timestamp |

> ℹ️ There is no `Cart` table — the cart is implicitly all `CartItem` rows for a given `UserId`.

### 📋 Orders
| Column | Type | Notes |
|---|---|---|
| `Id` | int (PK) | Auto-increment |
| `UserId` | int (FK → Users) | Customer who ordered |
| `TotalAmount` | decimal | Sum of all line items |
| `Status` | string | `"Pending"` → `"Paid"` |
| `PlacedAt` | datetime | UTC timestamp |

### 📝 OrderItems
| Column | Type | Notes |
|---|---|---|
| `Id` | int (PK) | Auto-increment |
| `OrderId` | int (FK → Orders) | Parent order |
| `ProductId` | int (FK → Products) | Product reference |
| `MerchantId` | int (FK → Merchants) | Denormalized for merchant queries |
| `ProductName` | string | ⚡ Snapshot at checkout time |
| `UnitPrice` | decimal | ⚡ Snapshot at checkout time |
| `Quantity` | int | Item count |
| `LineTotal` | decimal | `UnitPrice × Quantity` |

> ⚡ Price and name are snapshotted at checkout — future merchant edits don't affect past orders.

### 💳 Payments
| Column | Type | Notes |
|---|---|---|
| `Id` | int (PK) | Auto-increment |
| `OrderId` | int (FK → Orders) | One-to-one with Order |
| `Status` | string | `"Pending"` / `"Completed"` / `"Failed"` |
| `PaymentUrl` | string | Dummy gateway URL with GUID |
| `InitiatedAt` | datetime | UTC timestamp |
| `CompletedAt` | datetime? | Nullable — set on completion |

### 🔗 Entity Relationship Diagram

```
Users (1) ──────────────── (0..1) Merchants (1) ──── (many) Products
  │                                                          │
  │                                                          │
(many) CartItems ──────────────────────────────── (FK) ProductId
  │
(many) Orders (1) ──── (many) OrderItems ──── (FK) MerchantId
          │
        (1) Payments
```

---

## 📡 API Endpoints

### 🔐 Auth — Public
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Register as Customer or Merchant |
| `POST` | `/api/auth/login` | Login and receive JWT token |

### 🏪 Merchants
| Method | Endpoint | Role | Description |
|---|---|---|---|
| `GET` | `/api/merchant` | Public | Browse all merchants |
| `GET` | `/api/merchant/{id}` | Public | View a single merchant |
| `GET` | `/api/merchant/{id}/products` | Public | View products of a merchant |
| `POST` | `/api/merchant/profile` | Merchant | Create store profile |
| `GET` | `/api/merchant/my-profile` | Merchant | View own profile |

### 📦 Products
| Method | Endpoint | Role | Description |
|---|---|---|---|
| `GET` | `/api/product` | Public | Browse all products |
| `GET` | `/api/product/{id}` | Public | View a single product |
| `POST` | `/api/product` | Merchant | Create a product |
| `PUT` | `/api/product/{id}` | Merchant | Update own product |

### 🛒 Cart
| Method | Endpoint | Role | Description |
|---|---|---|---|
| `GET` | `/api/cart` | Customer | View cart with totals |
| `POST` | `/api/cart` | Customer | Add product to cart |
| `PUT` | `/api/cart/{cartItemId}` | Customer | Update item quantity |
| `DELETE` | `/api/cart/{cartItemId}` | Customer | Remove item from cart |

### 📋 Orders
| Method | Endpoint | Role | Description |
|---|---|---|---|
| `POST` | `/api/order/checkout` | Customer | Checkout cart → create order |
| `GET` | `/api/order` | Customer | View all own orders |
| `GET` | `/api/order/{id}` | Customer | View a single order |
| `GET` | `/api/order/merchant-items` | Merchant | View own order line items |

### 💳 Payments
| Method | Endpoint | Role | Description |
|---|---|---|---|
| `POST` | `/api/payment/initiate/{orderId}` | Customer | Initiate payment |
| `POST` | `/api/payment/complete/{paymentId}` | Customer | Complete payment |
| `GET` | `/api/payment/status/{orderId}` | Customer | Check payment status |

---

## 🚀 Features & Functionality

### 🔐 Authentication & Authorization
- Register as **Customer** or **Merchant** — JWT issued immediately on registration
- Role claims embedded in JWT (`Customer` / `Merchant`)
- `[Authorize(Roles = "...")]` enforced on all protected endpoints
- JWT interceptor in Angular automatically attaches `Authorization: Bearer <token>` to every HTTP request

### 🏪 Merchant Features
- Create a store profile (one per user, enforced in service layer)
- List, create, and update products (ownership verified via `User → Merchant → Product` chain)
- View incoming order items — only items from their own products, never other merchants' data

### 🛍️ Customer Features
- Browse all merchants and their product catalogs (no login required)
- Add products to cart — duplicate products merge quantities (idempotent)
- Update quantities and remove items from cart
- Checkout — cart converts to order with price snapshots, cart is cleared atomically
- Initiate and complete payment simulation
- View full order history with payment status

### 💳 Payment Flow
```
POST /api/payment/initiate/{orderId}   → generates dummy gateway URL
POST /api/payment/complete/{paymentId} → marks Payment=Completed, Order=Paid
GET  /api/payment/status/{orderId}     → check current status
```
Idempotent initiate — calling initiate twice returns the existing pending payment instead of creating a duplicate.

### 🔒 Ownership Enforcement
All data access is filtered by the `userId` or `merchantId` extracted from the JWT — never from user-supplied request parameters. A customer cannot access another customer's cart, orders, or payments.

---

## 🐳 Docker Setup

### Docker Images

| Service | Image | Port |
|---|---|---|
| 🗄️ Database | `postgres:16-alpine` | `5432` |
| ⚙️ Backend | `titan0709/multiclient-backend:latest` | `8080` |
| 🎨 Frontend | `titan0709/multiclient-frontend:latest` | `4200 → 80` |

### ▶️ Run with Docker Compose (Recommended)

The fastest way to run the entire stack:

```bash
# Clone the repository
git clone https://github.com/<your-username>/MultiClientPlatform.git
cd MultiClientPlatform

# Start all services
docker compose up -d
```

| Service | URL |
|---|---|
| 🎨 Frontend | http://localhost:4200 |
| ⚙️ Backend API | http://localhost:8080 |
| 📖 Swagger UI | http://localhost:8080/swagger |

### 🛑 Stop Services

```bash
docker compose down

# Remove volumes (wipes database)
docker compose down -v
```

### `docker-compose.yml`

```yaml
services:

  db:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: multiclientdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: <your-db-password>
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  backend:
    image: titan0709/multiclient-backend:latest
    ports:
      - "8080:8080"
    environment:
      ConnectionStrings__DefaultConnection: "Host=db;Port=5432;Database=multiclientdb;Username=postgres;Password=<your-db-password>"
      Jwt__Key: "<your-jwt-secret-at-least-32-chars>"
      Jwt__Issuer: "MultiClientPlatform"
      Jwt__Audience: "MultiClientPlatformUsers"
      Jwt__ExpiryMinutes: "60"
      ASPNETCORE_URLS: "http://+:8080"
      ASPNETCORE_ENVIRONMENT: "Development"
    depends_on:
      - db

  frontend:
    image: titan0709/multiclient-frontend:latest
    ports:
      - "4200:80"
    depends_on:
      - backend

volumes:
  pgdata:
```

> ✅ EF Core migrations run **automatically** on backend startup — no manual migration step needed.

### 🔨 Build Images Locally

```bash
# Build backend image
cd MultiClientPlatform.Api
docker build -t multiclient-backend .

# Build frontend image
cd ../marketplace-ui
docker build -t multiclient-frontend .
```

---

## 💻 Local Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) + npm
- [PostgreSQL 16](https://www.postgresql.org/) (or use Docker for DB only)
- [Angular CLI](https://angular.io/cli) — `npm install -g @angular/cli`

### Backend

```bash
cd MultiClientPlatform.Api

# Update connection string in appsettings.Development.json
# Then run:
dotnet run
```

API available at: `https://localhost:5001` | Swagger: `https://localhost:5001/swagger`

### Frontend

```bash
cd marketplace-ui
npm install
ng serve
```

Frontend available at: `http://localhost:4200`

> The Angular dev server proxies `/api/*` requests to the backend via `proxy.conf.json`.

---

## ⚙️ Environment Variables

### Backend (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=multiclientdb;Username=postgres;Password=<password>"
  },
  "Jwt": {
    "Key": "<secret-key-at-least-32-characters>",
    "Issuer": "MultiClientPlatform",
    "Audience": "MultiClientPlatformUsers",
    "ExpiryMinutes": "60"
  }
}
```

### Frontend (`src/environments/environment.ts`)

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:8080'
};
```

---

## 🔒 Security

| Concern | Mechanism |
|---|---|
| 🔑 Password storage | BCrypt hash with random salt |
| 🪙 Identity proof | JWT signed with HMAC-SHA256 |
| 🛡️ Role enforcement | `[Authorize(Roles = "...")]` checks Role claim |
| 👤 Ownership enforcement | Service layer compares JWT `userId` to entity's `userId`/`merchantId` |
| 🚫 Cross-user data access | All queries filtered by JWT claims — never from request body |
| ⏱️ Token expiry | Configurable via `Jwt:ExpiryMinutes` |
| 🌐 CORS | Restricted to `http://localhost:4200` only |

---

## 📁 Project Structure

```
CaseStudy/
├── MultiClientPlatform.Api/        ← ASP.NET Core Web API
│   ├── Features/
│   │   ├── Auth/                   ← Register, Login, JWT
│   │   ├── Merchants/              ← Store profiles
│   │   ├── Products/               ← Product catalog
│   │   ├── Cart/                   ← Shopping cart
│   │   ├── Orders/                 ← Checkout & order history
│   │   └── Payments/               ← Payment simulation
│   ├── Data/
│   │   └── ApplicationDbContext.cs ← EF Core DbContext
│   ├── Helpers/
│   │   └── JwtHelper.cs            ← JWT generation
│   ├── Migrations/                 ← EF Core migrations
│   ├── Dockerfile
│   └── Program.cs                  ← DI, middleware pipeline
│
├── marketplace-ui/                 ← Angular 21 SPA
│   ├── src/app/
│   │   ├── auth/                   ← Login & Register
│   │   ├── catalog/                ← Browse merchants & products
│   │   ├── cart/                   ← Cart management
│   │   ├── order/                  ← Order history
│   │   ├── payment/                ← Payment flow
│   │   ├── merchant/               ← Merchant dashboard
│   │   └── shared/                 ← Guards, Interceptors, Services
│   ├── Dockerfile
│   └── nginx.conf
│
├── Docs/
│   └── ApiEndpoints.md             ← Full API reference
├── docker-compose.yml
└── README.md
```

---

## 🗺️ End-to-End User Journeys

### 🛍️ Customer Journey
```
1. Register as Customer          → JWT issued
2. Browse merchants & products   → No auth required
3. Add products to cart          → JWT required
4. Review & update cart
5. Checkout                      → Order created, cart cleared
6. Initiate payment              → Dummy gateway URL returned
7. Complete payment              → Order marked as Paid
8. View order history            → All orders with status
```

### 🏪 Merchant Journey
```
1. Register as Merchant          → JWT issued
2. Create store profile          → One profile per account
3. Add & manage products         → Ownership enforced
4. View orders received          → Only own product line items
```

---

## 📄 License

This project is built as a case study for learning purposes.

---

<div align="center">
  Built with ❤️ using <strong>ASP.NET Core</strong> · <strong>Angular</strong> · <strong>PostgreSQL</strong> · <strong>Docker</strong>
</div>
