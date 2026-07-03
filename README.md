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

