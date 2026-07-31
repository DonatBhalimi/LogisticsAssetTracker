import { apiClient } from "./client";
import type { PagedResult } from "../types/common";
import type {
  Asset,
  AssetListQuery,
  AssetQr,
  CreateAssetRequest,
  UpdateAssetRequest,
} from "../types/asset";

export async function listAssets(query: AssetListQuery): Promise<PagedResult<Asset>> {
  const response = await apiClient.get<PagedResult<Asset>>("/assets", { params: query });
  return response.data;
}

export async function getAsset(id: string): Promise<Asset> {
  const response = await apiClient.get<Asset>(`/assets/${id}`);
  return response.data;
}

export async function createAsset(request: CreateAssetRequest): Promise<Asset> {
  const response = await apiClient.post<Asset>("/assets", request);
  return response.data;
}

export async function updateAsset(id: string, request: UpdateAssetRequest): Promise<Asset> {
  const response = await apiClient.put<Asset>(`/assets/${id}`, request);
  return response.data;
}

export async function deactivateAsset(id: string): Promise<void> {
  await apiClient.patch(`/assets/${id}/deactivate`);
}

export async function getAssetQr(id: string): Promise<AssetQr> {
  const response = await apiClient.get<AssetQr>(`/assets/${id}/qr`);
  return response.data;
}
