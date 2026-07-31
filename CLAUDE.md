# CLAUDE.md

## Project

Build the Logistics Asset Tracker MVP.

This is a full-stack internal business dashboard for tracking reusable logistics assets such as pallets, crates, boxes, containers, tools, devices, and warehouse equipment.

The project demonstrates:
- React + TypeScript frontend,
- ASP.NET Core Web API backend,
- PostgreSQL database,
- Entity Framework Core,
- QR/manual asset tracking,
- movement history,
- audit logs,
- dashboard analytics,
- rule-based risk detection,
- AI-assisted risk explanation.

---

## Core Stack

Frontend:
- React
- TypeScript

Backend:
- ASP.NET Core Web API

Database:
- PostgreSQL

ORM:
- Entity Framework Core

Charts:
- Recharts or approved simple chart library

AI:
- mocked AI service first
- real AI API only after mocked flow works and is approved

MVP source types:
- Manual
- QR

GPS is future scope. Do not implement or accept GPS in the MVP.

---

## Source of Truth

Do not load every document automatically.

Read only the documents required for the current phase.

Use:
- `docs/01_Project_Requirements.md` for product scope, target users, MVP goals, out-of-scope items, and success criteria.
- `docs/02_Functional_Requirements.md` for behavior and permissions.
- `docs/03_Technical_Architecture.md` for architecture.
- `docs/04_Data_Model.md` for entities, fields, enums, and relationships.
- `docs/05_Movement_Rules_and_Suspicious_Activity.md` for movement rules, suspicious activity, and risk thresholds.
- `docs/06_API_Specification.md` for API behavior.
- `docs/07_UI_UX_Specification.md` for UI screens and boundaries.
- `docs/08_AI_Risk_Recommendation_Specification.md` for AI behavior.
- `docs/09_Research_and_Library_Decisions.md` for library decisions.
- `docs/10_Phase_Implementation_Plan.md` for phase scope.
- `docs/11_Testing_and_Acceptance_Criteria.md` for acceptance criteria.
- `docs/12_Claude_Code_Prompts.md` for phase prompts.

If documents conflict, stop and report the contradiction.

Do not invent a resolution.

---

## Working Rules

Work phase by phase.

Do not build the whole project in one session.

At the start of each phase:
1. Read only the listed documents.
2. Summarize the phase goal.
3. List files you plan to create or edit.
4. Wait for approval before major file changes.
5. Implement in small steps.
6. Run build/tests when possible.
7. Report changed files, test results, and remaining gaps.

Do not:
- jump ahead to later phases,
- add unapproved features,
- change business rules,
- invent endpoints,
- implement GPS,
- let AI control business logic,
- hide backend validation errors,
- store secrets in code,
- present unfinished features as complete.

---

## High-Risk Project Rules

Backend validation is required.

Frontend validation is not trusted.

Status and condition are separate fields.

Movement-controlled fields must change only through the movement update flow.

Asset edit must not change:
- location,
- status,
- condition,
- assetCode,
- qrCodeValue.

Every valid completed state-changing movement creates `AssetMovement`.

Invalid movement attempts:
- do not create `AssetMovement`,
- do not modify `Asset`,
- create `AuditLog`.

`sourceType` is assigned by the backend.

Users must not choose `sourceType`.

QR scanning requires login.

There must be no public scan-and-update route.

Retired assets cannot receive normal movement updates.

Inactive assets cannot receive movement updates.

Inactive locations cannot be movement destinations.

Lost, inspection-clearance, suspicious activity, approval, and risk behavior must follow Documents 05 and 06 exactly.

---

## AI Rules

Risk detection is rule-based first.

AI only explains detected risks and suggests next steps.

AI must not:
- detect risks independently,
- approve movements,
- update database records directly,
- override backend validation,
- invent missing data.

Use mocked AI first.

If AI fails, return rule-based fallback behavior.

---

## UI Rules

UI must follow Document 07.

UI must:
- show backend errors clearly,
- use role-based navigation/actions,
- use readable text badges,
- disable invalid actions where possible.

UI must not:
- define independent business rules,
- allow movement changes from Edit Asset,
- expose public QR update routes,
- imply AI controls approvals or decisions.

---

## Testing Rule

Before treating a phase as complete:
- run available build/test commands,
- verify Document 11 acceptance criteria,
- report what passed,
- report what failed,
- list known limitations honestly.

---

## Output Style

Be direct and implementation-focused.

Prefer small, safe changes.

Explain changed files clearly.

When uncertain, stop and ask.

Do not expand documentation unless explicitly asked.
