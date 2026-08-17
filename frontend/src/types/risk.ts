// Document 04/08: risk detection is rule-based; AI only explains. generationSource
// distinguishes a rule-only fallback from a rule-detected risk with an AI explanation.
export type RiskLevel = "Low" | "Medium" | "High" | "Critical";
export type RiskType = "DelayRisk" | "NoRecentUpdate" | "DamagedAsset" | "LostAsset" | "SuspiciousMovement";
export type GenerationSource = "RuleEngine" | "RuleEngineWithAI";

export interface RiskRecommendation {
  id: string;
  assetId: string;
  assetCode: string;
  assetName: string;
  riskLevel: RiskLevel;
  riskType: RiskType;
  reason: string;
  recommendation: string;
  generationSource: GenerationSource;
  modelName?: string | null;
  generatedByUserId?: string | null;
  generatedByUserName?: string | null;
  createdAt: string;
}

// hasRisk = false means no RiskRecommendation records were created (Document 08).
export interface RiskAnalyzeResponse {
  hasRisk: boolean;
  recommendations: RiskRecommendation[];
}

export interface RiskRecommendationListQuery {
  assetId?: string;
  riskLevel?: RiskLevel;
  riskType?: RiskType;
  generationSource?: GenerationSource;
  generatedByUserId?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}
