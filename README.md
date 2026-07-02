# Library Management System

REST API for managing library operations built with Python and Flask.

[![CI/CD](https://github.com/Matam-Rohith/LibraryManagementSystem/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/Matam-Rohith/LibraryManagementSystem/actions/workflows/ci-cd.yml)
[![Python](https://img.shields.io/badge/Python-3.12-blue)](https://www.python.org/)
[![Flask](https://img.shields.io/badge/Flask-3.0-black)](https://flask.palletsprojects.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue)](https://www.postgresql.org/)
[![MongoDB](https://img.shields.io/badge/MongoDB-7.0-green)](https://www.mongodb.com/)
[![Docker](https://img.shields.io/badge/Docker-ready-blue)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Tech Stack

- Python 3.12
- Flask 3.0 with Blueprints
- PostgreSQL 16 via SQLAlchemy + Flask-SQLAlchemy
- Flask-Migrate (Alembic) for database migrations
- MongoDB 7 (PyMongo) for activity logs with 90-day TTL
- JWT authentication via Flask-JWT-Extended
- Role-based access control (Admin / Member)
- Flasgger for Swagger / OpenAPI docs
- Open Library external API integration
- Gunicorn WSGI server
- Docker + Docker Compose
- GitHub Actions CI/CD
- AWS ECS Fargate deployment config
- pytest + pytest-flask for unit and integration tests

## Project Structure

```
.
+-- app/
|   +-- __init__.py         # App factory
|   +-- models/             # SQLAlchemy models (User, Book, BorrowRecord, Reservation, Fine)
|   +-- routes/             # Flask Blueprints (auth, books, borrow, reservations, fines, logs, openlibrary)
|   +-- services/           # Business logic (activity_log_service)
|   +-- utils/              # Decorators (admin_required)
+-- tests/                  # pytest tests
+-- config.py               # Config and TestingConfig
+-- run.py                  # App entrypoint
+-- seed.py                 # DB seeder (roles + admin user)
+-- requirements.txt
+-- requirements-test.txt
+-- Dockerfile
+-- docker-compose.yml
+-- .github/workflows/ci-cd.yml
```

## Running Locally with Docker

```bash
git clone https://github.com/Matam-Rohith/LibraryManagementSystem.git
cd LibraryManagementSystem
cp .env.example .env
docker-compose up --build
```

Swagger UI: http://localhost:8080/apidocs

Health check: http://localhost:8080/health

Default admin account:
- Email: admin@library.com
- Password: Admin@123456

## Running Locally without Docker

```bash
python -m venv venv
source venv/bin/activate
pip install -r requirements.txt
cp .env.example .env
python seed.py
python run.py
```

## Running Tests

```bash
pip install -r requirements-test.txt
pytest tests/ -v --cov=app
```

## API Endpoints

| Method | Endpoint | Role | Description |
|---|---|---|---|
| POST | /api/auth/register | Public | Register a new member |
| POST | /api/auth/login | Public | Login and get JWT token |
| GET | /api/auth/me | Any | Get current user info |
| GET | /api/books | Any | List all books |
| POST | /api/books | Admin | Add a new book |
| PUT | /api/books/:id | Admin | Update a book |
| DELETE | /api/books/:id | Admin | Delete a book |
| GET | /api/borrow | Any | Get borrow records |
| POST | /api/borrow | Any | Borrow a book |
| PUT | /api/borrow/:id/return | Any | Return a book |
| GET | /api/reservations | Any | Get reservations |
| POST | /api/reservations | Any | Reserve a book |
| PUT | /api/reservations/:id/cancel | Any | Cancel reservation |
| GET | /api/fines | Any | Get fines |
| PUT | /api/fines/:id/pay | Any | Pay a fine |
| GET | /api/activitylogs | Admin | Get MongoDB activity logs |
| GET | /api/activitylogs/user/:id | Admin | Get logs by user |
| GET | /api/openlibrary/search | Any | Search Open Library |
| GET | /api/openlibrary/isbn/:isbn | Any | Lookup book by ISBN |
| GET | /health | Public | Health check |

## Role Permissions

| Action | Admin | Member |
|---|---|---|
| Add / Edit / Delete books | yes | no |
| Borrow and return books | yes | yes |
| Reserve books | yes | yes |
| Pay fines | yes | yes |
| View activity logs | yes | no |
| Search Open Library | yes | yes |

## Environment Variables

| Variable | Description |
|---|---|
| DATABASE_URL | PostgreSQL connection string |
| MONGO_URI | MongoDB connection string |
| MONGO_DB_NAME | MongoDB database name |
| JWT_SECRET_KEY | JWT signing key |
| SECRET_KEY | Flask session secret |

## AWS Deployment

See aws/README-AWS-Deployment.md for deploying to AWS ECS Fargate.

## License

MIT
