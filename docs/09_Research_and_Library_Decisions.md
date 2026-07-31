# Research and Library Decisions

## Document Purpose

This document records research tasks and library decisions before implementation.

Claude must not randomly choose libraries or introduce new technical tools without checking this document.

This document does not redefine business rules, movement rules, API behavior, database schema, or AI behavior.

Business behavior must follow Documents 02, 04, 05, and 06.

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

Charts:
- Recharts or another simple React chart library

AI:
- Start with mocked AI service
- Later connect Claude or OpenAI API only after the mocked flow works

QR Generation:
- To be researched before implementation

QR Scanning:
- To be researched before implementation

---

## Library Decision Rules

Before adding a new library, check:

- active maintenance,
- TypeScript support where relevant,
- documentation quality,
- compatibility with React or ASP.NET Core,
- ease of implementation,
- security concerns,
- whether the library is necessary for the MVP.

Avoid adding libraries for small tasks that can be handled simply.

---

## Research Task 1: QR Code Generation

Research:
- QR code generation for React or .NET,
- generating QR codes from permanent asset QR URLs,
- displaying QR codes in the UI,
- downloading or printing QR codes.

Decision:

Chosen library:
qrcode.react (frontend, React/TypeScript)

Reason:
`qrCodeValue` (Document 04) is a backend-generated opaque token/URL string only; the backend does not need an image-generation dependency. qrcode.react is actively maintained, has full TypeScript support, and renders SVG or Canvas output client-side from that string. SVG output supports download/print directly in the browser. react-qr-code was also evaluated and is a viable actively-maintained alternative; qrcode.react was chosen for broader adoption and dual SVG/Canvas export.

Implementation notes:
- Backend generates and stores the immutable `qrCodeValue` token at asset creation (Document 04); no QR image library is added to the backend.
- `GET /api/assets/{id}/qr` (Document 06) returns the token/URL as JSON; the frontend renders the scannable QR image from it using qrcode.react.

---

## Research Task 2: QR Code Scanning

Research:
- browser-based QR scanning,
- mobile browser camera permissions,
- React-compatible QR scanner libraries,
- fallback manual asset-code entry.

Decision:

Chosen library:
@yudiel/react-qr-scanner (frontend, React/TypeScript)

Reason:
Actively maintained, built on the Barcode Detection API, native TypeScript support, and handles camera permission flow on both desktop and mobile browsers. Older alternatives (e.g. react-qr-scanner) are unmaintained and were ruled out under the "active maintenance" criterion above.

Implementation notes:
- Manual asset-code fallback (Documents 02, 05, 07) remains required for devices/browsers without camera access or Barcode Detection API support.
- Scanning flow and camera-permission handling are implemented in Phase 3, not Phase 1.

---

## Research Task 3: Chart Library

Research:
- Recharts,
- simple React chart alternatives,
- dashboard card and chart requirements.

Decision:

Preferred library:
Recharts

Reason:
Recharts is suitable for simple dashboard charts such as assets by status, condition, location.
---

## Research Task 4: Asset Tracking Concepts

Research:
- how reusable logistics assets are tracked,
- common asset identifiers,
- movement history fields,
- why traceability matters.

Decision:

The MVP tracks:
- asset code,
- location,
- status,
- condition,
- source type,
- updated by user,
- timestamp,
- notes.

Detailed data structure must follow Document 04.

---

## Research Task 5: Movement Rules

Research:
- common asset lifecycle states,
- why Delivered should not happen directly from Available,
- why Damaged assets require inspection,
- why Lost asset reactivation needs control.

Decision:

The MVP does not use a complex workflow engine.

Movement behavior must follow Document 05.

No new movement rules should be added from research without explicit approval.

---

## Research Task 6: Future GPS Integration

Research:
- how GPS devices send tracking data,
- typical GPS payload fields,
- how GPS could later become a movement source.

Decision:

GPS is not implemented in the MVP.

The MVP must not accept GPS updates.

The design may remain future-compatible, but implementation must only support Manual and QR source types.

---

## Research Task 7: AI Structured Output

Research:
- how to request structured JSON from AI,
- how to validate AI output,
- how to handle AI failure,
- how to prevent AI from controlling business actions.

Decision:

AI output must be validated before saving.

Backend rules remain the source of truth.

AI must only explain risks and suggest actions.

Detailed AI behavior must follow Document 08.

---

## Research Task 8: Dashboard KPIs

Research:
- useful logistics dashboard KPIs,
- asset status distribution,
- delayed assets,
- damaged assets,
- lost assets,
- recent movements,
- suspicious activity,
- pending approvals.

Decision:

The MVP dashboard shows:
- active asset totals,
- assets by status,
- assets by condition,
- assets by location,
- recent movements,
- suspicious activity,
- risky assets,
- pending approvals.

Dashboard behavior must follow Documents 06 and 07.

---

## Useful Search Terms

- QR code generation React TypeScript
- ASP.NET Core QR code generation
- React QR scanner browser camera
- mobile browser camera QR scanner React
- warehouse asset tracking system
- logistics asset movement history
- reusable pallet tracking software
- GPS tracking device API payload
- AI structured JSON output validation
- dashboard KPIs for asset tracking

---

## Final Rule

Research may guide implementation choices, but it must not change approved MVP behavior.

If research suggests a new feature, new rule, or new workflow, it must be treated as future scope unless explicitly approved.