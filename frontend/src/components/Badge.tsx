// Badges are always text-based; color is a secondary cue, never the only signal (Document 07).
const BADGE_COLORS: Record<string, string> = {
  Available: "#1b5e20",
  InTransit: "#0d47a1",
  Delivered: "#4a148c",
  Lost: "#b71c1c",
  Retired: "#424242",
  Good: "#1b5e20",
  Damaged: "#b71c1c",
  NeedsInspection: "#e65100",
  Active: "#1b5e20",
  Inactive: "#616161",
};

export function Badge({ text }: { text: string }) {
  const color = BADGE_COLORS[text] ?? "#333333";
  return (
    <span
      style={{
        display: "inline-block",
        padding: "0.15rem 0.5rem",
        borderRadius: 4,
        border: `1px solid ${color}`,
        color,
        fontSize: "0.85rem",
        whiteSpace: "nowrap",
      }}
    >
      {text}
    </span>
  );
}
