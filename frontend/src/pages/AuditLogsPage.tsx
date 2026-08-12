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
      <h1>Audit Logs</h1>

      <form onSubmit={handleFilterSubmit} style={{ display: "flex", flexWrap: "wrap", gap: "0.75rem", alignItems: "flex-end", marginBottom: "1rem" }}>
        <div>
          <label htmlFor="fromDate">From</label>
          <input id="fromDate" type="date" value={fromDate} onChange={(event) => setFromDate(event.target.value)} style={{ display: "block" }} />
        </div>
        <div>
          <label htmlFor="toDate">To</label>
          <input id="toDate" type="date" value={toDate} onChange={(event) => setToDate(event.target.value)} style={{ display: "block" }} />
        </div>
        <div>
          <label htmlFor="entityType">Entity Type</label>
          <select id="entityType" value={entityType} onChange={(event) => setEntityType(event.target.value)} style={{ display: "block" }}>
            {ENTITY_TYPES.map((t) => (
              <option key={t || "all"} value={t}>
                {t || "All"}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label htmlFor="action">Action</label>
          <input id="action" type="text" value={action} onChange={(event) => setAction(event.target.value)} placeholder="e.g. MovementBlocked" style={{ display: "block" }} />
        </div>
        <div>
          <label htmlFor="userId">User ID</label>
          <input id="userId" type="text" value={userId} onChange={(event) => setUserId(event.target.value)} placeholder="user GUID" style={{ display: "block" }} />
        </div>
        <button type="submit">Apply Filters</button>
      </form>

      {error && <ErrorBanner message={error} />}
      {isLoading ? <LoadingState /> : <AuditLogTable logs={logs} />}
    </div>
  );
}
