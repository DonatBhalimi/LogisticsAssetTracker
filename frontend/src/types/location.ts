export type LocationType =
  | "Warehouse"
  | "Truck"
  | "DistributionCenter"
  | "ClientSite"
  | "MaintenanceArea"
  | "Unknown";

export interface Location {
  id: string;
  name: string;
  type: LocationType;
  address?: string | null;
  description?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLocationRequest {
  name: string;
  type: LocationType;
  address?: string;
  description?: string;
}

export interface UpdateLocationRequest {
  name: string;
  type: LocationType;
  address?: string;
  description?: string;
}

export interface LocationListQuery {
  type?: LocationType;
  isActive?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}
