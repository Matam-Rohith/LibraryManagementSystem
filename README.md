# 📚 Library Management System

A production-grade **ASP.NET Core 8** REST API for managing library operations — built with PostgreSQL, MongoDB, Docker, JWT authentication, role-based access control, CI/CD, and AWS deployment support.

[![CI/CD](https://github.com/Matam-Rohith/LibraryManagementSystem/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/Matam-Rohith/LibraryManagementSystem/actions/workflows/ci-cd.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue)](https://www.postgresql.org/)
[![MongoDB](https://img.shields.io/badge/MongoDB-7.0-green)](https://www.mongodb.com/)
[![Docker](https://img.shields.io/badge/Docker-ready-blue)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## 🚀 Live Demo

> Hosted on **Render.com** (free tier — may spin down after inactivity)

- **Swagger UI**: `https://your-render-url.onrender.com/swagger`
- **Health Check**: `https://your-render-url.onrender.com/health`

## ✨ Features

| Feature | Status |
|---|---|
| ASP.NET Core 8 Web API | ✅ |
| PostgreSQL (EF Core) | ✅ |
| MongoDB Activity Logs | ✅ |
| Docker + Docker Compose | ✅ |
| JWT Authentication | ✅ |
| Role-Based Access Control (Admin/Member) | ✅ |
| Swagger / OpenAPI Docs | ✅ |
| Unit Tests (xUnit + Moq) | ✅ |
| Open Library External API | ✅ |
| GitHub Actions CI/CD | ✅ |
| AWS ECS Fargate Deployment | ✅ |
| Health Checks | ✅ |
| Serilog Structured Logging | ✅ |

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────┐
│                  Library API (ASP.NET Core 8)        │
│                                                     │
│  Controllers → Services → Repositories → EF Core   │
│       ↓              ↓                              │
│  ActivityLogService    OpenLibraryService           │
│       ↓              ↓                              │
│    MongoDB        Open Library API (external)       │
└────────────────────────┬────────────────────────────┘
                         ↓
                    PostgreSQL
```

## 🐳 Quick Start with Docker

```bash
git clone https://github.com/Matam-Rohith/LibraryManagementSystem.git
cd LibraryManagementSystem
docker-compose up --build
```

Then open: http://localhost:8080/swagger

**Default Admin credentials:**
- Email: `admin@library.com`  
- Password: `Admin@123456`

## 🔐 Role-Based Access Control

| Endpoint | Admin | Member |
|---|---|---|
| GET /api/books | ✅ | ✅ |
| POST /api/books | ✅ | ❌ |
| DELETE /api/books | ✅ | ❌ |
| POST /api/borrow | ✅ | ✅ |
| GET /api/activitylog | ✅ | ❌ |
| GET /api/openlibrary/search | ✅ | ✅ |

## 🧪 Running Tests

```bash
cd Tests
dotnet test --verbosity normal
```

## 🌐 External API Integration

The `/api/openlibrary/search` endpoint queries the [Open Library API](https://openlibrary.org/developers/api) to find book metadata by title, author, or keyword. Use it to get book details before adding them to your library.

## ☁️ AWS Deployment

See [`aws/README-AWS-Deployment.md`](aws/README-AWS-Deployment.md) for a step-by-step guide to deploy on **AWS ECS Fargate** with RDS PostgreSQL and MongoDB Atlas.

## 📊 MongoDB Activity Logs

Every significant action (login, borrow, return, add/delete book) is logged to MongoDB with:
- User ID & email
- Role (Admin/Member)
- Entity type & ID
- Timestamp (UTC)
- IP address
- TTL: auto-deleted after 90 days

## 🔧 Environment Variables

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `MongoDB__ConnectionString` | MongoDB connection string |
| `MongoDB__DatabaseName` | MongoDB database name |
| `Jwt__Key` | JWT signing key (min 32 chars) |
| `Jwt__Issuer` | JWT issuer |
| `Jwt__Audience` | JWT audience |

## 📄 License

MIT License — see [LICENSE](LICENSE)
