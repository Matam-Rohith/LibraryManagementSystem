# Library Management System

ASP.NET Core 8 REST API for managing library operations — books, members, borrowing, reservations, and fines.

[![CI/CD](https://github.com/Matam-Rohith/LibraryManagementSystem/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/Matam-Rohith/LibraryManagementSystem/actions/workflows/ci-cd.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue)](https://www.postgresql.org/)
[![MongoDB](https://img.shields.io/badge/MongoDB-7.0-green)](https://www.mongodb.com/)
[![Docker](https://img.shields.io/badge/Docker-ready-blue)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Tech Stack

- ASP.NET Core 8 Web API
- PostgreSQL via EF Core (Npgsql)
- MongoDB for activity logs
- Docker + Docker Compose
- JWT authentication
- Role-based access control (Admin / Member)
- Swagger / OpenAPI
- xUnit + Moq unit tests
- Open Library external API integration
- GitHub Actions CI/CD
- AWS ECS Fargate deployment config
- Health checks endpoint
- Serilog structured logging

## Architecture

```
Controllers -> Services -> Repositories -> EF Core -> PostgreSQL
     |               |
 ActivityLogService   OpenLibraryService
     |               |
  MongoDB         Open Library API
```

## Running Locally with Docker

```bash
git clone https://github.com/Matam-Rohith/LibraryManagementSystem.git
cd LibraryManagementSystem
docker-compose up --build
```

Swagger UI: http://localhost:8080/swagger

Default admin account:
- Email: `admin@library.com`
- Password: `Admin@123456`

## Role Permissions

| Endpoint | Admin | Member |
|---|---|---|
| GET /api/books | yes | yes |
| POST /api/books | yes | no |
| DELETE /api/books | yes | no |
| POST /api/borrow | yes | yes |
| GET /api/activitylog | yes | no |
| GET /api/openlibrary/search | yes | yes |

## Running Tests

```bash
cd Tests
dotnet test --verbosity normal
```

## External API

`GET /api/openlibrary/search?query=...` queries [Open Library](https://openlibrary.org/developers/api) and returns book metadata (title, authors, publish year, cover image, ISBN). Useful for looking up book details before adding them to the catalog.

`GET /api/openlibrary/isbn/{isbn}` fetches a specific book by ISBN.

## AWS Deployment

See [aws/README-AWS-Deployment.md](aws/README-AWS-Deployment.md) for deploying to AWS ECS Fargate with RDS PostgreSQL and MongoDB Atlas.

## Activity Logs

All significant actions are written to MongoDB with the following fields:
- User ID and email
- Role
- Entity type and ID
- Timestamp (UTC)
- IP address

Logs are automatically deleted after 90 days via a MongoDB TTL index.

## Environment Variables

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `MongoDB__ConnectionString` | MongoDB connection string |
| `MongoDB__DatabaseName` | MongoDB database name |
| `Jwt__Key` | JWT signing key (min 32 chars) |
| `Jwt__Issuer` | JWT issuer |
| `Jwt__Audience` | JWT audience |

## License

MIT
