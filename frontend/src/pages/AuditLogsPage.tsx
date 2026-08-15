import { useEffect, useState, type FormEvent } from "react";
import * as auditLogsApi from "../api/auditLogs";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { AuditLogTable } from "../features/auditLogs/AuditLogTable";
import { ApiError } from "../types/common";
import type { AuditLog } from "../types/auditLog";

const ENTITY_TYPES = ["", "User", "Location", "Asset", "AssetMovement", "MovementApproval"];

// Document 07 Screen 14: Admin/Manager review of completed actions and blocked attempts.
export function AuditLogsPage() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [entityType, setEntityType] = useState("");
  const [action, setAction] = useState("");
  const [userId, setUserId] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function refresh() {
    setIsLoading(true);
    setError(null);
    try {
      const result = await auditLogsApi.listAuditLogs({
        entityType: entityType || undefined,
        action: action.trim() || undefined,
        userId: userId.trim() || undefined,
        fromDate: fromDate || undefined,
        toDate: toDate || undefined,
        page: 1,
        pageSize: 100,
        sortDirection: "desc",
      });
      setLogs(result.items);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to load audit logs.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void refresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function handleFilterSubmit(event: FormEvent) {
    event.preventDefault();
    void refresh();
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Audit Logs</h1>
          <p className="page-header-desc">Review completed actions and blocked attempts.</p>
        </div>
      </div>

      <form onSubmit={handleFilterSubmit} className="filters-bar">
        <div className="field">
          <label htmlFor="fromDate">From</label>
          <input id="fromDate" type="date" value={fromDate} onChange={(event) => setFromDate(event.target.value)} />
        </div>
        <div className="field">
          <label htmlFor="toDate">To</label>
          <input id="toDate" type="date" value={toDate} onChange={(event) => setToDate(event.target.value)} />
        </div>
        <div className="field">
          <label htmlFor="entityType">Entity Type</label>
          <select id="entityType" value={entityType} onChange={(event) => setEntityType(event.target.value)}>
            {ENTITY_TYPES.map((t) => (
              <option key={t || "all"} value={t}>
                {t || "All"}
              </option>
            ))}
          </select>
        </div>
        <div className="field">
          <label htmlFor="action">Action</label>
          <input id="action" type="text" value={action} onChange={(event) => setAction(event.target.value)} placeholder="e.g. MovementBlocked" />
        </div>
        <div className="field">
          <label htmlFor="userId">User ID</label>
          <input id="userId" type="text" value={userId} onChange={(event) => setUserId(event.target.value)} placeholder="user GUID" />
        </div>
        <button type="submit" className="btn btn-secondary">
          Apply Filters
        </button>
      </form>

      {error && <ErrorBanner message={error} />}
      {isLoading ? <LoadingState /> : <AuditLogTable logs={logs} />}
    </div>
  );
}
