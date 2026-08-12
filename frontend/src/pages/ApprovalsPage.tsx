import { useEffect, useState } from "react";
import * as approvalsApi from "../api/approvals";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { ApprovalsTable } from "../features/approvals/ApprovalsTable";
import { ApiError } from "../types/common";
import type { ApprovalStatus, MovementApproval } from "../types/approval";

const STATUS_FILTERS: (ApprovalStatus | "")[] = ["", "Pending", "Approved", "Rejected"];

// Document 07 Screen 9: Admin/Manager review of Lost-reactivation requests.
export function ApprovalsPage() {
  const [approvals, setApprovals] = useState<MovementApproval[]>([]);
  const [status, setStatus] = useState<ApprovalStatus | "">("Pending");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function refresh() {
    setIsLoading(true);
    setError(null);
    try {
      const result = await approvalsApi.listApprovals({
        status: status || undefined,
        page: 1,
        pageSize: 50,
        sortDirection: "desc",
      });
      setApprovals(result.items);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to load approvals.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void refresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [status]);

  async function handleApprove(id: string, decisionNote: string) {
    await approvalsApi.approveMovementRequest(id, { decisionNote });
    await refresh();
  }

  async function handleReject(id: string, decisionNote: string) {
    await approvalsApi.rejectMovementRequest(id, { decisionNote });
    await refresh();
  }

  return (
    <div>
      <h1>Approvals</h1>

      <div style={{ marginBottom: "1rem" }}>
        <label htmlFor="statusFilter">Status</label>{" "}
        <select id="statusFilter" value={status} onChange={(event) => setStatus(event.target.value as ApprovalStatus | "")}>
          {STATUS_FILTERS.map((s) => (
            <option key={s || "all"} value={s}>
              {s || "All"}
            </option>
          ))}
        </select>
      </div>

      {error && <ErrorBanner message={error} />}
      {isLoading ? (
        <LoadingState />
      ) : (
        <ApprovalsTable approvals={approvals} onApprove={handleApprove} onReject={handleReject} />
      )}
    </div>
  );
}
