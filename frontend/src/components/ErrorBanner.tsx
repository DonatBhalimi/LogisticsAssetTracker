export function ErrorBanner({ message }: { message: string }) {
  return (
    <div role="alert" className="error-banner">
      {message}
    </div>
  );
}
