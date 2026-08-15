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
    return (
      <div className="table-card">
        <EmptyState label="No audit log entries found." />
      </div>
    );
  }

  return (
    <div className="table-card">
      <div className="table-wrap">
        <table className="data-table">
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
              <tr key={log.id} className={log.isSuspicious ? "suspicious-row" : undefined}>
                <td className="cell-muted">{new Date(log.createdAt).toLocaleString()}</td>
                <td>
                  <Badge text={eventTypeFor(log.action)} />
                </td>
                <td>{log.action}</td>
                <td className="cell-muted">{log.entityType}</td>
                <td className="cell-muted">{log.userName ?? "System"}</td>
                <td>
                  {log.isSuspicious ? <Badge text="Suspicious" /> : <span className="cell-muted">—</span>}
                  {log.isSuspicious && log.suspiciousReason && <div className="cell-hint">{log.suspiciousReason}</div>}
                </td>
                <td>
                  {log.metadataJson ? (
                    <details>
                      <summary style={{ cursor: "pointer", color: "var(--text-muted)", fontSize: "12.5px" }}>View</summary>
                      <pre
                        style={{
                          whiteSpace: "pre-wrap",
                          fontSize: "11.5px",
                          maxWidth: 360,
                          marginTop: "6px",
                          background: "var(--surface-muted)",
                          border: "1px solid var(--border)",
                          borderRadius: "6px",
                          padding: "8px",
                        }}
                      >
                        {log.metadataJson}
                      </pre>
                    </details>
                  ) : (
                    <span className="cell-muted">—</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
