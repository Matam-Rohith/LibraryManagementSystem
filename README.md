# 📚 Library Management System — ASP.NET Core 8 Web API

A production-ready Library Management System built with **ASP.NET Core 8**, **Entity Framework Core**, **SQL Server**, and **JWT Authentication**.

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| API Framework | ASP.NET Core 8 Web API |
| Language | C# 12 |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (LocalDB for dev) |
| Authentication | ASP.NET Identity + JWT Bearer |
| API Docs | Swagger / OpenAPI |
| CI/CD | GitHub Actions |
| Architecture | Repository Pattern + Service Layer + DI |

## 🚀 Getting Started

```bash
git clone https://github.com/Matam-Rohith/LibraryManagementSystem.git
cd LibraryManagementSystem
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

Open Swagger at: `https://localhost:7001/swagger`

## 🔐 Default Admin

| Email | Password |
|---|---|
| admin@library.com | Admin@123456 |

## 📡 API Endpoints

- `POST /api/auth/register` — Register
- `POST /api/auth/login` — Login
- `GET/POST/PUT/DELETE /api/books` — Book CRUD (Admin)
- `POST /api/borrow/issue` — Issue book (Admin)
- `POST /api/borrow/return` — Return book (Admin)
- `GET /api/borrow/overdue` — Overdue list (Admin)
- `GET/POST /api/fines` — Fine management
- `GET/POST /api/reservations` — Reservations

## 👤 Author

**Matam Rohith** — [GitHub](https://github.com/Matam-Rohith) | [Portfolio](https://rohith-portfolio-six.vercel.app/)
