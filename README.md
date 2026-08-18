# Logistics Asset Tracker

Internal business dashboard for tracking reusable logistics assets (pallets, crates, boxes, containers, tools, devices, warehouse equipment) via manual updates and authenticated QR scanning — with full movement rules, approval workflows, suspicious-activity detection, an operational dashboard, and rule-based risk recommendations with AI-assisted explanations.

Full requirements, architecture, and business rules live in `docs/`. See `CLAUDE.md` for the source-of-truth document map and working rules. This README covers local setup, feature overview, and a demo script.

## Stack

- Frontend: React + TypeScript (Vite)
- Backend: ASP.NET Core Web API (.NET 10)
- Database: PostgreSQL
- ORM: Entity Framework Core (Npgsql provider)
- Charts: Recharts
- QR generation: qrcode.react (frontend)
- QR scanning: @yudiel/react-qr-scanner (frontend)

## Project Status

**Demo-ready MVP.** All six planned phases are implemented:

1. Project setup
2. Core system — auth, roles, users, locations, assets
3. Basic tracking — QR generation/scanning, manual and QR movement history
4. Full movement rules, Lost-reactivation approvals, suspicious-activity detection, audit log review
5. Operational dashboard and rule-based risk recommendations with mocked AI explanations
6. Final quality pass — re-verified against `docs/11_Testing_and_Acceptance_Criteria.md`, UI consistency and error-message pass, this README

See `docs/10_Phase_Implementation_Plan.md` for the phase breakdown and `docs/11_Testing_and_Acceptance_Criteria.md` for the acceptance criteria this build was checked against.

**Out of scope for this MVP** (by design, not an oversight): GPS/live location tracking, live maps, push/email notifications, a mobile app, billing, and any AI control over business decisions — risk detection is rule-based first, and AI (currently mocked, no external API) only explains a risk the rules already found.

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

Runs on `http://localhost:5080` by default (see `Properties/launchSettings.json`). On startup it checks PostgreSQL connectivity, applies any pending EF Core migrations, and seeds demo data (see below) if the `Users` table is empty.

A development-only CORS policy allows the Vite dev server (`http://localhost:5173`) to call the API directly; it is not enabled outside the Development environment.

### 3. Frontend

```
cd frontend
npm install
cp .env.example .env
npm run dev
```

Runs on `http://localhost:5173` by default. `.env` is gitignored; copy `.env.example` and adjust `VITE_API_BASE_URL` if the backend runs on a different port.

## Demo Credentials

Seeded automatically on first backend run:

| Role | Email | Password | Notes |
|---|---|---|---|
| Admin | `admin@logisticstracker.test` | `Admin123!` | Full access |
| Manager | `manager@logisticstracker.test` | `Manager123!` | No user/location management |
| Operator | `operator@logisticstracker.test` | `Operator123!` | Assets, QR scanning, movement updates only |
| Operator (inactive) | `inactive@logisticstracker.test` | `Inactive123!` | Demonstrates that inactive users cannot log in |

A handful of demo locations and assets (including one already `Lost`, one already `Damaged`) are seeded alongside the users so the app isn't empty on first load.

## Feature Overview

- **Auth & roles** — JWT login, Admin/Manager/Operator role-based navigation and API authorization, inactive users blocked at login.
- **Users, Locations, Assets** — Admin-managed CRUD; asset code and QR token are always backend-generated, never client-supplied; Edit Asset cannot touch movement-controlled fields (location, status, condition).
- **QR & manual movement** — authenticated QR scanning (no public scan-and-update route), manual asset-code fallback, full Document 05 status/condition transition rules enforced server-side.
- **Approvals** — Operator Lost-to-Available requests go through a Manager/Admin approval queue; Manager/Admin have a separate dedicated direct-reactivation flow; Admin-only Lost-to-Retired.
- **Suspicious activity & audit logs** — blocked and completed movements are flagged against explicit suspicious-activity rules (repeated Lost-reactivation attempts, conflicting destinations, high-frequency updates, unauthorized Lost transitions); every blocked attempt and sensitive action is audit-logged; both are reviewable by Admin/Manager.
- **Dashboard** — live counts (not from stored recommendations), status/condition/location breakdowns, recent movements, and suspicious-activity/pending-approval previews.
- **Risk recommendations** — rule-based detection (DelayRisk, NoRecentUpdate, DamagedAsset, LostAsset, SuspiciousMovement) with a mocked AI explanation service and deterministic rule-based fallback if AI output is missing or invalid; AI never approves movements or overrides backend validation.

## Demo Script

A suggested walkthrough covering the main flows end to end (also the basis for this build's acceptance testing):

1. Log in as Admin.
2. Create a location.
3. Create an asset with status `Available` and condition `Good`.
4. View asset details.
5. View the generated QR code.
6. Scan the QR code (or use the manual asset-code fallback).
7. Submit a QR location update.
8. Review the movement history.
9. Submit a manual location update.
10. Attempt an invalid `Available → Delivered` movement.
11. Note the backend error and that the asset is unchanged.
12. Submit a valid `Available → InTransit` movement that also sets condition to `Damaged` in the same request.
13. Attempt to change the now-`Damaged` asset back to `Available`.
14. Note the blocked rule, and check the Suspicious Activity page for the flagged attempt.
15. As Manager/Admin, clear the condition back to `Good` through a manual movement (with a note).
16. Mark the asset as `Lost`.
17. Log out and log in as Operator.
18. Submit a Lost-reactivation request.
19. Confirm the asset stays `Lost` while the request is Pending.
20. Log in as Manager/Admin.
21. Generate a risk recommendation while the asset is still `Lost`.
22. Review the saved `LostAsset` recommendation.
23. Approve the reactivation request (with a decision note).
24. Confirm the updated asset state and movement history.
25. Confirm the approved movement preserved the original request's source type.
26. Review the Suspicious Activity page.
27. Review the Audit Logs page.
28. Review the Dashboard.

## Folder Structure

```
backend/LogisticsAssetTracker.Api/
  Controllers/     Data/            Domain/Entities/  Domain/Enums/
  Services/        Dtos/            Validators/       Auth/
  Common/          Migrations/

frontend/src/
  api/  auth/  components/  pages/  types/
  features/
    assets/  locations/  movements/  dashboard/
    approvals/  risk/  users/  auditLogs/  suspiciousActivity/
```

## Known Limitations

- AI risk explanations use an in-process mocked service (`MockAiRiskExplanationService`) — deterministic, templated text, no external API call or key. Swapping in a real model provider is future work, not part of this MVP.
- Two Document 06 dashboard endpoints (`/api/dashboard/suspicious-activity`, `/api/dashboard/pending-approvals`) were intentionally not built as separate endpoints — the Dashboard, Suspicious Activity, and Approvals pages reuse the existing `GET /api/audit-logs` and `GET /api/approvals/pending` endpoints instead, since they already return the same data.
- `Microsoft.OpenApi 2.0.0` (an ASP.NET Core default template dependency) carries a known advisory; it's a dev-time OpenAPI/Swagger generation package, not part of the runtime request path.
