// Document 06 Dashboard Endpoints — counts computed live, never from stored
// RiskRecommendation records.
export interface DashboardSummary {
  totalActiveAssets: number;
  availableAssets: number;
  inTransitAssets: number;
  damagedAssets: number;
  lostAssets: number;
  riskyAssets: number;
  suspiciousActivityCount: number;
  pendingApprovals: number;
}

export interface AssetGroupCount {
  key: string;
  count: number;
}
