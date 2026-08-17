import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

// Navigation is role-based per Document 07.
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
    links.unshift({ to: "/dashboard", label: "Dashboard" });
    links.push(
      { to: "/approvals", label: "Approvals" },
      { to: "/suspicious-activity", label: "Suspicious Activity" },
      { to: "/risk-recommendations", label: "Risk Recommendations" },
      { to: "/audit-logs", label: "Audit Logs" }
    );
  }

  // Document 07 navigation lists "QR Scanner" under Operator only.
  if (user.role === "Operator") {
    links.push({ to: "/scan", label: "QR Scanner" });
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-header">
          <div className="sidebar-brand">Logistics Asset Tracker</div>
          <div className="sidebar-brand-sub">Operations Console</div>
        </div>
        <nav className="sidebar-nav">
          {links.map((link) => (
            <NavLink key={link.to} to={link.to} className={({ isActive }) => `sidebar-link${isActive ? " active" : ""}`}>
              {link.label}
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-footer">
          <div>
            <div className="sidebar-user-name">{user.fullName}</div>
            <div className="sidebar-user-role">{user.role}</div>
          </div>
          <button className="btn btn-secondary btn-sm" onClick={() => void logout()}>
            Log out
          </button>
        </div>
      </aside>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
