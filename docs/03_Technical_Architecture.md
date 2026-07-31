# Technical Architecture

## Document Purpose

This document defines the technical direction for the Logistics Asset Tracker MVP.

It covers stack, architecture layers, project structure, and key technical boundaries.

It does not define database schema, full API contracts, UI layout, implementation phases, or detailed movement rules.

Detailed behavior is defined in Documents 04, 05, 06, 07, and 08.

---

## Architecture Type

The project is a full-stack web application.

The MVP should be built as a clean business application with:
- React frontend,
- ASP.NET Core Web API backend,
- PostgreSQL relational database,
- JWT authentication,
- role-based authorization,
- service-layer business logic,
- dashboard analytics,
- controlled AI-assisted recommendations.

The application should not be built as a prototype script or a single large controller-based backend.

---

## Approved Tech Stack

Frontend:
- React
- TypeScript

Backend:
- ASP.NET Core Web API

Database:
- PostgreSQL

ORM:
- Entity Framework Core

Authentication:
- JWT-based authentication
- role-based authorization policies

Charts:
- Recharts or another approved simple React chart library

QR:
- stable QR generation library
- browser-based QR scanning library
- manual asset-code fallback when camera scanning fails

AI:
- mocked AI service first
- real AI API only after mocked flow works and is approved

MVP movement source types:
- Manual
- QR

GPS is future scope.

The MVP API must not accept GPS updates.

---

## Backend Architecture

Use a clear layered backend structure.

Recommended layers:
- Controllers
- Services
- DTOs
- Validators
- Entity Framework Core DbContext
- Domain entities
- Configuration
- Authentication/Authorization
- Migrations

Controllers should:
- receive HTTP requests,
- validate basic request shape,
- call services,
- return responses.

Controllers should not contain business logic.

Services should contain business logic.

Important services:
- AuthService
- UserService
- AssetService
- LocationService
- MovementService
- MovementRuleService
- ApprovalService
- DashboardService
- RiskService
- AiRecommendationService
- AuditLogService

Use Entity Framework Core `DbContext` directly in services unless a repository layer clearly improves the structure.

Do not add an unnecessary repository pattern by default.

Use DTOs for requests and responses.

Do not expose database entities directly to the frontend.

---

## Backend Validation

Backend validation is required.

Controllers validate basic request shape.

Services enforce:
- authorization,
- business rules,
- movement rules,
- approval requirements,
- sourceType integrity,
- state-change validation.

Invalid movement attempts must leave the asset unchanged and follow the audit and suspicious-activity behavior defined in Documents 05 and 06.

State-changing operations that update multiple records must run transactionally.

This includes:
- movement completion,
- approval decisions,
- direct Lost reactivation,
- Lost-to-Retired,
- inspection clearance.

---

## Frontend Architecture

Use a feature-based frontend structure.

Suggested folders:
- `src/api`
- `src/auth`
- `src/components`
- `src/pages`
- `src/features/assets`
- `src/features/locations`
- `src/features/movements`
- `src/features/dashboard`
- `src/features/approvals`
- `src/features/risk`
- `src/types`
- `src/utils`

Each feature folder may contain:
- API calls,
- page components,
- forms,
- tables,
- feature-specific types,
- helper functions.

Avoid a random collection of unrelated components.

---

## Authentication and Authorization

Use JWT authentication and backend role-based authorization.

Inactive users must not be allowed to log in.

QR scanning requires authentication.

There must be no public scan-and-update route.

Detailed role permissions are defined in Document 02.

---

## Asset Edit Boundary

Basic asset editing must not change:
- location,
- status,
- condition,
- assetCode,
- qrCodeValue.

Location, status, and condition changes must use the movement flow.

---

## Movement Architecture

The MVP does not use a complex workflow engine.

Movement behavior is enforced through explicit backend service rules.

Every valid completed state-changing movement creates append-only movement history.

At least one of location, status, or condition must actually change.

Notes-only or no-change requests must be rejected.

The backend owns sourceType assignment.

Manual movement flow assigns:
- `sourceType = Manual`

QR movement flow assigns:
- `sourceType = QR`

The client must not choose sourceType.

A movement request that creates pending approval must not create final AssetMovement until approval is accepted.

Movement completion must be atomic across validation, AssetMovement creation, asset state update, suspicious result, and AuditLog writing.

Detailed movement, approval, suspicious, Lost, Retired, and inspection rules are defined in Documents 05 and 06.

---

## QR Architecture

Each asset has a permanent backend-generated QR value.

The QR value must not depend on editable asset fields.

QR scanning opens the authenticated update flow for the matching asset.

If camera scanning fails, manual asset-code fallback is allowed.

Manual fallback uses the normal manual movement flow and sourceType Manual.

---

## Risk and Suspicious Activity

Risk detection and suspicious-activity detection are separate backend responsibilities.

Detailed rules and thresholds are defined in Document 05.

---

## Dashboard Architecture

Dashboard data is calculated from current system data.

Dashboard uses active assets by default.

Live dashboard risk analysis must not automatically create RiskRecommendation records.

Detailed dashboard behavior is defined in Documents 06 and 07.

---

## AI Architecture

Risk detection is rule-based first.

AI only explains detected risks and recommends actions.

Use the mocked AI service first.

AI recommendation processing must follow Document 08.

---

## Suggested Movement Flow

1. Receive request.
2. Authenticate user.
3. Load asset.
4. Check permissions.
5. Determine backend sourceType.
6. Validate active asset and active destination location.
7. Validate that a real state change exists.
8. Validate movement rules and required notes.
9. If invalid, keep the asset unchanged, write and persist AuditLog, and return the error result.
10. If approval is required, create pending approval, keep the asset unchanged, write AuditLog, commit the transaction, and return the pending-approval result.
11. Detect whether the completed movement matches a suspicious rule.
12. Create AssetMovement with the suspicious result.
13. Update asset state.
14. Write AuditLog.
15. Commit the transaction.
16. Return result.

The exact database order may vary, but all records and state changes must be committed atomically.

---

## Architecture Priorities

Prioritize:
- clear project structure,
- service-layer business logic,
- backend validation,
- role-based authorization,
- transactional state changes,
- reliable movement history,
- readable DTOs,
- simple dashboard analytics,
- safe AI integration,
- future GPS compatibility without MVP GPS implementation.

Avoid:
- overengineering,
- complex workflow engine,
- public QR update routes,
- unnecessary repository pattern,
- complex permissions,
- GPS implementation in MVP,
- accepting GPS values in MVP API,
- AI-controlled business logic.