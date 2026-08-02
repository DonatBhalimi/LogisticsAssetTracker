import type { AssetCondition, AssetStatus } from "./asset";

export type SourceType = "Manual" | "QR";

export interface AssetMovement {
  id: string;
  assetId: string;
  fromLocationId: string;
  fromLocationName: string;
  toLocationId: string;
  toLocationName: string;
  previousStatus: AssetStatus;
  newStatus: AssetStatus;
  previousCondition: AssetCondition;
  newCondition: AssetCondition;
  sourceType: SourceType;
  updatedByUserId: string;
  updatedByUserName: string;
  notes?: string | null;
  // Absent/null for Operators: Document 06 hides suspicious indicators from them.
  isSuspicious?: boolean | null;
  suspiciousReason?: string | null;
  createdAt: string;
}

// Phase 3 scope (Document 10): location-only. No status/condition fields —
// the UI must not offer controls for changes this phase does not support.
export interface MovementRequest {
  toLocationId?: string;
  notes?: string;
}

export interface MovementResult {
  resultType: string;
  movement: AssetMovement;
  asset: import("./asset").Asset;
}

export interface AssetMovementListQuery {
  sourceType?: SourceType;
  isSuspicious?: boolean;
  fromDate?: string;
  toDate?: string;
  updatedByUserId?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}
