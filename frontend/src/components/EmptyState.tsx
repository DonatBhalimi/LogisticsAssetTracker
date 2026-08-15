export function EmptyState({ label = "No results found." }: { label?: string }) {
  return <p className="empty-state">{label}</p>;
}
