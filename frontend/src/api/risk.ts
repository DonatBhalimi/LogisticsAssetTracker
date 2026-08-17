import { apiClient } from "./client";
import type { PagedResult } from "../types/common";
import type { RiskAnalyzeResponse, RiskRecommendation, RiskRecommendationListQuery } from "../types/risk";

export async function analyzeAssetRisk(assetId: string): Promise<RiskAnalyzeResponse> {
  const response = await apiClient.post<RiskAnalyzeResponse>(`/assets/${assetId}/risk/analyze`);
  return response.data;
}

export async function getAssetRiskRecommendations(
  assetId: string,
  query: RiskRecommendationListQuery = {}
): Promise<PagedResult<RiskRecommendation>> {
  const response = await apiClient.get<PagedResult<RiskRecommendation>>(`/assets/${assetId}/risk-recommendations`, {
    params: query,
  });
  return response.data;
}

export async function listRiskRecommendations(query: RiskRecommendationListQuery = {}): Promise<PagedResult<RiskRecommendation>> {
  const response = await apiClient.get<PagedResult<RiskRecommendation>>("/risk-recommendations", { params: query });
  return response.data;
}
