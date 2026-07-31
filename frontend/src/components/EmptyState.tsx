export function EmptyState({ label = "No results found." }: { label?: string }) {
  return <p style={{ color: "#666" }}>{label}</p>;
}
