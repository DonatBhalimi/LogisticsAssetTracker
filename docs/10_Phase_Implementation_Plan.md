# Phase Implementation Plan

## Document Purpose

This document defines the implementation phases for building the Logistics Asset Tracker MVP.

It does not redefine requirements, movement rules, API behavior, database schema, UI behavior, or AI logic.

Implementation must follow Documents 02 through 09 as applicable.

---

## Main Rule

Do not build the whole project in one Claude Code session.

Use phases.

Start a new Claude Code session, or clear context, between major phases.

Each phase should have:
- clear scope,
- specific files to read,
- limited implementation goals,
- working deliverable before moving forward.

---

## Phase 0: Gap Review Before Coding

## Goal

Confirm that the project is ready for implementation.

## Claude Should Read

- 01 Project Requirements
- 02 Functional Requirements
- 03 Technical Architecture
- 04 Data Model
- 05 Movement Rules
- 06 API Specification

## Tasks

- Review approved requirements.
- Identify contradictions only if they are blocking.
- Confirm MVP scope.
- Confirm architecture direction.
- Confirm database entities.
- Confirm movement and approval behavior.
- Confirm API boundaries.
- Do not add new features.

## Deliverable

Implementation-ready documentation package.

---

## Phase 1: Project Setup and Architecture

## Goal

Create the full-stack project structure and finalize required library decisions.

## Claude Should Read

- 03 Technical Architecture
- 06 API Specification
- 09 Research and Library Decisions

## Tasks

- Create ASP.NET Core Web API project.
- Create React + TypeScript frontend project.
- Configure PostgreSQL connection.
- Add Entity Framework Core.
- Create backend folder structure.
- Create frontend feature-based folder structure.
- Add initial README.
- Add basic environment configuration.
- Research and record the selected QR generation library.
- Research and record the selected QR scanning library.
- Do not begin QR implementation while QR library decisions remain TBD.

## Deliverable

Frontend and backend both run locally, and QR library decisions are recorded.

---

## Phase 2: Core Entities, Authentication, and Basic UI

## Goal

Build the foundation of the system.

## Claude Should Read

- 02 Functional Requirements
- 04 Data Model
- 06 API Specification
- 07 UI/UX Specification

## Tasks

- Implement User model.
- Implement authentication and JWT login.
- Implement role-based authorization.
- Implement Admin user-management endpoints.
- Implement Location model and endpoints.
- Implement Asset model and endpoints.
- Implement AuditLog model and basic audit logging.
- Generate backend asset codes.
- Generate backend QR tokens.
- Add seed/demo data.
- Implement login page.
- Implement Users page.
- Implement Locations page.
- Implement Assets list, details, create, and edit pages.

## Deliverable

User can log in, Admin can manage users, locations, and assets, and core asset pages work.

---

## Phase 3: Basic Movement Tracking and QR Flow

Phase 3 movement scope:

Until Phase 4, allow only location-only movement updates where status and condition remain unchanged.

Reject status changes, condition changes, Lost reactivation, inspection clearance, Lost-to-Retired, and any other sensitive or unsupported transition.

Full status and condition movement rules are implemented in Phase 4.

## Goal

Build the tracking foundation for basic permitted movement updates.

## Claude Should Read

- 04 Data Model
- 05 Movement Rules
- 06 API Specification
- 07 UI/UX Specification
- 09 Research and Library Decisions

## Tasks

- Implement AssetMovement model.
- Implement manual movement endpoint.
- Implement QR movement endpoint.
- Implement QR code generation using the selected library.
- Implement QR scanner page using the selected library.
- Implement manual asset-code fallback.
- Create movement history records.
- Display movement history on asset details.
- Enforce authentication and permissions.
- Enforce active asset and active location checks.
- Enforce no-change rejection.
- Enforce backend-assigned sourceType.

## Deliverable

Manual and QR movement flows work for basic permitted, non-sensitive transitions and create movement history.

---

## Phase 4: Movement Rules, Suspicious Activity, Approvals, and Audit Review

## Goal

Add full business validation and review workflows.

## Claude Should Read

- 04 Data Model
- 05 Movement Rules
- 06 API Specification
- 07 UI/UX Specification

## Tasks

- Enforce full backend movement validation.
- Enforce status and condition transition rules.
- Enforce inactive and Retired asset restrictions.
- Enforce inspection-clearance behavior.
- Implement Lost-to-Available approval request flow.
- Implement Manager/Admin approve and reject flow.
- Implement direct Manager/Admin Lost reactivation.
- Implement Admin Lost-to-Retired behavior.
- Log invalid movement attempts.
- Mark suspicious completed movements and suspicious blocked attempts.
- Build Approvals page.
- Build Suspicious Activity page.
- Build Audit Logs page.

## Deliverable

Backend enforces approved movement rules, approvals work, suspicious activity is visible, and audit logs can be reviewed.

---

## Phase 5: Dashboard and AI Risk Recommendations

## Goal

Add operational visibility and AI-assisted risk explanation.

## Claude Should Read

- 04 Data Model
- 05 Movement Rules
- 06 API Specification
- 07 UI/UX Specification
- 08 AI Risk Recommendation Specification

## Tasks

- Build dashboard summary cards.
- Build charts for assets by status, condition, and location.
- Build recent movements section.
- Build suspicious activity section.
- Build pending approvals section.
- Implement rule-based risk detection.
- Implement mocked AI recommendation service.
- Validate AI output.
- Save RiskRecommendation records.
- Display recommendations in UI.

## Deliverable

Dashboard shows useful operational visibility and AI explains detected risks.

---

## Phase 6: Testing, Polish, README, and Demo

## Goal

Make the MVP presentable.

## Claude Should Read

- 02 Functional Requirements
- 05 Movement Rules
- 06 API Specification
- 07 UI/UX Specification
- 08 AI Risk Recommendation Specification

## Tasks

- Test main user flows.
- Test role-based access.
- Test movement validation.
- Test approval flow.
- Test QR flow.
- Test AI risk recommendation flow.
- Fix bugs.
- Improve error messages.
- Add loading and empty states.
- Clean UI spacing.
- Update README.
- Add screenshots.
- Prepare demo script.
- Record demo video if needed.

## Deliverable

Demo-ready MVP.

---

## Claude Code Working Rule

At the start of each phase, Claude should:

1. Read only the documents needed for that phase.
2. Summarize the phase goal.
3. List the files it plans to create or edit.
4. Implement in small steps.
5. Stop after the phase deliverable works.

Claude must not:
- jump ahead to later phases,
- add unapproved features,
- change business rules,
- invent new endpoints,
- ignore source-of-truth documents,
- implement GPS in the MVP,
- let AI control business logic.