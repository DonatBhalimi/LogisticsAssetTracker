import { apiClient } from "./client";
import type { PagedResult } from "../types/common";
import type {
  Asset,
  AssetListQuery,
  AssetQr,
  CreateAssetRequest,
  UpdateAssetRequest,
} from "../types/asset";
import type { MovementResult, ReactivateAssetRequest } from "../types/movement";

export async function listAssets(query: AssetListQuery): Promise<PagedResult<Asset>> {
  const response = await apiClient.get<PagedResult<Asset>>("/assets", { params: query });
  return response.data;
}

export async function getAsset(id: string): Promise<Asset> {
  const response = await apiClient.get<Asset>(`/assets/${id}`);
  return response.data;
}

export async function getAssetByCode(assetCode: string): Promise<Asset> {
  const response = await apiClient.get<Asset>(`/assets/by-code/${encodeURIComponent(assetCode)}`);
  return response.data;
}

export async function getAssetByQr(qrCodeValue: string): Promise<Asset> {
  const response = await apiClient.get<Asset>(`/assets/qr/${encodeURIComponent(qrCodeValue)}`);
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

// Manager/Admin dedicated direct Lost reactivation (Document 06) — never routed
// through the normal movement form.
export async function reactivateAsset(id: string, request: ReactivateAssetRequest): Promise<MovementResult> {
  const response = await apiClient.post<MovementResult>(`/assets/${id}/reactivate`, request);
  return response.data;
}
