import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

// Navigation is role-based per Document 07. Dashboard and Risk Recommendations are
// added when those features are implemented (Phase 5) — Phase 4 adds Approvals,
// Suspicious Activity, and Audit Logs only.
export function AppLayout() {
  const { user, logout } = useAuth();

  if (!user) {
    return <Outlet />;
  }

  const links: { to: string; label: string }[] = [{ to: "/assets", label: "Assets" }];

  if (user.role === "Admin") {
    links.push({ to: "/locations", label: "Locations" }, { to: "/users", label: "Users" });
  }

  if (user.role === "Admin" || user.role === "Manager") {
    links.push(
      { to: "/approvals", label: "Approvals" },
      { to: "/suspicious-activity", label: "Suspicious Activity" },
      { to: "/audit-logs", label: "Audit Logs" }
    );
  }

  // Document 07 navigation lists "QR Scanner" under Operator only.
  if (user.role === "Operator") {
    links.push({ to: "/scan", label: "QR Scanner" });
  }

  return (
    <div>
      <header style={{ display: "flex", justifyContent: "space-between", alignItems: "center", padding: "0.75rem 1.5rem", borderBottom: "1px solid #ddd" }}>
        <nav style={{ display: "flex", gap: "1rem" }}>
          {links.map((link) => (
            <NavLink key={link.to} to={link.to}>
              {link.label}
            </NavLink>
          ))}
        </nav>
        <div style={{ display: "flex", alignItems: "center", gap: "1rem" }}>
          <span>
            {user.fullName} ({user.role})
          </span>
          <button onClick={() => void logout()}>Log out</button>
        </div>
      </header>
      <main style={{ padding: "1.5rem" }}>
        <Outlet />
      </main>
    </div>
  );
}
