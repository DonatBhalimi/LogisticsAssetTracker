import { useEffect, useState } from "react";
import * as auditLogsApi from "../api/auditLogs";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { AuditLogTable } from "../features/auditLogs/AuditLogTable";
import { ApiError } from "../types/common";
import type { AuditLog } from "../types/auditLog";

const EVENT_TYPES = ["", "Asset", "AssetMovement", "MovementApproval"];

// Document 07 Screen 10. Per the approved Phase 4 approach, suspicious activity is not
// a separate backend endpoint — it reuses GET /api/audit-logs filtered to isSuspicious=true.
// Because Document 05 requires every suspicious completed movement (not just blocked
// attempts) to also write an AuditLog entry, this filtered view already combines both,
// satisfying "must combine or normalize suspicious completed movements and blocked attempts."
// Note: a true per-asset filter is not available through this endpoint (entityId means the
// asset for blocked attempts but the movement for completed ones) — that gap is not faked
// here; "Entity Type" doubles as the closest available event-type filter.
export function SuspiciousActivityPage() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [entityType, setEntityType] = useState("");
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
        isSuspicious: true,
        entityType: entityType || undefined,
        userId: userId.trim() || undefined,
        fromDate: fromDate || undefined,
        toDate: toDate || undefined,
        page: 1,
        pageSize: 100,
        sortDirection: "desc",
      });
      setLogs(result.items);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to load suspicious activity.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void refresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div>
      <h1>Suspicious Activity</h1>
      <p style={{ color: "#666" }}>Completed movements and blocked attempts that matched an explicit suspicious rule.</p>

      <form
        onSubmit={(event) => {
          event.preventDefault();
          void refresh();
        }}
        style={{ display: "flex", flexWrap: "wrap", gap: "0.75rem", alignItems: "flex-end", marginBottom: "1rem" }}
      >
        <div>
          <label htmlFor="fromDate">From</label>
          <input id="fromDate" type="date" value={fromDate} onChange={(event) => setFromDate(event.target.value)} style={{ display: "block" }} />
        </div>
        <div>
          <label htmlFor="toDate">To</label>
          <input id="toDate" type="date" value={toDate} onChange={(event) => setToDate(event.target.value)} style={{ display: "block" }} />
        </div>
        <div>
          <label htmlFor="entityType">Event Type</label>
          <select id="entityType" value={entityType} onChange={(event) => setEntityType(event.target.value)} style={{ display: "block" }}>
            {EVENT_TYPES.map((t) => (
              <option key={t || "all"} value={t}>
                {t || "All"}
              </option>
            ))}
          </select>
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
