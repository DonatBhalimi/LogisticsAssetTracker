import { Badge } from "../../components/Badge";
import { EmptyState } from "../../components/EmptyState";
import type { AssetMovement } from "../../types/movement";

// Read-only movement history (Document 07 Screen 8). Status/condition columns are
// shown because the data exists on every record — Phase 3 movements simply always
// have previous === new for those two fields, since this phase is location-only.
export function MovementHistoryTable({ movements }: { movements: AssetMovement[] }) {
  if (movements.length === 0) {
    return <EmptyState label="No movement history yet." />;
  }

  return (
    <table style={{ width: "100%", borderCollapse: "collapse" }}>
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
          <tr key={m.id} style={{ borderTop: "1px solid #eee" }}>
            <td>{m.fromLocationName}</td>
            <td>{m.toLocationName}</td>
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
            <td>{m.sourceType}</td>
            <td>{m.updatedByUserName}</td>
            <td>{new Date(m.createdAt).toLocaleString()}</td>
            <td>{m.notes ?? "—"}</td>
            {/* isSuspicious is null for Operators (Document 06); render nothing rather than guess. */}
            <td>{m.isSuspicious == null ? "—" : <Badge text={m.isSuspicious ? "Suspicious" : "No"} />}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
