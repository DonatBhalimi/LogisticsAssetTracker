// Badges are always text-based; color is a secondary cue, never the only signal (Document 07).
// Soft-tinted tones, not solid fills — matches the enterprise dashboard style direction.
type BadgeTone = "success" | "danger" | "warning" | "info" | "violet" | "neutral";

const BADGE_TONES: Record<string, BadgeTone> = {
  // Status
  Available: "success",
  InTransit: "info",
  Delivered: "violet",
  Lost: "danger",
  Retired: "neutral",
  // Condition
  Good: "success",
  Damaged: "danger",
  NeedsInspection: "warning",
  // Active / inactive
  Active: "success",
  Inactive: "neutral",
  // Suspicious indicator (Document 06/07)
  Suspicious: "danger",
  No: "neutral",
  // Approval status (Document 04)
  Pending: "warning",
  Approved: "success",
  Rejected: "danger",
  // SourceType (Document 05/06)
  Manual: "info",
  QR: "info",
  // Roles (Document 02)
  Admin: "neutral",
  Manager: "info",
  Operator: "neutral",
  // Audit log event type (completed vs. blocked)
  "Blocked Attempt": "warning",
  "Completed Action": "neutral",
};

export function Badge({ text }: { text: string }) {
  const tone = BADGE_TONES[text] ?? "neutral";
  return <span className={`badge badge-${tone}`}>{text}</span>;
}
