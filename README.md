# Logistics Asset Tracker

Internal business dashboard for tracking reusable logistics assets (pallets, crates, boxes, containers, tools, devices, warehouse equipment) via manual updates and authenticated QR scanning.

Full requirements, architecture, and business rules live in `docs/`. See `CLAUDE.md` for the source-of-truth document map and working rules. This README covers local setup only.

## Stack

- Frontend: React + TypeScript (Vite)
- Backend: ASP.NET Core Web API (.NET 10)
- Database: PostgreSQL
- ORM: Entity Framework Core (Npgsql provider)
- Charts: Recharts
- QR generation: qrcode.react (frontend)
- QR scanning: @yudiel/react-qr-scanner (frontend)

## Project Status

Phase 1 (project setup) complete. No business logic, authentication, or entities are implemented yet — see `docs/10_Phase_Implementation_Plan.md` for the phased build order.

## Prerequisites

- .NET SDK 10+
- Node.js 20+ and npm
- Docker Desktop (for local PostgreSQL) — or a PostgreSQL 16 instance you point the backend at yourself

## Local Setup

### 1. Database

From the repo root:

```
docker compose up -d
```

This starts PostgreSQL 16 on `localhost:5432` with the credentials already configured in `backend/LogisticsAssetTracker.Api/appsettings.Development.json`. These are local development defaults only, not production secrets.

### 2. Backend

```
cd backend/LogisticsAssetTracker.Api
dotnet run
```

Runs on `http://localhost:5080` by default (see `Properties/launchSettings.json`). On startup in Development, it logs a PostgreSQL connectivity check result.

### 3. Frontend

```
cd frontend
npm install
cp .env.example .env
npm run dev
```

Runs on `http://localhost:5173` by default. `.env` is gitignored; copy `.env.example` and adjust `VITE_API_BASE_URL` if the backend runs on a different port.

## Folder Structure

```
backend/LogisticsAssetTracker.Api/
  Controllers/     Data/            Domain/Entities/
  Services/        Domain/Enums/    Validators/
  Dtos/            Auth/            Configuration/
  Common/

frontend/src/
  api/  auth/  components/  pages/  types/  utils/
  features/
    assets/  locations/  movements/  dashboard/
    approvals/  risk/  users/  auditLogs/  suspiciousActivity/
```

## Known Limitations (Phase 1)

- No authentication, entities, or business logic yet.
- The default ASP.NET Core template dependency `Microsoft.OpenApi 2.0.0` and the default Vite-installed `Microsoft.AspNetCore.OpenApi`/npm audit chain currently carry known advisories tracked separately; see Phase 1 handoff notes for details before addressing.
