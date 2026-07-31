# Movement Rules and Suspicious Activity

## Document Purpose

This document defines movement validation, approval behavior, risk detection, suspicious activity detection, thresholds, and validation order for the Logistics Asset Tracker MVP.

This document owns:
- movement rules,
- Lost reactivation rules,
- inspection-clearance rules,
- suspicious-activity rules,
- risk thresholds,
- risk-level mapping,
- backend validation order.

Data model details are defined in Document 04.

API behavior is defined in Document 06.

UI behavior is defined in Document 07.

---

## Core Concepts

Status represents workflow state.

Allowed status values:
- Available
- InTransit
- Delivered
- Lost
- Retired

Condition represents physical state.

Allowed condition values:
- Good
- Damaged
- NeedsInspection

Status and condition are separate fields.

Delayed is not a stored status.

Delayed is a computed risk or dashboard signal.

Retired is terminal in the MVP.

Retired assets cannot receive normal movement updates.

RepeatedDamage is not an MVP risk type unless a future rule and threshold are explicitly approved.

---

## Backend Enforcement

Movement rules must be enforced in the backend.

Frontend validation may improve user experience but is not trusted.

Invalid movement attempts must:
- leave the asset unchanged,
- not create AssetMovement,
- create AuditLog,
- return a clear error,
- be marked suspicious only when an explicit suspicious rule matches.

State-changing movement operations must be transactional when multiple records are updated.

This includes:
- movement completion,
- approval decisions,
- direct Lost reactivation,
- Lost-to-Retired,
- inspection clearance.

---

## Movement Creation Rules

Every valid completed state-changing movement must create AssetMovement.

A movement is state-changing only if at least one changes:
- location,
- status,
- condition.

Notes-only submissions must be rejected.

No-change submissions must be rejected.

No-change check-ins are outside the MVP.

Asset creation creates Asset and AuditLog, but does not create AssetMovement.

Movement history begins with the first post-creation movement update.

Invalid movement attempts must not create AssetMovement.

---

## SourceType Rules

MVP sourceType values:
- Manual
- QR

The backend assigns sourceType.

The frontend must not choose sourceType.

Manual endpoint creates completed movements with:
- sourceType = Manual

QR endpoint creates completed movements with:
- sourceType = QR

A QR request that creates pending approval must not create final AssetMovement until approval is accepted.

A manual request that creates pending approval must not create final AssetMovement until approval is accepted.

Manual asset-code fallback uses the manual movement flow and sourceType Manual.

GPS is future scope.

The MVP API must not accept GPS updates.

---

## Active Resource Rules

Inactive assets cannot receive movement updates.

Inactive locations cannot be selected as new movement destinations.

Retired assets cannot receive normal movement updates.

Blocked attempts must create AuditLog.

---

## Status and Condition Rules

Available cannot move directly to Delivered.

Valid intended flow:
- Available -> InTransit -> Delivered

Invalid flow:
- Available -> Delivered

A movement that changes status to Available requires the resulting condition to be Good.

Invalid status-change examples:
- changing status to Available with condition Damaged,
- changing status to Available with condition NeedsInspection.

A condition-only damage report may set condition to:
- Damaged,
- NeedsInspection.

A condition-only damage report may keep the current status unchanged, including when the current status is Available.

This condition-only damage behavior is not treated as a transition into Available.

The InTransit and Delivered restriction is evaluated against the asset condition before the request.

If the asset is already Damaged or NeedsInspection before the request, it cannot transition to:
- InTransit,
- Delivered.

A single movement may change status from Available to InTransit while also setting condition to Damaged or NeedsInspection, provided the asset condition was Good before the request.

After that movement, the asset cannot transition to Delivered until Manager/Admin clears the condition back to Good.

Operators may report damage by setting condition to:
- Damaged,
- NeedsInspection.

Operators cannot clear condition back to Good.

Only Manager/Admin can clear:
- Damaged -> Good
- NeedsInspection -> Good

Inspection clearance:
- must use the normal manual movement flow,
- requires non-empty inspection or decision note,
- creates AssetMovement with sourceType Manual,
- stores the note in AssetMovement.notes,
- writes AuditLog,
- must be transactional.

Inspection clearance does not use the approval queue.

---

## Lost Asset Rules

Lost asset handling must not be bypassed through normal movement updates.

Lost cannot transition to these statuses through normal movement update:
- Available
- InTransit
- Delivered

Operator Lost-to-Available must use the approval-request flow.

Manager/Admin Lost-to-Available must use the dedicated direct-reactivation flow defined in Document 06.

Manager/Admin Lost-to-Available requests submitted through the normal manual or QR movement endpoints must be rejected.

Lost-to-Available must use:
- Operator approval flow, or
- Manager/Admin direct reactivation flow.

Any other transition out of Lost is prohibited unless explicitly defined.

---

## Operator Lost-to-Available Approval Request

Operator cannot directly reactivate a Lost asset.

Operator Lost-to-Available creates a pending MovementApproval only when:
- asset is active,
- asset is currently Lost,
- asset condition is Good,
- no pending Lost-reactivation approval already exists,
- submitted notes are non-empty.

Submitted notes must be stored as MovementApproval.requestReason.

The asset remains Lost while approval is pending.

If the Lost asset condition is Damaged or NeedsInspection, Manager/Admin must first clear the condition before reactivation.

Duplicate pending Lost-reactivation requests must be rejected with conflict response.

A normal valid Lost-reactivation approval request is not automatically suspicious.

---

## Manager/Admin Approval Decision

Manager/Admin can approve or reject pending Lost-reactivation requests.

Approval and rejection require non-empty decision note.

Before approval, the backend must revalidate:
- approval is still Pending,
- asset is still active,
- asset is still Lost,
- asset condition is still Good,
- requested transition is still valid.

If approved:
- approvalStatus becomes Approved,
- final AssetMovement is created,
- final movement preserves requestedSourceType,
- final AssetMovement.notes preserves MovementApproval.requestReason,
- Manager/Admin decisionNote remains on MovementApproval and AuditLog metadata,
- asset becomes Available,
- AuditLog is written.

Approval execution must be transactional.

If rejected:
- approvalStatus becomes Rejected,
- asset remains Lost,
- decisionNote remains on MovementApproval and AuditLog metadata,
- AuditLog is written.

Rejection execution must be transactional.

A QR reactivation request must ultimately create a QR movement, not a Manual movement.

---

## Manager/Admin Direct Lost Reactivation

Manager/Admin can directly reactivate a Lost asset only when:
- asset is active,
- asset is currently Lost,
- condition is Good,
- decision note is non-empty,
- no pending Lost-reactivation approval exists.

Direct reactivation:
- must use the dedicated direct-reactivation flow defined in Document 06,
- uses sourceType Manual,
- creates AssetMovement,
- stores decision note in AssetMovement.notes,
- updates asset status to Available,
- writes AuditLog,
- must be transactional.

Direct reactivation must be rejected if a pending Lost-reactivation approval already exists.

The normal manual and QR movement endpoints must not perform direct Manager/Admin Lost reactivation.

---

## Lost-to-Retired

Admin may change a Lost asset to Retired through normal manual movement flow.

Rules:
- Admin only,
- Manager and Operator cannot perform this transition,
- non-empty retirement note is required,
- sourceType is Manual,
- AssetMovement must be created,
- AuditLog must be written,
- processing must be transactional.

The retirement note must be stored in AssetMovement.notes and AuditLog metadata.

---

## General Status Transition Policy

The MVP does not use a full workflow engine.

Rules:
- Available cannot move directly to Delivered.
- A movement that changes status to Available requires resulting condition Good.
- A condition-only damage report may keep the existing status unchanged.
- Assets already Damaged or NeedsInspection before the request cannot transition to InTransit or Delivered.
- A movement may change Available to InTransit while also reporting Damaged or NeedsInspection when the pre-movement condition is Good.
- Lost cannot leave Lost through normal movement update except approved paths.
- Operator Lost-to-Available uses approval.
- Manager/Admin Lost-to-Available uses the dedicated direct-reactivation flow.
- Lost-to-Retired is Admin-only.
- Retired is terminal.
- All other unlisted status transitions are allowed only if they do not violate permission, condition, approval, active-resource, Lost, Retired, or sourceType rules.

---

## Risk vs Suspicious Activity

The system must distinguish risk from suspicious activity.

Risk is a current or historical asset concern detected by backend rules.

Examples:
- asset InTransit too long,
- asset not updated for many days,
- asset condition is Damaged,
- asset status is Lost,
- asset has suspicious activity on record.

Suspicious activity is a specific completed movement or blocked attempt that matches an explicit suspicious rule.

Every invalid movement attempt must be audited.

Invalid attempts are marked suspicious only when an explicit suspicious rule matches.

---

## Suspicious Activity Rules

An event is suspicious when it matches one of these rules:
- unauthorized direct Lost -> Available attempt,
- unauthorized direct Lost -> InTransit attempt,
- unauthorized direct Lost -> Delivered attempt,
- repeated Lost reactivation requests exceeding the approved threshold,
- attempted status transition to Available while the resulting condition is Damaged or NeedsInspection,
- conflicting location information,
- too many completed updates in a short period.

A condition-only damage report that leaves an existing Available status unchanged is not automatically suspicious.

A normal Lost reactivation request through the approval process is not automatically suspicious.

Suspicious activity must be visible to Admin and Manager.

The suspicious activity view must combine or normalize:
- suspicious completed movements from AssetMovement,
- suspicious blocked attempts from AuditLog.

---

## Configurable Thresholds

These MVP default values must be configurable.

Claude must not silently invent different values.

Defaults:
- maximum InTransit duration: 72 hours,
- high DelayRisk threshold: InTransit longer than 7 days,
- stale asset duration: 7 days without update,
- high stale threshold: 14 days without update,
- high update frequency: more than 5 completed movements within 60 minutes,
- repeated Lost reactivation threshold: more than 2 submitted attempts within 24 hours,
- conflicting location window: different destination locations submitted for the same asset within 10 minutes.

For the high update-frequency threshold:
- count completed AssetMovement records for the asset,
- blocked attempts do not count as completed updates.

For the repeated Lost-reactivation threshold:
- count every submitted Lost-reactivation attempt,
- include attempts rejected because a Pending approval already exists.

For conflicting location detection:
- compare submitted destination locations for the same asset,
- count both completed movements and blocked attempts when the submitted destination is known.
- only explicitly submitted destination values count,
- omitted `toLocationId` is not compared,
- if `toLocationId` is omitted, the request is excluded from conflicting-location detection.

Thresholds must not be hard-coded randomly inside business logic.

---

## Stale Risk Calculation

Stale duration is calculated from the most recent AssetMovement.createdAt.

If the asset has no movement history, stale duration is calculated from Asset.createdAt.

---

## Risk Eligibility

DelayRisk and NoRecentUpdate apply only to active, non-Retired assets.

Dashboard risk calculations use active assets by default.

Other historical records remain available, but inactive or Retired assets should not become newly delayed or stale.

For the MVP, SuspiciousMovement risk exists when the asset has any suspicious completed movement or suspicious blocked attempt on record.

SuspiciousMovement is therefore a historical risk flag in the MVP.

---

## Risk Level Assignment

Risk level must be assigned deterministically by backend rules.

If multiple levels apply to the same risk type, use the highest applicable level.

## DelayRisk

Medium:
- asset is InTransit longer than 72 hours.

High:
- asset is InTransit longer than 7 days.

## NoRecentUpdate

Medium:
- asset has no movement update for 7 days.

High:
- asset has no movement update for 14 days.

If the asset has no movement history, use Asset.createdAt.

## DamagedAsset

Medium:
- condition = NeedsInspection.

High:
- condition = Damaged.

## LostAsset

Critical:
- status = Lost.

## SuspiciousMovement

High:
- the asset has suspicious activity on record.

Critical:
- suspicious activity involves unauthorized Lost-asset reactivation,
- repeated Lost-reactivation requests exceed the approved threshold.

Low is reserved for future lower-severity risk rules.

The MVP currently produces Medium, High, or Critical.

---

## Backend Validation Order

The backend should validate movement requests in this order:

1. Authenticate user.
2. Load asset.
3. Check user permissions.
4. Determine backend-owned sourceType.
5. Validate asset is active.
6. Validate asset is not Retired for normal movement.
7. Validate target location is active when location changes.
8. Validate at least one real state change exists.
9. Validate required notes for transition-specific rules.
10. Validate status and condition rules using both the current state and requested resulting state.
11. Validate Lost and Retired restrictions.
12. Determine whether approval is required.
13. If invalid, reject the request, keep the asset unchanged, write AuditLog, mark it suspicious only if an explicit suspicious rule matches, and return the error result.
14. If approval is required, create pending MovementApproval, keep the asset unchanged, write AuditLog, commit the transaction, and return the pending-approval result.
15. If completed movement is valid, detect whether it matches a suspicious rule.
16. Create AssetMovement with suspicious result.
17. Update Asset current state.
18. Write AuditLog.
19. Commit transaction.
20. Return result.

The exact database operation order may vary, but related state changes must be committed atomically.

---

## Audit Requirements

AuditLog must be written for:
- valid movement created,
- QR update submitted,
- invalid movement attempted,
- suspicious movement detected,
- approval requested,
- duplicate approval request rejected,
- approval approved,
- approval rejected,
- Lost asset directly reactivated by Manager/Admin,
- Lost asset changed to Retired by Admin,
- condition cleared back to Good by Manager/Admin,
- AI recommendation generated.

Audit logs support traceability and managerial review.

Audit metadata must not store secrets.

---

## AI Boundary

AI must not enforce movement rules.

Risk detection must be rule-based first.

AI may only explain detected risks and recommend next actions.

AI must not:
- approve movements,
- update assets,
- create AssetMovement records,
- override backend validation,
- invent missing data.