import { Badge } from "../../components/Badge";
import { EmptyState } from "../../components/EmptyState";
import type { AssetMovement } from "../../types/movement";

// Read-only movement history (Document 07 Screen 8).
export function MovementHistoryTable({ movements }: { movements: AssetMovement[] }) {
  if (movements.length === 0) {
    return (
      <div className="table-card">
        <EmptyState label="No movement history yet." />
      </div>
    );
  }

  return (
    <div className="table-card">
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th align="left">Previous Location</th>
              <th align="left">New Location</th>
              <th align="left">Previous Status</th>
              <th align="left">New Status</th>
              <th align="left">Previous Condition</th>
              <th align="left">New Condition</th>
              <th align="left">Source</th>
              <th align="left">Updated By</th>
              <th align="left">Timestamp</th>
              <th align="left">Notes</th>
              <th align="left">Suspicious</th>
            </tr>
          </thead>
          <tbody>
            {movements.map((m) => (
              <tr key={m.id} className={m.isSuspicious ? "suspicious-row" : undefined}>
                <td className="cell-muted">{m.fromLocationName}</td>
                <td className="cell-muted">{m.toLocationName}</td>
                <td>
                  <Badge text={m.previousStatus} />
                </td>
                <td>
                  <Badge text={m.newStatus} />
                </td>
                <td>
                  <Badge text={m.previousCondition} />
                </td>
                <td>
                  <Badge text={m.newCondition} />
                </td>
                <td>
                  <Badge text={m.sourceType} />
                </td>
                <td className="cell-muted">{m.updatedByUserName}</td>
                <td className="cell-muted">{new Date(m.createdAt).toLocaleString()}</td>
                <td className="cell-muted">{m.notes ?? "—"}</td>
                {/* isSuspicious is null for Operators (Document 06); render nothing rather than guess. */}
                <td>
                  {m.isSuspicious == null ? (
                    <span className="cell-muted">—</span>
                  ) : (
                    <Badge text={m.isSuspicious ? "Suspicious" : "No"} />
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
