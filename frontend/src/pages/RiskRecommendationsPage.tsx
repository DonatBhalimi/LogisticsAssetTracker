import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import * as riskApi from "../api/risk";
import { Badge } from "../components/Badge";
import { EmptyState } from "../components/EmptyState";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { ApiError } from "../types/common";
import type { GenerationSource, RiskLevel, RiskRecommendation, RiskType } from "../types/risk";

const RISK_LEVELS: (RiskLevel | "")[] = ["", "Low", "Medium", "High", "Critical"];
const RISK_TYPES: (RiskType | "")[] = ["", "DelayRisk", "NoRecentUpdate", "DamagedAsset", "LostAsset", "SuspiciousMovement"];
const SOURCES: (GenerationSource | "")[] = ["", "RuleEngine", "RuleEngineWithAI"];

// Document 07 Screen 11: Admin/Manager review of generated recommendations.
export function RiskRecommendationsPage() {
  const [recommendations, setRecommendations] = useState<RiskRecommendation[]>([]);
  const [riskLevel, setRiskLevel] = useState<RiskLevel | "">("");
  const [riskType, setRiskType] = useState<RiskType | "">("");
  const [generationSource, setGenerationSource] = useState<GenerationSource | "">("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function refresh() {
    setIsLoading(true);
    setError(null);
    try {
      const result = await riskApi.listRiskRecommendations({
        riskLevel: riskLevel || undefined,
        riskType: riskType || undefined,
        generationSource: generationSource || undefined,
        page: 1,
        pageSize: 50,
        sortDirection: "desc",
      });
      setRecommendations(result.items);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to load risk recommendations.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void refresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [riskLevel, riskType, generationSource]);

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Risk Recommendations</h1>
          <p className="page-header-desc">Rule-based risk detections with AI-assisted or fallback explanations.</p>
        </div>
      </div>

      <div className="filters-bar">
        <div className="field">
          <label htmlFor="riskLevel">Risk Level</label>
          <select id="riskLevel" value={riskLevel} onChange={(event) => setRiskLevel(event.target.value as RiskLevel | "")}>
            {RISK_LEVELS.map((l) => (
              <option key={l || "all"} value={l}>
                {l || "All"}
              </option>
            ))}
          </select>
        </div>
        <div className="field">
          <label htmlFor="riskType">Risk Type</label>
          <select id="riskType" value={riskType} onChange={(event) => setRiskType(event.target.value as RiskType | "")}>
            {RISK_TYPES.map((t) => (
              <option key={t || "all"} value={t}>
                {t || "All"}
              </option>
            ))}
          </select>
        </div>
        <div className="field">
          <label htmlFor="generationSource">Source</label>
          <select
            id="generationSource"
            value={generationSource}
            onChange={(event) => setGenerationSource(event.target.value as GenerationSource | "")}
          >
            {SOURCES.map((s) => (
              <option key={s || "all"} value={s}>
                {s || "All"}
              </option>
            ))}
          </select>
        </div>
      </div>

      {error && <ErrorBanner message={error} />}

      {isLoading ? (
        <LoadingState />
      ) : recommendations.length === 0 ? (
        <div className="table-card">
          <EmptyState label="No risk recommendations found." />
        </div>
      ) : (
        <div className="table-card">
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th align="left">Asset</th>
                  <th align="left">Risk Type</th>
                  <th align="left">Risk Level</th>
                  <th align="left">Reason</th>
                  <th align="left">Recommendation</th>
                  <th align="left">Source</th>
                  <th align="left">Generated</th>
                </tr>
              </thead>
              <tbody>
                {recommendations.map((r) => (
                  <tr key={r.id}>
                    <td>
                      <Link to={`/assets/${r.assetId}`}>{r.assetCode}</Link>
                      <div className="cell-muted">{r.assetName}</div>
                    </td>
                    <td>{r.riskType}</td>
                    <td>
                      <Badge text={r.riskLevel} />
                    </td>
                    <td className="cell-muted">{r.reason}</td>
                    <td className="cell-muted">{r.recommendation}</td>
                    <td>
                      <Badge text={r.generationSource} />
                    </td>
                    <td className="cell-muted">{new Date(r.createdAt).toLocaleString()}</td>
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
