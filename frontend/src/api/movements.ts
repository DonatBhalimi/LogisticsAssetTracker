import { apiClient } from "./client";
import type { PagedResult } from "../types/common";
import type { AssetMovementListQuery, AssetMovement, MovementRequest, MovementResult } from "../types/movement";

export async function getMovementHistory(
  assetId: string,
  query: AssetMovementListQuery = {}
): Promise<PagedResult<AssetMovement>> {
  const response = await apiClient.get<PagedResult<AssetMovement>>(`/assets/${assetId}/movements`, {
    params: query,
  });
  return response.data;
}

export async function createManualMovement(assetId: string, request: MovementRequest): Promise<MovementResult> {
  const response = await apiClient.post<MovementResult>(`/assets/${assetId}/movements/manual`, request);
  return response.data;
}

export async function createQrMovement(qrCodeValue: string, request: MovementRequest): Promise<MovementResult> {
  const response = await apiClient.post<MovementResult>(`/assets/qr/${qrCodeValue}/movements`, request);
  return response.data;
}
