import { useState } from "react";
import { Badge } from "../../components/Badge";
import { EmptyState } from "../../components/EmptyState";
import { ErrorBanner } from "../../components/ErrorBanner";
import { ApiError } from "../../types/common";
import type { MovementApproval } from "../../types/approval";

interface ApprovalsTableProps {
  approvals: MovementApproval[];
  onApprove: (id: string, decisionNote: string) => Promise<void>;
  onReject: (id: string, decisionNote: string) => Promise<void>;
}

// Document 07 Screen 9: Pending records may be decided; Approved/Rejected are read-only.
export function ApprovalsTable({ approvals, onApprove, onReject }: ApprovalsTableProps) {
  if (approvals.length === 0) {
    return (
      <div className="table-card">
        <EmptyState label="No approval requests found." />
      </div>
    );
  }

  return (
    <div className="table-card">
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th align="left">Asset</th>
              <th align="left">Requested Change</th>
              <th align="left">Requested By</th>
              <th align="left">Source</th>
              <th align="left">Request Reason</th>
              <th align="left">Status</th>
              <th align="left">Decision Note</th>
              <th align="left">Requested At</th>
              <th align="left">Actions</th>
            </tr>
          </thead>
          <tbody>
            {approvals.map((approval) => (
              <ApprovalRow key={approval.id} approval={approval} onApprove={onApprove} onReject={onReject} />
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function ApprovalRow({
  approval,
  onApprove,
  onReject,
}: {
  approval: MovementApproval;
  onApprove: (id: string, decisionNote: string) => Promise<void>;
  onReject: (id: string, decisionNote: string) => Promise<void>;
}) {
  const [decisionNote, setDecisionNote] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const isPending = approval.approvalStatus === "Pending";
  const canDecide = decisionNote.trim() !== "";

  async function decide(action: "approve" | "reject") {
    setError(null);
    setIsSubmitting(true);
    try {
      if (action === "approve") {
        await onApprove(approval.id, decisionNote.trim());
      } else {
        await onReject(approval.id, decisionNote.trim());
      }
      setDecisionNote("");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to record decision.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <tr>
      <td>
        <div>{approval.assetCode}</div>
        <div className="cell-muted">{approval.assetName}</div>
      </td>
      <td>
        <div className="btn-row" style={{ marginBottom: "4px" }}>
          <Badge text={approval.requestedStatus} />
          <Badge text={approval.requestedCondition} />
        </div>
        <div className="cell-muted">{approval.requestedLocationName ?? "Current location"}</div>
      </td>
      <td>{approval.requestedByUserName}</td>
      <td>
        <Badge text={approval.requestedSourceType} />
      </td>
      <td className="cell-muted">{approval.requestReason}</td>
      <td>
        <Badge text={approval.approvalStatus} />
      </td>
      <td className="cell-muted">{approval.decisionNote ?? "—"}</td>
      <td className="cell-muted">{new Date(approval.createdAt).toLocaleString()}</td>
      <td style={{ minWidth: 220 }}>
        {isPending ? (
          <>
            {error && <ErrorBanner message={error} />}
            <textarea
              value={decisionNote}
              onChange={(event) => setDecisionNote(event.target.value)}
              placeholder="Decision note (required)"
              rows={2}
              style={{ marginBottom: "8px" }}
            />
            <div className="btn-row">
              <button className="btn btn-primary btn-sm" onClick={() => void decide("approve")} disabled={!canDecide || isSubmitting}>
                Approve
              </button>
              <button className="btn btn-danger btn-sm" onClick={() => void decide("reject")} disabled={!canDecide || isSubmitting}>
                Reject
              </button>
            </div>
          </>
        ) : (
          <span className="cell-muted">—</span>
        )}
      </td>
    </tr>
  );
}
