# Brainstorming and Decisions

## Document Purpose

This document records the early product decisions for the Logistics Asset Tracker MVP.

This document is a historical decision record.

Use this document only during brainstorming and requirements refinement.

Claude must not write code during this phase.

---

## Project Idea

Logistics Asset Tracker is a business web application for tracking reusable logistics assets.

Example assets:
- pallets,
- crates,
- boxes,
- containers,
- tools,
- devices,
- warehouse equipment.

The MVP tracks assets through:
- manual movement updates,
- authenticated QR code scanning.

The system should show:
- current asset location,
- current status,
- current condition,
- movement history,
- suspicious activity,
- risk signals.

---

## Problem

Companies often lose visibility over reusable logistics assets.

Common problems:
- asset tracking is often done through spreadsheets or paper notes,
- movement history is incomplete or unreliable,
- damaged or lost assets may be handled without proper review,
- managers lack a clear operational dashboard and audit trail.

The MVP solves this with a traceable software-based tracking system.

---

## Core MVP Decision

The MVP is software-first.

Included:
- asset management,
- location management,
- manual movement updates,
- QR code generation,
- authenticated QR scanning,
- movement history,
- movement rules,
- suspicious activity detection,
- dashboard analytics,
- AI-assisted risk explanation.

Excluded:
- GPS hardware,
- live maps,
- IoT devices,
- mobile app,
- notification system,
- contractor portals,
- billing,
- custom role builder,
- full workflow engine.

---

## Tech Direction

The approved technology direction is React with TypeScript, ASP.NET Core Web API, PostgreSQL, and Entity Framework Core.

---

## GPS Decision

GPS is not part of the MVP.

Reason:
GPS adds hardware, device communication, battery, network, real-time tracking, and map complexity.

The MVP source types are:
- Manual
- QR

GPS may be considered later, but the MVP API must not accept GPS updates.

---

## QR Decision

QR scanning requires login.

There must be no public scan-and-update path.

Reason:
Every movement update must have a reliable `updatedBy` user for movement history and audit logs.

If a user scans a QR code while logged out, the app should redirect to login first.

Manual asset-code fallback is allowed if camera scanning fails.

---

## AI Decision

AI must not control business logic.

Risk detection is rule-based first.

AI may only:
- explain detected risks,
- recommend next actions,
- summarize movement history.

AI must not:
- update database records,
- approve movements,
- override backend validation,
- invent missing data.

Start with a mocked AI service.

Connect a real AI API only after the mocked flow works and is approved.

---

## Status and Condition Decision

Status and condition are separate fields.

Status values:
- Available
- InTransit
- Delivered
- Lost
- Retired

Condition values:
- Good
- Damaged
- NeedsInspection

Delayed is not a stored status.

Delayed is a computed risk or dashboard signal.

---

## Role Decision

The MVP uses three roles:
- Admin
- Manager
- Operator

Detailed permissions are defined in Document 02.

The `assignedToUser` field is informational metadata only and does not control asset visibility.

---

## Movement Direction

The MVP does not use a complex workflow engine.

Movement validation is handled by explicit backend rules defined in Document 05.

---

## Dashboard Direction

The dashboard provides operational visibility for Admin and Manager.

Detailed dashboard behavior is defined in Documents 06 and 07.

---

## Scope Discipline

Do not add:
- GPS implementation,
- real-time maps,
- IoT integration,
- SMS/email notifications,
- mobile app,
- complex enterprise permissions,
- full workflow engine,
- billing,
- contractor portals.

Future ideas should be recorded as future scope, not added to the MVP.

---

## Claude Behavior During Brainstorming

Claude should act as a product and technical reviewer.

Claude should:
- ask only important questions,
- identify contradictions,
- identify missing decisions,
- challenge weak assumptions,
- suggest simpler alternatives,
- keep scope focused.

Claude should not:
- write code,
- create files,
- choose implementation libraries,
- expand scope,
- add enterprise features,
- assume unclear behavior without asking.

Brainstorming is complete when:
- MVP scope is clear,
- rejected scope is confirmed,
- no major product questions remain,
- the project is ready for requirements and architecture planning.