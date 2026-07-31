# Claude Code Prompts

## Document Purpose

This document provides phase-based prompts for Claude Code.

Do not ask Claude Code to build the whole project in one session.

Use one phase at a time.

Claude must follow the approved documents and must not invent new business rules.

---

## Global Rule for Every Phase

At the start of each phase, Claude must:

1. Read only the listed documents.
2. Summarize the phase goal.
3. List the files it plans to create or edit.
4. Wait for approval before major file changes.
5. Implement in small steps.
6. Run build/tests when possible.
7. Report changed files and remaining gaps.

Claude must not:
- jump ahead to later phases,
- add unapproved features,
- change business rules,
- invent new endpoints,
- implement GPS in the MVP,
- let AI control business logic.

---

## Phase 0 Prompt: Gap Review Before Coding

I am starting Phase 0: Gap Review Before Coding for Logistics Asset Tracker.

Read:
- @CLAUDE.md
- @docs/01_Project_Requirements.md
- @docs/02_Functional_Requirements.md
- @docs/03_Technical_Architecture.md
- @docs/04_Data_Model.md
- @docs/05_Movement_Rules_and_Suspicious_Activity.md
- @docs/06_API_Specification.md

Do not write code.

Task:
- Review the implementation readiness of the project.
- Identify only blocking contradictions or missing decisions.
- Confirm MVP scope, architecture, data model, movement rules, approval flow, and API boundaries.
- Do not add new features.
- Ask only essential questions.

Deliverable:
- Short readiness summary.
- Blocking issues only.
- Clear next-step recommendation.

---

## Phase 1 Prompt: Project Setup and Architecture

I am starting Phase 1: Project Setup and Architecture.

Read:
- @CLAUDE.md
- @docs/03_Technical_Architecture.md
- @docs/06_API_Specification.md
- @docs/09_Research_and_Library_Decisions.md
- @docs/10_Phase_Implementation_Plan.md
- @docs/11_Testing_and_Acceptance_Criteria.md

Do not code immediately.

First:
- confirm tech stack,
- propose backend folder structure,
- propose frontend folder structure,
- identify required dependencies,
- research and record selected QR generation library,
- research and record selected QR scanning library,
- confirm Phase 1 plan,
- wait for approval.

After approval:
- create backend project,
- create frontend project,
- configure PostgreSQL and EF Core,
- add basic environment configuration,
- make frontend and backend run locally.

Deliverable:
- frontend runs locally,
- backend runs locally,
- database connection works,
- QR library decisions are no longer TBD,
- changed files reported.

---

## Phase 2 Prompt: Core Entities, Authentication, and Basic UI

I am starting Phase 2: Core Entities, Authentication, and Basic UI.

Read:
- @CLAUDE.md
- @docs/02_Functional_Requirements.md
- @docs/04_Data_Model.md
- @docs/06_API_Specification.md
- @docs/07_UI_UX_Specification.md
- @docs/11_Testing_and_Acceptance_Criteria.md

Implement only Phase 2 scope:
- User model,
- authentication and JWT login,
- role-based authorization,
- Admin user-management endpoints,
- Location model and endpoints,
- Asset model and endpoints,
- AuditLog model and basic audit logging,
- backend asset code generation,
- backend QR token generation,
- seed/demo data,
- login page,
- Users page,
- Locations page,
- Assets list page,
- Asset details page,
- Create/Edit Asset pages.

Do not implement:
- QR scanning,
- movement update flow,
- full movement rules,
- approvals,
- AI,
- GPS.

After implementation:
- run build/tests,
- show changed files,
- explain role enforcement,
- confirm Phase 2 acceptance criteria,
- list remaining gaps.

---

## Phase 3 Prompt: Basic Movement Tracking and QR Flow

I am starting Phase 3: Basic Movement Tracking and QR Flow.

Phase 3 movement scope:

Until Phase 4, allow only location-only movement updates where status and condition remain unchanged.

Reject status changes, condition changes, Lost reactivation, inspection clearance, Lost-to-Retired, and any other sensitive or unsupported transition.

Full status and condition movement rules are implemented in Phase 4.

Read:
- @CLAUDE.md
- @docs/04_Data_Model.md
- @docs/05_Movement_Rules_and_Suspicious_Activity.md
- @docs/06_API_Specification.md
- @docs/07_UI_UX_Specification.md
- @docs/09_Research_and_Library_Decisions.md
- @docs/11_Testing_and_Acceptance_Criteria.md

Implement only Phase 3 scope:
- AssetMovement model,
- manual movement endpoint,
- QR movement endpoint,
- QR code generation using selected library,
- QR scanner page using selected library,
- manual asset-code fallback,
- movement history records,
- movement history display.

Enforce foundational rules in this phase:
- authentication,
- permissions,
- active asset check,
- active location check,
- no-change rejection,
- backend-assigned sourceType.

Until Phase 4:
- allow only explicitly implemented basic, non-sensitive transitions,
- reject sensitive or unsupported transitions,
- do not temporarily allow transitions that violate Document 05.

Do not implement full sensitive movement workflows yet.

After implementation:
- run build/tests,
- show changed files,
- explain how QR maps to asset update,
- confirm sourceType behavior,
- confirm Phase 3 acceptance criteria,
- list remaining gaps.

---

## Phase 4 Prompt: Movement Rules, Approvals, Suspicious Activity, and Audit Review

I am starting Phase 4: Movement Rules, Approvals, Suspicious Activity, and Audit Review.

Read:
- @CLAUDE.md
- @docs/04_Data_Model.md
- @docs/05_Movement_Rules_and_Suspicious_Activity.md
- @docs/06_API_Specification.md
- @docs/07_UI_UX_Specification.md
- @docs/11_Testing_and_Acceptance_Criteria.md

Implement only Phase 4 scope:
- full backend movement validation,
- status and condition rules,
- inactive and Retired asset restrictions,
- inspection-clearance behavior,
- Lost-to-Available approval request flow,
- Manager/Admin approve and reject flow,
- direct Manager/Admin Lost reactivation,
- Admin Lost-to-Retired behavior,
- suspicious completed movement detection,
- suspicious blocked attempt logging,
- Approvals page,
- Suspicious Activity page,
- Audit Logs page.

Rules:
- backend validation is required,
- frontend validation alone is not acceptable,
- invalid movements must return clear errors,
- asset must remain unchanged while approval is pending.

After implementation:
- run build/tests,
- show changed files,
- explain where each rule is enforced,
- confirm Phase 4 acceptance criteria,
- list remaining gaps.

---

## Phase 5 Prompt: Dashboard and AI Risk Recommendations

I am starting Phase 5: Dashboard and AI Risk Recommendations.

Read:
- @CLAUDE.md
- @docs/04_Data_Model.md
- @docs/05_Movement_Rules_and_Suspicious_Activity.md
- @docs/06_API_Specification.md
- @docs/07_UI_UX_Specification.md
- @docs/08_AI_Risk_Recommendation_Specification.md
- @docs/11_Testing_and_Acceptance_Criteria.md

Implement only Phase 5 scope:
- dashboard summary cards,
- assets by status chart,
- assets by condition chart,
- assets by location chart,
- recent movements section,
- suspicious activity section,
- pending approvals section,
- rule-based risk detection,
- mocked AI recommendation service,
- AI output validation,
- RiskRecommendation storage,
- risk recommendation UI display.

Rules:
- risk detection must be rule-based first,
- AI only explains and recommends,
- AI must not update assets,
- AI must not approve movements,
- AI must not override backend rules,
- use the mocked AI service first,
- do not connect a real AI API until the mocked flow works and is approved.

After implementation:
- run build/tests,
- show changed files,
- explain risk detection flow,
- explain AI recommendation flow,
- confirm Phase 5 acceptance criteria,
- list remaining gaps.

---

## Phase 6 Prompt: Testing, Polish, README, and Demo

I am starting Phase 6: Testing, Polish, README, and Demo.

Read:
- @CLAUDE.md
- @docs/02_Functional_Requirements.md
- @docs/05_Movement_Rules_and_Suspicious_Activity.md
- @docs/06_API_Specification.md
- @docs/07_UI_UX_Specification.md
- @docs/08_AI_Risk_Recommendation_Specification.md
- @docs/10_Phase_Implementation_Plan.md
- @docs/11_Testing_and_Acceptance_Criteria.md

Do not add major new features.

Focus only on:
- fixing bugs,
- testing main flows,
- improving UI consistency,
- adding empty states,
- improving validation messages,
- adding demo data,
- improving README,
- adding screenshots if useful,
- preparing demo script.

After implementation:
- run full build/tests,
- show final changed files,
- confirm demo checklist,
- list known limitations honestly,
- suggest final GitHub commit message.

---

## Final Reminder

Claude Code should work phase by phase.

Each phase must end with:
- working code,
- build/test result,
- changed files,
- acceptance criteria status,
- remaining gaps.

Do not move to the next phase until the current phase works.