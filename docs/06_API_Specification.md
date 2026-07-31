# API Specification

## Document Purpose

This document defines MVP endpoint paths, access, request fields, response outcomes, and endpoint-specific responsibilities.

Data structure is defined in Document 04.

Movement, approval, suspicious-activity, risk, and validation rules are defined in Document 05.

AI behavior is defined in Document 08.

---

## Standard API Rules

All endpoints require JWT authentication unless marked Public.

Inactive users cannot log in.

QR scanning and QR movement require authentication.

There is no public scan-and-update route.

The backend owns:
- authorization and business validation,
- `sourceType`,
- `assetCode`,
- `qrCodeValue`,
- protected-field enforcement.

List endpoints should support `page`, `pageSize`, `sortBy`, and `sortDirection` where applicable.

Errors use:

~~~json
{
  "code": "INVALID_MOVEMENT",
  "message": "Available cannot move directly to Delivered.",
  "details": {}
}
~~~

Common status codes:
- `200 OK`: successful read or action.
- `201 Created`: resource or movement created.
- `202 Accepted`: approval created; asset unchanged.
- `204 No Content`: successful action without body.
- `400 Bad Request`: malformed request or invalid enum.
- `401 Unauthorized`: missing or invalid JWT.
- `403 Forbidden`: insufficient permission.
- `404 Not Found`: resource not found.
- `409 Conflict`: state, stale approval, duplicate approval, inactive-resource, or location-in-use conflict.
- `422 Unprocessable Entity`: business-rule failure.

---

## Authentication Endpoints

## POST /api/auth/login

Access:
Public.

Request:
- email,
- password.

Rules:
- email lookup is case-insensitive,
- inactive users cannot log in,
- `passwordHash` is never returned.

Response:
JWT token and user summary.

## GET /api/auth/me

Access:
Authenticated users.

Returns the current user.

## POST /api/auth/logout

Access:
Authenticated users.

MVP behavior:
Client removes the JWT.

Server-side revocation is future scope.

---

## User Endpoints

## GET /api/users

Access:
Admin.

Supports:
- search,
- role,
- isActive,
- pagination,
- sorting.

## POST /api/users

Access:
Admin.

Request:
- fullName,
- email,
- password,
- role,
- isActive optional, default true.

Rules:
- email is unique case-insensitively,
- backend hashes the password,
- `passwordHash` is not accepted or returned.

## PUT /api/users/{id}

Access:
Admin.

Allowed fields:
- fullName,
- email,
- role,
- isActive.

Rules:
- email uniqueness remains case-insensitive,
- `passwordHash` is not accepted or returned.

---

## Location Endpoints

## GET /api/locations

Access:
Authenticated users.

Rules:
- Operators see active locations only.
- Admin and Manager may view active and inactive locations.

Supports:
- type,
- isActive,
- search,
- pagination,
- sorting.

## POST /api/locations

Access:
Admin.

Request:
- name,
- type,
- address optional,
- description optional.

## PUT /api/locations/{id}

Access:
Admin.

Allowed fields:
- name,
- type,
- address,
- description.

## PATCH /api/locations/{id}/deactivate

Access:
Admin.

Rules:
- preserve historical records,
- inactive locations cannot be destinations,
- reject while active assets reference the location.

Conflict:
- `409 Conflict`
- code: `LOCATION_IN_USE`

---

## Asset Endpoints

## GET /api/assets

Access:
Authenticated users.

Rules:
- Operators see active assets only.
- Admin and Manager may view inactive assets.

Supports:
- status,
- condition,
- locationId,
- type,
- search,
- isActive,
- pagination,
- sorting.

## GET /api/assets/{id}

Access:
Authenticated users.

Rules:
- Operators access active assets only.
- Admin and Manager may access inactive assets.
- Inactive assets cannot be updated.

## GET /api/assets/by-code/{assetCode}

Access:
Authenticated users.

Purpose:
Manual asset-code fallback.

Rules:
- lookup is case-insensitive,
- invalid code returns `404`,
- Operators cannot access inactive assets,
- updates use the manual movement endpoint.

## GET /api/assets/qr/{qrCodeValue}

Access:
Authenticated users.

Purpose:
Return QR movement context.

Rules:
- invalid token returns `404`,
- inactive assets cannot be updated,
- updates use the QR movement endpoint.

## POST /api/assets

Asset creation rules:
- Asset creation is not a movement update.
- Asset creation does not create AssetMovement.
- Initial status must be one of the approved AssetStatus values except Retired.
- Retired assets must not be created directly in the MVP.
- If initial status is Available, initial condition must be Good.
- Delayed is not accepted as a stored status.
- Other valid status and condition combinations may be created by Admin unless explicitly blocked by enum validation.

Access:
Admin.

Request:
- name,
- type,
- currentLocationId,
- status,
- condition,
- assignedToUserId optional.

Rules:
- status and condition follow Document 05,
- `Delayed` is not accepted as status,
- backend generates `assetCode` and `qrCodeValue`,
- client must not provide them,
- create Asset and AuditLog,
- do not create AssetMovement.

## PUT /api/assets/{id}

Access:
Admin.

Allowed fields:
- name,
- type,
- assignedToUserId.

Must not change:
- currentLocationId,
- status,
- condition,
- assetCode,
- qrCodeValue.

## PATCH /api/assets/{id}/deactivate

Access:
Admin.

Rules:
- preserve history,
- deactivated assets cannot receive movements.

## GET /api/assets/{id}/qr

Access:
Admin and Manager.

Rules:
- use permanent `qrCodeValue`,
- QR data must not depend on editable fields.

## POST /api/assets/{id}/reactivate

Access:
Manager and Admin.

Purpose:
Direct Lost-to-Available reactivation.

Request:
- toLocationId optional,
- decisionNote required.

Rules:
- asset must be active, Lost, and Good,
- reject if a Pending Lost-reactivation approval exists,
- omitted `toLocationId` keeps current location,
- provided destination must be active,
- create AssetMovement with `sourceType = Manual`,
- set `updatedByUserId` to the authenticated Manager/Admin,
- store decisionNote in movement notes and AuditLog,
- update status to Available,
- commit atomically.

Pending-approval conflict:
- `409 Conflict`
- code: `PENDING_APPROVAL_EXISTS`

---

## Movement Endpoints

Omitted movement fields keep their current values.

At least one resulting location, status, or condition must change.

Completed movements use the authenticated submitter as `updatedByUserId`.

## GET /api/assets/{id}/movements

Operator response rule:
- Operators may view movement history for active assets.
- Operator responses must not expose `isSuspicious` or `suspiciousReason`.
- Operators must not use `isSuspicious` as a filter.
- Admin and Manager may view suspicious indicators and suspicious reasons.

Access:
- Admin and Manager: all assets.
- Operator: active assets only.

Supports:
- sourceType,
- isSuspicious,
- fromDate,
- toDate,
- updatedByUserId,
- pagination,
- sorting.

## POST /api/assets/{id}/movements/manual

Access:
Admin, Manager, and Operator according to Document 02.

Request:
- toLocationId optional,
- newStatus optional,
- newCondition optional,
- notes optional for normal movement.

Backend sets:
- `sourceType = Manual`.

Rules:
- follow Document 05,
- notes are required for inspection clearance, Lost-to-Retired, and Operator Lost-to-Available,
- Manager/Admin Lost-to-Available must use `/api/assets/{id}/reactivate`,
- reject Manager/Admin direct reactivation through this endpoint.

Valid Operator Lost-to-Available request:
- create MovementApproval,
- store notes as `requestReason`,
- set `requestedSourceType = Manual`,
- set requested status and condition to Available and Good,
- store optional destination as `requestedLocationId`,
- keep asset unchanged,
- do not create AssetMovement,
- return `202 Accepted`.

## POST /api/assets/qr/{qrCodeValue}/movements

Access:
Admin, Manager, and Operator according to Document 02.

Request:
- toLocationId optional,
- newStatus optional,
- newCondition optional,
- notes optional for normal movement.

Backend sets:
- `sourceType = QR`.

Rules:
- follow Document 05,
- QR cannot perform inspection clearance or Lost-to-Retired,
- Manager/Admin direct reactivation must use `/api/assets/{id}/reactivate`,
- reject Manager/Admin Lost-to-Available through this endpoint,
- client cannot submit `sourceType`,
- GPS is not accepted.

Valid Operator Lost-to-Available request:
- create MovementApproval,
- store notes as `requestReason`,
- set `requestedSourceType = QR`,
- set requested status and condition to Available and Good,
- store optional destination as `requestedLocationId`,
- keep asset unchanged,
- do not create AssetMovement,
- return `202 Accepted`.

## Movement Outcomes

Completed:
- `201 Created`
- `resultType = MovementCreated`
- include movement and updated asset.

Approval required:
- `202 Accepted`
- `resultType = ApprovalRequired`
- include approval,
- `assetUnchanged = true`.

No state change:
- `422 Unprocessable Entity`
- code: `NO_STATE_CHANGE`

Duplicate Pending approval:
- `409 Conflict`
- code: `PENDING_APPROVAL_EXISTS`

Invalid movement:
- do not update Asset,
- do not create AssetMovement,
- write AuditLog,
- mark suspicious only when Document 05 matches.

---

## Approval Endpoints

## GET /api/approvals

Access:
Manager and Admin.

Supports:
- status,
- assetId,
- requestedByUserId,
- fromDate,
- toDate,
- pagination,
- sorting.

## GET /api/approvals/pending

Access:
Manager and Admin.

Equivalent to:
- `GET /api/approvals?status=Pending`

## POST /api/approvals/{id}/approve

Access:
Manager and Admin.

Request:
- decisionNote required.

Rules:
- approval must be Pending,
- asset must remain active, Lost, and Good,
- requested status and condition must remain Available and Good,
- requested destination must still be active,
- null destination keeps current location,
- create AssetMovement using `requestedSourceType`,
- set `updatedByUserId` to the deciding Manager/Admin,
- preserve requester and requestReason,
- store decisionNote on MovementApproval and AuditLog,
- update asset and approval decision fields,
- commit atomically.

Stale or invalid:
- `409 Conflict`
- code: `STALE_APPROVAL`

## POST /api/approvals/{id}/reject

Access:
Manager and Admin.

Request:
- decisionNote required.

Rules:
- approval must be Pending,
- keep asset unchanged,
- set Rejected decision fields,
- store decisionNote on MovementApproval and AuditLog,
- commit atomically.

Stale or decided:
- `409 Conflict`
- code: `STALE_APPROVAL`

---

## Dashboard Endpoints

Access:
Manager and Admin.

Dashboard calculations use active assets by default.

## GET /api/dashboard/summary

Response:
- totalActiveAssets,
- availableAssets,
- inTransitAssets,
- damagedAssets,
- lostAssets,
- riskyAssets,
- suspiciousActivityCount,
- pendingApprovals.

Rules:
- `damagedAssets` counts active assets with condition Damaged,
- `riskyAssets` counts distinct active assets with at least one current rule-based risk,
- `suspiciousActivityCount` counts normalized suspicious completed movements and blocked attempts,
- stored RiskRecommendation records do not control live counts.

## GET /api/dashboard/assets-by-status

Returns active asset counts by status.

## GET /api/dashboard/assets-by-condition

Returns active asset counts by condition.

## GET /api/dashboard/assets-by-location

Returns active asset counts by location.

## GET /api/dashboard/recent-movements

Supports pagination.

## GET /api/dashboard/suspicious-activity

Returns normalized suspicious completed movements and blocked attempts.

Supports:
- assetId,
- userId,
- eventType,
- fromDate,
- toDate,
- pagination,
- sorting.

## GET /api/dashboard/pending-approvals

Returns Pending approval summary.

---

## Risk Recommendation Endpoints

## POST /api/assets/{id}/risk/analyze

Access:
Manager and Admin.

Rules:
- follow Documents 05 and 08,
- create no record when no risk exists,
- create one RiskRecommendation per detected risk type,
- use mocked AI first,
- validate AI output,
- use `RuleEngineWithAI` for valid AI output,
- use `RuleEngine` for fallback,
- do not fail only because AI is unavailable,
- write AuditLog when records are generated.

## GET /api/assets/{id}/risk-recommendations

Access:
Manager and Admin.

Supports:
- riskLevel,
- riskType,
- fromDate,
- toDate,
- pagination,
- sorting.

## GET /api/risk-recommendations

Access:
Manager and Admin.

Supports:
- assetId,
- riskLevel,
- riskType,
- generationSource,
- generatedByUserId,
- fromDate,
- toDate,
- pagination,
- sorting.

---

## Audit Log Endpoint

## GET /api/audit-logs

Access:
Admin and Manager.

Supports:
- userId,
- entityType,
- entityId,
- action,
- fromDate,
- toDate,
- pagination,
- sorting.

Rules:
- include important completed actions and blocked attempts,
- never expose passwords, hashes, JWTs, API keys, or secrets.

Detailed audit actions are defined in Document 05.