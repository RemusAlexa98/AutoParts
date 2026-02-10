# AutoPartsShop API

<<<<<<< HEAD
AutoPartsShop este un **ASP.NET Core Web API** pentru un magazin online de piese auto, construit ca proiect real-world backend, cu accent pe **securitate, scalabilitate și bune practici**.  
Proiectul implementează un flux complet de autentificare modern (JWT + Refresh Tokens), gestionare coș de cumpărături, comenzi și roluri (Admin / User).

---

## 🔧 Tehnologii folosite
=======
AutoPartsShop is a **real-world ASP.NET Core Web API** for an online auto parts store, built with a strong focus on **security, clean architecture, and production-ready practices**.  
The project implements a complete modern authentication flow, cart & order management, and role-based access (Admin / User).

This repository represents a backend that is **ready to be consumed by a frontend UI** (React planned).

---

## 🛠️ Tech Stack
>>>>>>> 5f2b6f2 (Add README)

- **ASP.NET Core Web API**
- **Entity Framework Core + Migrations**
- **PostgreSQL**
<<<<<<< HEAD
- **JWT Authentication + Refresh Token Rotation**
- **BCrypt pentru parole**
- **Rate Limiting (AspNetCoreRateLimit)**
- **Swagger / OpenAPI**
- **CORS configurat pentru UI**
- **Global Error Handling (ProblemDetails)**
- **Git & GitHub (version control)**

---

## 🔐 Autentificare & Securitate

- Login cu **JWT Access Token** (expirare scurtă)
- **Refresh Token Rotation**:
  - fiecare refresh invalidează tokenul vechi
  - refresh token-urile sunt **hash-uite în DB (HMAC + pepper)**
- Logout permis **doar cu access token valid**
- Audit complet pentru refresh tokens:
  - `IsRevoked`
  - `RevokedAt`
  - `RevokedByUserId`
- Parolele sunt stocate cu **BCrypt**
- Validare la fiecare request:
  - user existent
  - user activ
  - user neșters

---

## 🛒 Cart (Coș de cumpărături)

- Add / Update / Remove produse din coș
- Coșul este **user-specific**
- Răspunsuri UI-friendly:
  ```json
  {
    "items": [...],
    "total": 349.90,
    "totalItems": 5
  }
=======
- **JWT Authentication (Access Tokens)**
- **Refresh Token Rotation**
- **BCrypt password hashing**
- **Rate Limiting (AspNetCoreRateLimit)**
- **Swagger / OpenAPI**
- **CORS (Frontend-ready)**
- **Global Error Handling (ProblemDetails)**
- **Git & GitHub**

---

## 🔐 Authentication & Security

- Login with **JWT Access Token** (short-lived)
- **Refresh Token Rotation**
  - each refresh invalidates the previous refresh token
  - refresh tokens are **stored hashed in the database (HMAC + pepper)**
- Logout is allowed **only with a valid access token**
- Full refresh token audit:
  - `IsRevoked`
  - `RevokedAt`
  - `RevokedByUserId`
- Passwords are securely stored using **BCrypt**
- Every authenticated request validates:
  - user existence
  - user active state
  - user not deleted

---

## 🛒 Cart System

- Add / Update / Remove products from cart
- Cart is **user-specific**
- UI-friendly responses:

```json
{
  "items": [...],
  "total": 349.90,
  "totalItems": 5
}
```

- `totalItems` = total quantity of products (perfect for UI badge)
- `total` = cart total price

---

## 📦 Orders

- Checkout from cart → creates an order
- **Atomic stock handling** (all-or-nothing)
- User order history
- Order cancellation with rules
- Admin features:
  - paginated & sortable order list
  - update order status

---

## 📐 DTO-Based API Design

The API never exposes EF Core entities directly.  
All input/output is handled through dedicated DTOs:

### Auth
- `LoginResponseDto`

### Products
- `ProductDto`
- `CreateProductDto`
- `UpdateProductDto`

### Cart
- `CartDto`
- `CartItemDto`
- `AddToCartRequest`
- `UpdateCartItemRequest`

### Orders
- `OrderDto`
- `OrderItemDto`
- `CheckoutResponseDto`
- `CancelOrderResponseDto`

---

## 🚦 Rate Limiting

- IP-based rate limiting
- Configurable via `appsettings.json`
- Applied before authentication

---

## ❗ Error Handling

- Global exception handler
- JSON responses using **ProblemDetails**
- Predictable HTTP responses for frontend:
  - 400 – Bad Request
  - 401 – Unauthorized
  - 403 – Forbidden
  - 404 – Not Found
  - 500 – Internal Server Error

---

## 🌐 CORS

Configured for frontend applications:
- `http://localhost:5173` (React + Vite)
- `http://localhost:3000` (React CRA)

---

## 🧪 Swagger / OpenAPI

- Swagger UI enabled
- JWT authentication supported via **Authorize**
- Full API documentation available for testing

---

## 🗄️ Database & Migrations

- **EF Core Migrations** (Alembic equivalent in .NET)
- Automatic migration at startup:
```csharp
context.Database.Migrate();
```
- Development cleanup for expired refresh tokens

---

## 🚀 Running the Project

### Prerequisites
- .NET SDK
- PostgreSQL

### Run
```bash
dotnet restore
dotnet run
```

The API will be available at:
```
https://localhost:xxxx
```

---

## 👤 Seeded Accounts (Development)

| Role  | Email              | Password |
|------|--------------------|----------|
| Admin | admin@gmail.com | adminA1. |
| User  | user1@gmail.com | userA11. |

---

## 📌 Project Status

- Backend: **stable & production-ready**
- Designed for real-world usage
- Next step:
  - React + Bootstrap frontend
  - Auth integration with refresh token rotation
  - Admin dashboard

---

## 📝 Notes

This project is intended as:
- a professional portfolio project
- a solid backend foundation for a real application
- an example of modern authentication and API design in ASP.NET Core
>>>>>>> 5f2b6f2 (Add README)
