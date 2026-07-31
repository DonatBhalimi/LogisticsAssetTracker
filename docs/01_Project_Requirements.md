# Project Requirements Document: Logistics Asset Tracker

## Document Purpose

This document defines the product scope, target users, MVP goals, core features, out-of-scope items, and success criteria for Logistics Asset Tracker.

It does not define technical architecture, database schema, API contracts, UI layout, implementation phases, or detailed movement rules.

Detailed behavior is defined in later documents.

If this document conflicts with Documents 02 through 12, the later approved documents take precedence.

---

## Product Summary

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

The product should help users understand:
- where an asset is,
- its current status,
- its current condition,
- who updated it,
- when it changed,
- whether it has risk or suspicious activity.

Status and condition are separate fields.

Delayed is not a stored status. It is a computed risk signal.

---

## Target Users

## Admin

Admin manages:
- users,
- locations,
- assets,
- permitted movement actions.

## Manager

Manager:
- reviews dashboard analytics,
- reviews suspicious activity,
- reviews risk recommendations,
- reviews approval requests,
- performs permitted higher-trust movement actions.

## Operator

Operator:
- views active assets,
- updates permitted asset movement,
- scans QR codes,
- reports damage.

The `assignedToUser` field is informational metadata only.

It does not control asset visibility in the MVP.

Detailed permissions are defined in Document 02.

---

## MVP Goal

Build a working MVP that allows users to:

- manage assets,
- manage locations,
- update asset movement,
- scan QR codes after login,
- store movement history,
- enforce backend movement rules,
- review suspicious activity,
- view dashboard analytics,
- generate AI-assisted risk explanations.

The MVP should be stable, demo-ready, and focused.

---

## Core MVP Features

## Asset Registry

The system must support creating, viewing, editing basic information, filtering, and deactivating assets according to user permissions.

Asset edit must not bypass the movement update flow.

## Location Management

The system must support creating, editing, listing, and deactivating locations.

Inactive locations remain available for historical records but cannot be selected for new movement updates.

## Manual Tracking

Permitted users can update asset location, status, condition, and notes through the manual movement flow.

## QR Tracking

Each asset has a permanent QR code.

QR scanning requires login.

There must be no public scan-and-update path.

Manual asset-code fallback is allowed when camera scanning fails.

## Movement History

Every valid completed state-changing movement creates an append-only movement history record.

Movement history supports traceability by recording what changed, who changed it, when it changed, and how it was submitted.

## Movement Rules

The backend must enforce movement rules.

Frontend validation may improve user experience but must not be trusted as the source of truth.

Detailed movement rules are defined in Document 05.

## Suspicious Activity

The system must detect and display suspicious activity for Manager/Admin review.

Suspicious activity may include:
- suspicious completed movements,
- suspicious blocked attempts.

Detailed suspicious-activity rules are defined in Document 05.

## Approval Flow

Operator Lost-to-Available requests require approval.

The asset must remain unchanged while approval is pending.

Manager/Admin direct reactivation is allowed only under the conditions defined in Documents 05 and 06.

## Dashboard Analytics

The dashboard provides operational visibility for Admin and Manager.

It should show asset status, condition, location, recent movement, suspicious activity, risk, and pending approval information.

Detailed dashboard behavior is defined in Documents 06 and 07.

## AI Risk Recommendations

Risk detection is rule-based first.

AI may explain detected risks and recommend next actions.

AI must not:
- approve movements,
- update records directly,
- override backend validation,
- invent missing data.

Detailed AI behavior is defined in Document 08.

---

## Out of Scope

The MVP will not include:

- GPS hardware integration,
- live map tracking,
- native mobile app,
- SMS notifications,
- email notification system,
- complex enterprise permissions,
- custom role builder,
- assignment-based asset visibility,
- billing,
- IoT device integration,
- full workflow engine.

Future ideas should not be added to the MVP unless explicitly approved.

---

## Success Criteria

The MVP is successful if:

- Admin can manage users, locations, and assets.
- Users can log in and access features according to role.
- Operators can view active assets.
- QR scanning requires login.
- Completed manual and QR state-changing movements create movement history.
- Backend validation blocks invalid movement behavior.
- Suspicious activity is visible to Manager/Admin.
- Lost reactivation approval flow works.
- Dashboard shows useful operational data.
- Rule-based risk detection works.
- AI-assisted recommendations explain detected risks.
- AI failure does not break the app.
- README and demo clearly explain scope and main flows.

---

## Product Quality Expectations

The MVP should be:

- clear,
- traceable,
- role-aware,
- backend-validated,
- demo-ready,
- honest about limitations.

No unfinished or fake features should be presented as complete.