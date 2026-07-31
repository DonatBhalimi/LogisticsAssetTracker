import { apiClient } from "./client";
import type { PagedResult } from "../types/common";
import type {
  CreateLocationRequest,
  Location,
  LocationListQuery,
  UpdateLocationRequest,
} from "../types/location";

export async function listLocations(query: LocationListQuery): Promise<PagedResult<Location>> {
  const response = await apiClient.get<PagedResult<Location>>("/locations", { params: query });
  return response.data;
}

export async function createLocation(request: CreateLocationRequest): Promise<Location> {
  const response = await apiClient.post<Location>("/locations", request);
  return response.data;
}

export async function updateLocation(id: string, request: UpdateLocationRequest): Promise<Location> {
  const response = await apiClient.put<Location>(`/locations/${id}`, request);
  return response.data;
}

export async function deactivateLocation(id: string): Promise<void> {
  await apiClient.patch(`/locations/${id}/deactivate`);
}
