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
    return <EmptyState label="No approval requests found." />;
  }

  return (
    <table style={{ width: "100%", borderCollapse: "collapse", marginTop: "1rem" }}>
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
    <tr style={{ borderTop: "1px solid #eee", verticalAlign: "top" }}>
      <td>
        {approval.assetCode}
        <br />
        {approval.assetName}
      </td>
      <td>
        Status: <Badge text={approval.requestedStatus} /> Condition: <Badge text={approval.requestedCondition} />
        <br />
        {approval.requestedLocationName ?? "Current location"}
      </td>
      <td>{approval.requestedByUserName}</td>
      <td>{approval.requestedSourceType}</td>
      <td>{approval.requestReason}</td>
      <td>
        <Badge text={approval.approvalStatus} />
      </td>
      <td>{approval.decisionNote ?? "—"}</td>
      <td>{new Date(approval.createdAt).toLocaleString()}</td>
      <td style={{ minWidth: 220 }}>
        {isPending ? (
          <>
            {error && <ErrorBanner message={error} />}
            <textarea
              value={decisionNote}
              onChange={(event) => setDecisionNote(event.target.value)}
              placeholder="Decision note (required)"
              rows={2}
              style={{ display: "block", width: "100%", marginBottom: "0.35rem" }}
            />
            <button onClick={() => void decide("approve")} disabled={!canDecide || isSubmitting}>
              Approve
            </button>{" "}
            <button onClick={() => void decide("reject")} disabled={!canDecide || isSubmitting}>
              Reject
            </button>
          </>
        ) : (
          "—"
        )}
      </td>
    </tr>
  );
}
