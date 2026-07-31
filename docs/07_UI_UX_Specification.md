# UI/UX Specification

## Document Purpose

This document defines the main UI screens and design direction for the Logistics Asset Tracker MVP.

It does not redefine business rules, permissions, API behavior, database structure, movement validation, approval rules, risk logic, or AI behavior.

Business behavior must follow Documents 02, 04, 05, and 06.

---

## Design Direction

The application should look like a clean internal business dashboard.

Use:
- readable tables,
- compact dashboard cards,
- simple forms,
- clear filters,
- text-based status badges,
- visible backend error messages.

Avoid:
- unnecessary animation,
- cluttered screens,
- vague labels,
- oversized controls,
- frontend-only business logic.

The UI should work on desktop and mobile browsers, especially for QR scanning and movement updates.

---

## Global UI Rules

- Navigation and actions must be role-based.
- Backend validation errors must be shown clearly.
- Data pages must support loading, empty, success, and error states.
- Inactive or Retired assets must not show enabled movement actions.
- Badges must include text and must not rely only on color.
- Delayed, Risky, and Suspicious are UI signals, not stored AssetStatus values.
- Users must not choose `sourceType`.
- Authenticated users must have a logout action.

---

## Navigation

Admin:
- Dashboard
- Assets
- Locations
- Users
- Approvals
- Suspicious Activity
- Risk Recommendations
- Audit Logs

Manager:
- Dashboard
- Assets
- Approvals
- Suspicious Activity
- Risk Recommendations
- Audit Logs

Operator:
- Assets
- QR Scanner
- Movement History for active assets

---

## Main Screens

### 1. Login

Contains:
- email,
- password,
- login button,
- error message area.

Show backend login errors clearly.

---

### 2. Dashboard

Access:
Admin and Manager.

Purpose:
Show operational overview.

Include:
- cards for active assets, available, in transit, damaged, lost, risky assets, suspicious activity, and pending approvals,
- charts for assets by status, condition, and location,
- recent movements,
- suspicious activity,
- pending approvals.

Dashboard must not create RiskRecommendation records.

---

### 3. Assets List

Purpose:
Search, filter, and open assets.

Include:
- search,
- status filter,
- condition filter,
- location filter,
- type filter,
- active/inactive filter where permitted,
- pagination,
- asset table.

Table columns:
- Asset Code
- Name
- Type
- Current Location
- Status
- Condition
- Last Updated
- Actions

Actions depend on role and asset state:
- View
- Update Movement
- Edit Basic Information
- View or Generate QR
- Generate Risk Recommendation
- Deactivate Asset

Rules:
- Admin may create, edit, and deactivate assets.
- Admin and Manager may view or generate QR codes.
- Admin and Manager may generate risk recommendations.
- Movement actions are shown only when permitted.
- Operator cannot view inactive assets.

---

### 4. Asset Details

Purpose:
Show full asset information.

Include:
- asset details,
- current status and condition,
- assigned user,
- movement history,
- risk recommendations for Admin and Manager,
- suspicious warning when applicable,
- QR information for Admin and Manager.

Possible actions:
- Update Movement
- Edit Basic Information
- View QR
- Generate Risk Recommendation
- Reactivate Lost Asset
- Deactivate Asset

Rules:
- Edit Basic Information and Deactivate Asset are Admin-only.
- View QR and Generate Risk Recommendation are available to Admin and Manager only.
- Direct Lost reactivation is available to Manager and Admin only when permitted by Documents 05 and 06.
- Operators request Lost reactivation through the normal movement form.
- Asset details must not directly edit location, status, condition, assetCode, or qrCodeValue.

---

### 5. Create/Edit Asset

Create Asset:
- name,
- type,
- initial location,
- initial status,
- initial condition,
- assigned user optional.

Edit Asset:
- name,
- type,
- assigned user optional.

Edit Asset must not expose movement-controlled fields.

---

### 6. QR Scanner

Purpose:
Scan an asset QR code.

Include:
- browser camera scanner,
- manual asset-code fallback,
- scan result area,
- error message area.

Rules:
- QR scanning requires login.
- Successful scan opens the QR movement flow.
- Manual asset-code fallback opens the manual movement flow.
- Inactive assets must not allow movement submission.

---

### 7. Movement Update

Purpose:
Submit permitted location, status, or condition changes.

Include:
- current asset summary,
- new location,
- new status,
- new condition,
- notes,
- submit button.

Possible outcomes:
- movement completed,
- approval request created,
- validation error,
- permission error,
- inactive asset error,
- Retired asset error.

Rules:
- The form must follow Documents 05 and 06.
- Users must not choose `sourceType`.
- Backend errors must be displayed without replacing them with independent frontend rules.
- Manager/Admin direct Lost reactivation must use the dedicated Reactivate Lost Asset action.
- QR movement must not expose inspection clearance or Lost-to-Retired actions.

---

### 8. Movement History

Purpose:
Show movement traceability.

Columns:
- Previous Location
- New Location
- Previous Status
- New Status
- Previous Condition
- New Condition
- Source Type
- Updated By
- Timestamp
- Notes
- Suspicious Indicator

Suspicious Indicator visibility:
- Admin and Manager can see suspicious indicators.
- Operators must not see suspicious indicators or suspicious reasons.

Movement history is read-only.

---

### 9. Approvals

Access:
Admin and Manager.

Purpose:
Review Lost-reactivation requests.

Include:
- status filter,
- asset,
- requested change,
- requested by,
- requested source type,
- request reason,
- decision note,
- approve and reject actions.

Rules:
- decision note is required for approval and rejection,
- Pending records may be decided,
- Approved and Rejected records are read-only.

---

### 10. Suspicious Activity

Access:
Admin and Manager.

Purpose:
Review suspicious completed movements and blocked attempts.

Include:
- date filter,
- asset filter,
- user filter,
- event type filter,
- suspicious activity table.

Completed movements and blocked attempts must be clearly distinguished.

---

### 11. Risk Recommendations

Access:
Admin and Manager.

Purpose:
Review generated recommendations.

Include:
- asset filter,
- risk level filter,
- risk type filter,
- asset,
- explanation,
- recommendation,
- generation source,
- generated date.

AI-assisted output must not appear to approve movements or override business rules.

---

### 12. Locations

Access:
Admin.

Purpose:
Manage locations.

Include:
- locations table,
- search,
- type filter,
- active/inactive filter,
- create location action,
- edit action,
- deactivate action.

Inactive locations must not appear as new movement destinations.

---

### 13. Users

Access:
Admin.

Purpose:
Manage users.

Include:
- users table,
- search,
- role filter,
- active/inactive filter,
- create user action,
- edit action,
- activate action,
- deactivate action.

Never display passwords, password hashes, JWT tokens, API keys, or secrets.

---

### 14. Audit Logs

Access:
Admin and Manager.

Purpose:
Review completed actions and blocked attempts.

Include:
- date filter,
- user filter,
- action filter,
- entity filter,
- read-only audit table.

Never display passwords, hashes, tokens, API keys, or secrets.

---

## Badges

Status:
- Available
- InTransit
- Delivered
- Lost
- Retired

Condition:
- Good
- Damaged
- NeedsInspection

Signals:
- Delayed
- Risky
- Suspicious

---

## UI Boundaries

The UI must not:
- define independent movement rules,
- override backend validation,
- hide backend errors,
- expose public QR update routes,
- allow movement changes from Edit Asset,
- allow users to choose `sourceType`,
- route Manager/Admin direct Lost reactivation through the normal movement form,
- imply that AI controls approvals or business decisions.