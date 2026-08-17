import { apiClient } from "./client";
import type { PagedResult } from "../types/common";
import type { AssetGroupCount, DashboardSummary } from "../types/dashboard";
import type { AssetMovement, AssetMovementListQuery } from "../types/movement";

export async function getDashboardSummary(): Promise<DashboardSummary> {
  const response = await apiClient.get<DashboardSummary>("/dashboard/summary");
  return response.data;
}

export async function getAssetsByStatus(): Promise<AssetGroupCount[]> {
  const response = await apiClient.get<AssetGroupCount[]>("/dashboard/assets-by-status");
  return response.data;
}

export async function getAssetsByCondition(): Promise<AssetGroupCount[]> {
  const response = await apiClient.get<AssetGroupCount[]>("/dashboard/assets-by-condition");
  return response.data;
}

export async function getAssetsByLocation(): Promise<AssetGroupCount[]> {
  const response = await apiClient.get<AssetGroupCount[]>("/dashboard/assets-by-location");
  return response.data;
}

export async function getRecentMovements(query: AssetMovementListQuery = {}): Promise<PagedResult<AssetMovement>> {
  const response = await apiClient.get<PagedResult<AssetMovement>>("/dashboard/recent-movements", { params: query });
  return response.data;
}
