import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

// Navigation is role-based per Document 07. Only Phase 2 destinations are wired up;
// later phases (Approvals, Suspicious Activity, Risk Recommendations, Audit Logs,
// Dashboard, QR Scanner) are added when those features are implemented.
export function AppLayout() {
  const { user, logout } = useAuth();

  if (!user) {
    return <Outlet />;
  }

  const links: { to: string; label: string }[] = [{ to: "/assets", label: "Assets" }];

  if (user.role === "Admin") {
    links.push({ to: "/locations", label: "Locations" }, { to: "/users", label: "Users" });
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
