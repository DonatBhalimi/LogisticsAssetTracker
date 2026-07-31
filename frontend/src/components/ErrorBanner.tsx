export function ErrorBanner({ message }: { message: string }) {
  return (
    <div role="alert" style={{ background: "#fdecea", color: "#611a15", padding: "0.75rem 1rem", borderRadius: 4, marginBottom: "1rem" }}>
      {message}
    </div>
  );
}
