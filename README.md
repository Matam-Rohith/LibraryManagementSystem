# Libra — Library Management System

A full-stack, production-quality Library Management System featuring a university book catalog, circulation checkout & return desk, patron reservations, overdue penalty calculations, financial fine ledger, Open Library global search & import, and audit logging.

## Core Features

- **University Catalog**: 50 pre-indexed textbooks across Computer Science, Electronics, Mechanical Engineering, Mathematics, Physics, Chemistry, Medicine, Law, and Business.
- **Search & Filtering**: Real-time debounced title/author/ISBN/category search with availability filters and sorting (A–Z, publication year, available copies).
- **Circulation Counter**: Rapid staff checkout station with patron selection, customizable loan durations (7, 14, 21, 28 days), automatic stock tracking, and return processing.
- **Overdue & Penalty Engine**: Automatic calculation of overdue loan days (standard rate ₹10/day overdue), instant fine generation upon return, and payment settlement ledger.
- **Patron Hold Requests**: 3-day reservation holds for high-demand titles with staff conversion to active loans.
- **Open Library Global Integration**: Live search against millions of books via Open Library API with 1-click catalog import.
- **Operational Audit Log**: Immutable timestamped event trail tracking circulation actions, stock modifications, and fine clearances.
- **Role-Based Access Control**: JWT authentication with distinct `Admin`, `Assistant`, and `Member` privileges.

## Technology Stack

- **Backend**: Node.js 22, Express 4, TypeScript 5, tsx
- **Frontend**: Responsive Single-Page Application (vanilla ES6+, semantic HTML5, modern design tokens)
- **Security**: JWT (`jsonwebtoken`), bcrypt password hashing (`bcryptjs`), CORS
- **APIs**: RESTful JSON architecture with support for both modern camelCase and legacy snake_case contracts

## Default Accounts (Reviewer Access)

| Role | Email | Password | Membership ID |
|---|---|---|---|
| **Admin** | `admin@library.com` | `Admin@123456` | `LIB-ADMIN-001` |
| **Member** | `member@library.com` | `Member@123456` | `LIB-MEMBER-001` |

## API Endpoints

### Authentication & Users
- `POST /api/auth/register` — Register a new patron
- `POST /api/auth/login` — Sign in and obtain JWT bearer token
- `GET /api/auth/me` — Retrieve active authenticated profile
- `GET /api/members` — List all registered library patrons

### Books & Catalog
- `GET /api/books` — Query catalog with optional `search` and `category` parameters
- `GET /api/books/:id` — Retrieve specific book bibliographic details
- `POST /api/books` — Accession a new title to the collection
- `PUT /api/books/:id` — Update bibliographic information or stock
- `DELETE /api/books/:id` — Remove a book from circulation

### Circulation & Loans
- `GET /api/borrow` — List all loans (or current patron's loans)
- `GET /api/borrow/user/:userId` — List loans for a specific patron
- `GET /api/borrow/overdue` — List overdue loans
- `POST /api/borrow/issue` — Issue a new loan (`userId`, `bookId`, `dueDays`)
- `POST /api/borrow/return` — Process return and calculate overdue penalties

### Holds & Reservations
- `GET /api/reservations` — List active reservations
- `POST /api/reservations` — Place a hold on a title
- `POST /api/reservations/:id/fulfill` — Convert hold into active loan
- `DELETE /api/reservations/:id` — Cancel hold

### Fines & Ledger
- `GET /api/fines` — Retrieve penalty ledger records
- `POST /api/fines/pay` — Settle and record fine payment

### Open Library & Audit
- `GET /api/openlibrary/search?query=...` — Live Open Library search proxy
- `POST /api/openlibrary/import` — Accession external title into university catalog
- `GET /api/activitylog` — Operational event audit trail
- `GET /api/stats` — Real-time catalog KPI metrics
- `GET /health` — Service health monitor

## Getting Started

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Run TypeScript compilation check
npm run build

# Run linter
npm run lint
```
