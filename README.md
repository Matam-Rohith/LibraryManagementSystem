# 📚 Library Management System — ASP.NET Core 8 Web API

A production-ready Library Management System built with **ASP.NET Core 8**, **Entity Framework Core**, **SQL Server**, and **JWT Authentication**.

[![.NET CI/CD](https://github.com/Matam-Rohith/LibraryManagementSystem/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Matam-Rohith/LibraryManagementSystem/actions/workflows/dotnet.yml)

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
| Hosting | Railway / Render (Docker) |
| Architecture | Repository Pattern + Service Layer + DI |

---

## 🚀 Getting Started (Local)

```bash
git clone https://github.com/Matam-Rohith/LibraryManagementSystem.git
cd LibraryManagementSystem
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

Open Swagger at: `https://localhost:7001/swagger`

---

## 🚂 Deploy on Railway (Free)

1. Go to [railway.app](https://railway.app) → **New Project** → **Deploy from GitHub Repo**
2. Select `Matam-Rohith/LibraryManagementSystem`
3. Add a **PostgreSQL** or **MySQL** plugin (Railway provides free DB)
4. Set these environment variables in Railway dashboard:

| Variable | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | *(Railway DB connection string)* |
| `Jwt__Key` | *(any strong secret, 32+ chars)* |
| `Jwt__Issuer` | `LibraryManagementSystemAPI` |
| `Jwt__Audience` | `LibraryManagementSystemClient` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

5. Railway auto-detects the `Dockerfile` and deploys 🎉
6. For **GitHub Actions auto-deploy**: copy your Railway token from dashboard → add as GitHub Secret `RAILWAY_TOKEN`

---

## 🎨 Deploy on Render (Free)

1. Go to [render.com](https://render.com) → **New Web Service** → Connect GitHub repo
2. Render auto-detects `render.yaml` and configures the service
3. Set these environment variables in Render dashboard (marked `sync: false`):

| Variable | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | *(Render PostgreSQL internal URL)* |
| `Jwt__Key` | *(any strong secret, 32+ chars)* |

4. Add a **PostgreSQL** database from Render dashboard (free tier)
5. Deploy! Your API will be live at `https://library-management-system.onrender.com`

---

## 🔐 Default Admin

| Email | Password |
|---|---|
| admin@library.com | Admin@123456 |

---

## 📡 API Endpoints

| Method | Endpoint | Role |
|---|---|---|
| GET | `/health` | Public |
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| GET | `/api/books` | Public |
| POST/PUT/DELETE | `/api/books` | Admin |
| POST | `/api/borrow/issue` | Admin |
| POST | `/api/borrow/return` | Admin |
| GET | `/api/borrow/overdue` | Admin |
| GET/POST | `/api/fines` | Admin/Member |
| GET/POST | `/api/reservations` | Member |

---

## 👤 Author

**Matam Rohith** — [GitHub](https://github.com/Matam-Rohith) | [Portfolio](https://rohith-portfolio-six.vercel.app/)
