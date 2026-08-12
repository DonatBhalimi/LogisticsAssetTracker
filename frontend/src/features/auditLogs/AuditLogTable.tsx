import { Badge } from "../../components/Badge";
import { EmptyState } from "../../components/EmptyState";
import type { AuditLog } from "../../types/auditLog";

// Blocked-attempt actions vs. completed-action actions, so the two can be visually
// distinguished per Document 07 Screen 10 ("must be clearly distinguished").
const BLOCKED_ACTIONS = new Set(["MovementBlocked", "MovementApprovalDuplicateRejected"]);

function eventTypeFor(action: string): string {
  return BLOCKED_ACTIONS.has(action) ? "Blocked Attempt" : "Completed Action";
}

// Read-only audit table (Document 07 Screen 14), reused by AuditLogsPage and
// SuspiciousActivityPage (Document 07 Screen 10, filtered to isSuspicious=true).
export function AuditLogTable({ logs }: { logs: AuditLog[] }) {
  if (logs.length === 0) {
    return <EmptyState label="No audit log entries found." />;
  }

  return (
    <table style={{ width: "100%", borderCollapse: "collapse", marginTop: "1rem" }}>
      <thead>
        <tr>
          <th align="left">Timestamp</th>
          <th align="left">Event Type</th>
          <th align="left">Action</th>
          <th align="left">Entity Type</th>
          <th align="left">User</th>
          <th align="left">Suspicious</th>
          <th align="left">Details</th>
        </tr>
      </thead>
      <tbody>
        {logs.map((log) => (
          <tr key={log.id} style={{ borderTop: "1px solid #eee", verticalAlign: "top" }}>
            <td>{new Date(log.createdAt).toLocaleString()}</td>
            <td>
              <Badge text={eventTypeFor(log.action)} />
            </td>
            <td>{log.action}</td>
            <td>{log.entityType}</td>
            <td>{log.userName ?? "System"}</td>
            <td>
              {log.isSuspicious ? <Badge text="Suspicious" /> : "—"}
              {log.isSuspicious && log.suspiciousReason && (
                <div style={{ color: "#666", fontSize: "0.8rem" }}>{log.suspiciousReason}</div>
              )}
            </td>
            <td>
              {log.metadataJson ? (
                <details>
                  <summary>View</summary>
                  <pre style={{ whiteSpace: "pre-wrap", fontSize: "0.8rem", maxWidth: 360 }}>{log.metadataJson}</pre>
                </details>
              ) : (
                "—"
              )}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
