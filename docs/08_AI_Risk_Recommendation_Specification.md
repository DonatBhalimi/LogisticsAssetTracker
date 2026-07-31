# AI Risk Recommendation Specification

## Document Purpose

This document defines how AI-assisted risk recommendations work in the Logistics Asset Tracker MVP.

It does not define risk thresholds, movement rules, approval rules, or database schema.

Risk detection rules must follow Documents 05 and 06.

---

## Core Principle

Risk detection is rule-based first.

AI is used only to explain detected risks and suggest practical next steps.

AI must not:
- detect risks independently,
- approve movements,
- change database records directly,
- override backend validation,
- invent missing information.

---

## Supported Risk Types

The MVP supports these risk types:

- DelayRisk
- NoRecentUpdate
- DamagedAsset
- LostAsset
- SuspiciousMovement

Risk levels, thresholds, severity mappings, and stale-risk calculations must follow Document 05.

---

## AI Recommendation Flow

1. Backend runs rule-based risk analysis.
2. If no risk is detected, return a no-risk result and do not create a RiskRecommendation record.
3. If one or more risks are detected, prepare structured AI input.
4. Send risk context to the AI service.
5. Validate the AI response.
6. Save one RiskRecommendation record per detected risk type.
7. Return the recommendation result to the frontend.

---

## AI Input

The AI service should receive structured data only.

Example:

~~~json
{
  "assetCode": "PALLET-0001",
  "assetType": "Pallet",
  "currentStatus": "InTransit",
  "currentCondition": "Good",
  "currentLocation": "Truck 12",
  "riskType": "DelayRisk",
  "riskLevel": "High",
  "hoursInTransit": 192,
  "recentMovements": [
    {
      "fromLocation": "Warehouse Skopje",
      "toLocation": "Truck 12",
      "newStatus": "InTransit",
      "sourceType": "QR",
      "createdAt": "2026-07-01"
    }
  ]
}
~~~

The backend should provide only known system data.

Missing data should be omitted or marked as null. AI must not guess it.

---

## Expected AI Output

AI must return structured JSON.

Example:

~~~json
{
  "reason": "The asset has been in transit longer than expected without a confirmed update.",
  "recommendation": "Contact the assigned operator or destination location to verify the asset status."
}
~~~

The backend owns `riskType` and `riskLevel`.

AI should generate only:
- reason,
- recommendation,
- optional short movement summary.

---

## Validation Rules

The backend must validate AI output before saving it.

Valid AI output must:
- be valid JSON,
- include a concise reason,
- include a practical recommendation,
- avoid unsupported claims,
- avoid changing risk level or risk type.

Invalid AI output must not be stored as AI-generated content.

---

## Fallback Behavior

If AI fails, times out, or returns invalid output:

- the request must not fail only because of AI,
- the backend must return the rule-based risk result,
- the backend must save a deterministic fallback recommendation with `generationSource = RuleEngine`.

Fallback example:

~~~text
This asset has been flagged as risky based on system rules. Review its latest movement history and take the appropriate operational action.
~~~

---

## Generation Source

Use:

- `RuleEngineWithAI` when rules detected the risk and AI produced valid explanation/recommendation.
- `RuleEngine` when rules detected the risk and the system used deterministic fallback text.

There is no standalone AI risk source in the MVP.

---

## UI Behavior

The UI may show recommendations in:

- Dashboard
- Asset Details
- Risk Recommendations Page

The UI must distinguish:
- rule-based fallback recommendations,
- AI-assisted recommendations.

The UI must not make AI appear responsible for approvals, movement validation, or final business decisions.