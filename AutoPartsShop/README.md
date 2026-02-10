🚗🔧 AutoPartsShop API
AutoPartsShop is a real-world ASP.NET Core Web API for an online auto parts store, built with a strong focus on security 🔐, clean architecture 🧱, and production-ready practices 🚀.
The project implements a complete modern authentication flow, cart and order management, and role-based access (Admin / User).
This backend is ready to be consumed by a frontend UI (React planned ⚛️).
🛠️ TECH STACK
• ASP.NET Core Web API • Entity Framework Core with Migrations • PostgreSQL 🐘 • JWT Authentication (Access Tokens) • Refresh Token Rotation 🔁 • BCrypt password hashing 🔐 • Rate Limiting (AspNetCoreRateLimit) 🚦 • Swagger / OpenAPI 📄 • CORS (Frontend-ready 🌐) • Global Error Handling (ProblemDetails ⚠️) • Git & GitHub 🧑‍💻
🔐 AUTHENTICATION & SECURITY
• Login using JWT Access Token (short-lived ⏱️) • Refresh Token Rotation implemented – each refresh invalidates the previous refresh token – refresh tokens are stored hashed in the database (HMAC + server-side pepper) • Logout is allowed only when the access token is valid
🛡️ Refresh Token Audit • IsRevoked • RevokedAt • RevokedByUserId
• Passwords are securely stored using BCrypt • Every authenticated request validates: – user existence – user active state – user not deleted
🛒 CART SYSTEM
• Add, update, and remove products from cart • Cart is user-specific 👤 • UI-friendly cart responses include: – list of items – total cart value 💰 – totalItems (sum of quantities, perfect for UI badge 🛍️)
📦 ORDERS
• Checkout creates an order from the cart • Atomic stock handling (all-or-nothing ⚙️) • Users can view their order history • Orders can be cancelled according to business rules
👑 Admin Features • Paginated and sortable order list • Update order status
📐 DTO-BASED API DESIGN
• Entity Framework Core entities are never exposed directly • All input and output is handled via dedicated DTOs • Clean, predictable, UI-friendly API design
🚦 RATE LIMITING
• IP-based rate limiting enabled • Configurable via appsettings.json • Applied before authentication to prevent abuse
⚠️ ERROR HANDLING
• Global exception handler implemented • All errors returned as JSON using ProblemDetails
Predictable HTTP responses: • 400 – Bad Request • 401 – Unauthorized • 403 – Forbidden • 404 – Not Found • 500 – Internal Server Error
🌐 CORS
Configured for frontend applications: • http://localhost:5173 → React + Vite ⚛️ • http://localhost:3000 → React CRA
📄 SWAGGER / OPENAPI
• Swagger UI enabled • JWT authentication supported via Authorize button • Entire API is documented and testable
🗄️ DATABASE & MIGRATIONS
• Entity Framework Core Migrations (Alembic equivalent in .NET) • Database migrations applied automatically at startup • Expired refresh tokens cleaned up automatically in development
🚀 RUNNING THE PROJECT
Prerequisites: • .NET SDK • PostgreSQL
Run steps: • dotnet restore • dotnet run
👤 SEEDED ACCOUNTS (DEVELOPMENT)
ADMIN Email: admin@gmail.com
Password: adminA1.
USER Email: user1@gmail.com
Password: userA11.
📌 PROJECT STATUS
• Backend is stable & production-ready ✅ • Designed for real-world usage • Next steps: – React + Bootstrap frontend ⚛️🎨 – Auth integration with refresh token rotation – Admin dashboard
📝 NOTES
This project is intended as: • A professional backend portfolio project • A solid foundation for a real production application • An example of modern authentication and API design in ASP.NET Core

