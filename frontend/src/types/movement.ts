import type { Asset, AssetCondition, AssetStatus } from "./asset";
import type { MovementApproval } from "./approval";

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

// Phase 4 (Document 05): status and condition changes are now supported. sourceType
// remains backend-derived — the UI must never let a user choose it.
export interface MovementRequest {
  toLocationId?: string;
  newStatus?: AssetStatus;
  newCondition?: AssetCondition;
  notes?: string;
}

// resultType is "MovementCreated" (movement/asset populated) or "ApprovalRequired"
// (approval populated, assetUnchanged true, movement/asset stay absent).
export interface MovementResult {
  resultType: string;
  movement?: AssetMovement | null;
  asset?: Asset | null;
  approval?: MovementApproval | null;
  assetUnchanged?: boolean | null;
}

// Manager/Admin direct Lost-to-Available reactivation (Document 06 POST /assets/{id}/reactivate).
export interface ReactivateAssetRequest {
  toLocationId?: string;
  decisionNote: string;
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
