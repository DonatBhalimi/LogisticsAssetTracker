export type AssetType =
  | "Pallet"
  | "Crate"
  | "Box"
  | "Container"
  | "Tool"
  | "Device"
  | "WarehouseEquipment";

export type AssetStatus = "Available" | "InTransit" | "Delivered" | "Lost" | "Retired";

export type AssetCondition = "Good" | "Damaged" | "NeedsInspection";

export interface Asset {
  id: string;
  assetCode: string;
  name: string;
  type: AssetType;
  currentLocationId: string;
  currentLocationName: string;
  status: AssetStatus;
  condition: AssetCondition;
  assignedToUserId?: string | null;
  assignedToUserName?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateAssetRequest {
  name: string;
  type: AssetType;
  currentLocationId: string;
  status: AssetStatus;
  condition: AssetCondition;
  assignedToUserId?: string;
}

export interface UpdateAssetRequest {
  name: string;
  type: AssetType;
  assignedToUserId?: string;
}

export interface AssetListQuery {
  status?: AssetStatus;
  condition?: AssetCondition;
  locationId?: string;
  type?: AssetType;
  search?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}

export interface AssetQr {
  assetId: string;
  assetCode: string;
  qrCodeValue: string;
  qrUrl: string;
}
