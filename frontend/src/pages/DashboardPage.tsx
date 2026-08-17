import { useEffect, useState, type ReactNode } from "react";
import { Link } from "react-router-dom";
import { Bar, BarChart, CartesianGrid, Cell, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import * as dashboardApi from "../api/dashboard";
import * as auditLogsApi from "../api/auditLogs";
import * as approvalsApi from "../api/approvals";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { EmptyState } from "../components/EmptyState";
import { Badge } from "../components/Badge";
import { MovementHistoryTable } from "../features/movements/MovementHistoryTable";
import { AuditLogTable } from "../features/auditLogs/AuditLogTable";
import { ApiError } from "../types/common";
import type { AssetGroupCount, DashboardSummary } from "../types/dashboard";
import type { AssetMovement } from "../types/movement";
import type { AuditLog } from "../types/auditLog";
import type { MovementApproval } from "../types/approval";

// Chart colors reuse the exact same soft-tint palette as Badge.tsx, so a status/condition
// looks the same whether shown as a badge or a chart segment (Document 07: not colorful,
// enterprise-consistent).
const STATUS_COLORS: Record<string, string> = {
  Available: "#027a48",
  InTransit: "#175cd3",
  Delivered: "#5925dc",
  Lost: "#b42318",
  Retired: "#344054",
};

const CONDITION_COLORS: Record<string, string> = {
  Good: "#027a48",
  Damaged: "#b42318",
  NeedsInspection: "#b54708",
};

// Document 07 Screen 2: operational overview for Admin/Manager. Suspicious activity and
// pending approvals sections reuse the existing GET /api/audit-logs and GET
// /api/approvals/pending endpoints (Phase 4 data) rather than dedicated dashboard
// endpoints, per the approved Phase 5 scope.
export function DashboardPage() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [byStatus, setByStatus] = useState<AssetGroupCount[]>([]);
  const [byCondition, setByCondition] = useState<AssetGroupCount[]>([]);
  const [byLocation, setByLocation] = useState<AssetGroupCount[]>([]);
  const [recentMovements, setRecentMovements] = useState<AssetMovement[]>([]);
  const [suspiciousActivity, setSuspiciousActivity] = useState<AuditLog[]>([]);
  const [pendingApprovals, setPendingApprovals] = useState<MovementApproval[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setIsLoading(true);
    setError(null);

    Promise.all([
      dashboardApi.getDashboardSummary(),
      dashboardApi.getAssetsByStatus(),
      dashboardApi.getAssetsByCondition(),
      dashboardApi.getAssetsByLocation(),
      dashboardApi.getRecentMovements({ pageSize: 10, sortDirection: "desc" }),
      auditLogsApi.listAuditLogs({ isSuspicious: true, pageSize: 5, sortDirection: "desc" }),
      approvalsApi.listPendingApprovals({ pageSize: 5, sortDirection: "desc" }),
    ])
      .then(([summaryResult, statusResult, conditionResult, locationResult, movementsResult, suspiciousResult, approvalsResult]) => {
        setSummary(summaryResult);
        setByStatus(statusResult);
        setByCondition(conditionResult);
        setByLocation(locationResult);
        setRecentMovements(movementsResult.items);
        setSuspiciousActivity(suspiciousResult.items);
        setPendingApprovals(approvalsResult.items);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Unable to load dashboard."))
      .finally(() => setIsLoading(false));
  }, []);

  if (isLoading) {
    return <LoadingState />;
  }

  if (error && !summary) {
    return <ErrorBanner message={error} />;
  }

  if (!summary) {
    return null;
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Dashboard</h1>
          <p className="page-header-desc">Operational overview of active assets, risk, and pending work.</p>
        </div>
      </div>

      {error && <ErrorBanner message={error} />}

      <div className="stat-grid">
        <StatCard label="Active Assets" value={summary.totalActiveAssets} />
        <StatCard label="Available" value={summary.availableAssets} />
        <StatCard label="In Transit" value={summary.inTransitAssets} />
        <StatCard label="Damaged" value={summary.damagedAssets} tone={summary.damagedAssets > 0 ? "warning" : undefined} />
        <StatCard label="Lost" value={summary.lostAssets} tone={summary.lostAssets > 0 ? "danger" : undefined} />
        <StatCard label="Risky Assets" value={summary.riskyAssets} tone={summary.riskyAssets > 0 ? "warning" : undefined} />
        <StatCard
          label="Suspicious Activity"
          value={summary.suspiciousActivityCount}
          tone={summary.suspiciousActivityCount > 0 ? "danger" : undefined}
        />
        <StatCard label="Pending Approvals" value={summary.pendingApprovals} tone={summary.pendingApprovals > 0 ? "warning" : undefined} />
      </div>

      <div className="chart-grid">
        <ChartCard title="Assets by Status">
          {byStatus.length === 0 ? (
            <EmptyState label="No active assets." />
          ) : (
            <ResponsiveContainer width="100%" height={220}>
              <PieChart>
                <Pie data={byStatus} dataKey="count" nameKey="key" innerRadius={45} outerRadius={80}>
                  {byStatus.map((entry) => (
                    <Cell key={entry.key} fill={STATUS_COLORS[entry.key] ?? "#344054"} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          )}
          <ChartLegend items={byStatus} colors={STATUS_COLORS} />
        </ChartCard>

        <ChartCard title="Assets by Condition">
          {byCondition.length === 0 ? (
            <EmptyState label="No active assets." />
          ) : (
            <ResponsiveContainer width="100%" height={220}>
              <PieChart>
                <Pie data={byCondition} dataKey="count" nameKey="key" innerRadius={45} outerRadius={80}>
                  {byCondition.map((entry) => (
                    <Cell key={entry.key} fill={CONDITION_COLORS[entry.key] ?? "#344054"} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          )}
          <ChartLegend items={byCondition} colors={CONDITION_COLORS} />
        </ChartCard>

        <ChartCard title="Assets by Location">
          {byLocation.length === 0 ? (
            <EmptyState label="No active assets." />
          ) : (
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={byLocation}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                <XAxis dataKey="key" tick={{ fontSize: 11 }} interval={0} angle={-20} textAnchor="end" height={60} />
                <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
                <Tooltip />
                <Bar dataKey="count" fill="#14161f" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          )}
        </ChartCard>
      </div>

      <div className="section-header">
        <h2>Recent Movements</h2>
      </div>
      <MovementHistoryTable movements={recentMovements} />

      <div className="section-header">
        <h2>Suspicious Activity</h2>
        <Link to="/suspicious-activity">View all</Link>
      </div>
      <AuditLogTable logs={suspiciousActivity} />

      <div className="section-header">
        <h2>Pending Approvals</h2>
        <Link to="/approvals">View all</Link>
      </div>
      {pendingApprovals.length === 0 ? (
        <div className="table-card">
          <EmptyState label="No pending approvals." />
        </div>
      ) : (
        <div className="table-card">
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th align="left">Asset</th>
                  <th align="left">Requested By</th>
                  <th align="left">Source</th>
                  <th align="left">Requested At</th>
                </tr>
              </thead>
              <tbody>
                {pendingApprovals.map((approval) => (
                  <tr key={approval.id}>
                    <td>
                      <Link to={`/assets/${approval.assetId}`}>{approval.assetCode}</Link>
                    </td>
                    <td className="cell-muted">{approval.requestedByUserName}</td>
                    <td>
                      <Badge text={approval.requestedSourceType} />
                    </td>
                    <td className="cell-muted">{new Date(approval.createdAt).toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

function StatCard({ label, value, tone }: { label: string; value: number; tone?: "danger" | "warning" }) {
  return (
    <div className={`stat-card${tone ? ` tone-${tone}` : ""}`}>
      <div className="stat-card-label">{label}</div>
      <div className="stat-card-value">{value}</div>
    </div>
  );
}

function ChartCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div className="card card-padded">
      <h3 style={{ marginBottom: "12px" }}>{title}</h3>
      {children}
    </div>
  );
}

function ChartLegend({ items, colors }: { items: AssetGroupCount[]; colors: Record<string, string> }) {
  if (items.length === 0) return null;
  return (
    <div style={{ display: "flex", flexWrap: "wrap", gap: "10px", marginTop: "10px" }}>
      {items.map((item) => (
        <div key={item.key} style={{ display: "flex", alignItems: "center", gap: "6px", fontSize: "12.5px", color: "var(--text-muted)" }}>
          <span
            style={{
              display: "inline-block",
              width: 9,
              height: 9,
              borderRadius: 2,
              background: colors[item.key] ?? "#344054",
            }}
          />
          {item.key} ({item.count})
        </div>
      ))}
    </div>
  );
}
