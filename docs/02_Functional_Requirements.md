# Functional Requirements

## Document Purpose

This document defines what the Logistics Asset Tracker MVP must do.

It describes system behavior, user permissions, and feature requirements.

It does not define database schema, API contracts, UI layout, implementation phases, or detailed thresholds.

Detailed rules are defined in Documents 04, 05, 06, 07, and 08.

---

## 1. Authentication and Roles

The system must support login, logout, and role-based access.

Inactive users must not be allowed to log in.

Roles:
- Admin
- Manager
- Operator

Admin can:
- manage users,
- manage locations,
- create and edit assets,
- deactivate assets,
- view all assets,
- view and generate asset QR codes,
- view dashboard analytics,
- view movement history,
- view suspicious activity,
- view audit logs,
- approve or reject Lost reactivation requests,
- directly reactivate Lost assets when allowed,
- change Lost assets to Retired when allowed,
- generate and view risk recommendations,
- perform permitted movement updates,
- clear inspected assets back to Good.

Manager can:
- view assets,
- view and generate asset QR codes,
- view dashboard analytics,
- view movement history,
- view suspicious activity,
- view audit logs,
- approve or reject Lost reactivation requests,
- directly reactivate Lost assets when allowed,
- generate and view risk recommendations,
- perform permitted movement updates,
- clear inspected assets back to Good.

Operator can:
- view active assets,
- view movement history for active assets,
- scan QR codes after login,
- perform permitted manual and QR movement updates,
- report damage by setting condition to Damaged or NeedsInspection.

Operator cannot:
- manage users,
- view inactive assets,
- view audit logs,
- view dashboard analytics,
- generate risk recommendations,
- approve Lost reactivation,
- directly reactivate Lost assets,
- change Lost assets to Retired,
- clear condition back to Good,
- bypass movement rules.

The `assignedToUser` field is informational metadata only and does not control asset visibility.

---

## 2. Asset Management

The system must support:
- create asset,
- view asset list,
- view asset details,
- edit asset basic information,
- deactivate asset,
- filter assets.

Asset edit must not bypass the movement flow.

Asset edit must not directly change:
- currentLocationId,
- status,
- condition,
- assetCode,
- qrCodeValue.

Changes to location, status, or condition must go through the movement update flow.

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

It is a computed risk signal.

Retired is terminal in the MVP.

Retired assets cannot receive normal movement updates.

---

## 3. Location Management

The system must support:
- create location,
- edit location,
- deactivate location,
- list locations.

Inactive locations remain available for historical records.

Inactive locations cannot be selected as new movement destinations.

Operators can use active locations only.

---

## 4. Manual Movement Update

A permitted user can manually update:
- location,
- status,
- condition,
- notes.

At least one of location, status, or condition must actually change.

Notes-only or no-change submissions must be rejected.

For a valid completed movement, the system must:
- validate rules in the backend,
- update the asset state,
- create AssetMovement,
- write AuditLog.

Invalid movement attempts must:
- leave the asset unchanged,
- not create AssetMovement,
- create AuditLog.

Manual completed movements use `sourceType = Manual`.

---

## 5. QR Tracking

Each asset must have a unique permanent QR value.

QR scanning requires login.

There must be no unauthenticated scan-and-view or scan-and-update path.

After scanning, the user is taken to the authenticated asset update flow.

Completed QR movements use `sourceType = QR`.

A QR submission that creates a pending approval must not create final AssetMovement until approval is accepted.

Manual asset-code fallback is allowed when camera scanning fails.

Manual fallback uses the normal manual movement flow.

Admin and Manager may view or generate asset QR codes.

Operators may scan QR codes but do not manage QR code generation.

---

## 6. Movement History

Every valid completed state-changing movement must create a movement history record.

Movement history must preserve:
- previous values,
- new values,
- sourceType,
- updatedBy,
- createdAt,
- notes,
- suspicious flag when applicable.

Movement history must not be overwritten or deleted during normal use.

---

## 7. Movement Rules

Backend validation is required.

Frontend validation may exist but is not trusted.

Core rules:
- Available cannot move directly to Delivered.
- A movement that changes status to Available requires the resulting condition to be Good.
- A condition-only damage report may keep the current status unchanged, including Available.
- Damaged or NeedsInspection assets cannot transition to InTransit or Delivered until cleared back to Good.
- Operators can set condition to Damaged or NeedsInspection.
- Only Manager/Admin can clear condition back to Good.
- Inspection clearance requires a non-empty note.
- Inspection clearance uses manual movement flow.
- Lost cannot move to Available, InTransit, or Delivered through normal movement updates.
- Operator Lost-to-Available requires approval.
- Operator Lost-to-Available requires condition Good.
- Operator Lost-to-Available requires non-empty notes.
- Manager/Admin direct Lost reactivation is allowed only when valid.
- Manager/Admin direct Lost reactivation must use the dedicated reactivation flow.
- Normal manual and QR movement endpoints must reject Manager/Admin direct Lost-to-Available attempts.
- Direct Lost reactivation requires a non-empty decision note.
- Direct Lost reactivation is rejected if a Pending approval exists.
- Lost-to-Retired is Admin-only.
- Lost-to-Retired requires a non-empty retirement note.
- Retired assets cannot receive normal movement updates.
- Inactive assets cannot receive movement updates.
- Inactive locations cannot be new destinations.

All detailed transition, approval, suspicious-activity, and threshold rules are defined in Document 05.

API behavior is defined in Document 06.

---

## 8. Suspicious Activity

The system must distinguish risk from suspicious activity.

Risk is an asset concern detected by backend rules.

Suspicious activity is a specific completed movement or blocked attempt that matches an explicit suspicious rule.

Every invalid movement attempt must be audited.

Invalid attempts are marked suspicious only when an explicit suspicious rule matches.

Suspicious activity must be visible to Admin and Manager.

Detailed suspicious-activity rules are defined in Document 05.

---

## 9. Approval Flow

Operator Lost-to-Available requires a formal approval request.

When requested:
- asset must be active,
- asset must currently be Lost,
- condition must be Good,
- no Pending Lost-reactivation approval may already exist,
- submitted notes become the request reason,
- asset remains Lost while pending.

Manager/Admin can approve or reject the request.

Approval and rejection require a non-empty decision note.

Approval must revalidate the asset and requested destination before changing state.

Approved movement must preserve:
- original request reason,
- original requestedSourceType.

A QR reactivation request must ultimately create a QR movement.

Manager/Admin direct Lost reactivation:
- does not use the approval queue,
- must use the dedicated direct-reactivation flow,
- is rejected while a Pending approval exists.

Damaged or NeedsInspection to Good does not use the approval queue.

It is handled through Manager/Admin permission.

---

## 10. Dashboard

Dashboard is for Admin and Manager.

It must provide operational visibility over:
- active assets,
- status,
- condition,
- location,
- recent movements,
- suspicious activity,
- risky assets,
- pending approvals.

Dashboard risk counts must come from live rule-based analysis.

Dashboard calculation must not automatically create RiskRecommendation records.

Detailed dashboard behavior is defined in Documents 06 and 07.

---

## 11. AI Risk Recommendations

Risk detection is rule-based first.

AI recommendations are generated on demand.

AI may provide:
- risk explanation,
- recommended action,
- optional short movement summary.

AI must not:
- update records directly,
- approve movements,
- override backend rules,
- invent missing data.

If risk exists, the system creates recommendation records according to Document 08.

If no risk exists, no RiskRecommendation record is created.

If AI fails, the system returns rule-based fallback behavior.

---

## 12. Audit Log

The system must log important completed actions and blocked attempts.

Audit logs support traceability and managerial review.

Audit logs are visible to Admin and Manager.

Audit metadata must not store:
- passwords,
- password hashes,
- JWT tokens,
- API keys,
- secrets.

Detailed audit behavior is defined in Documents 05 and 06.