import type { AssetCondition, AssetStatus } from "./asset";
import type { SourceType } from "./movement";

export type ApprovalStatus = "Pending" | "Approved" | "Rejected";

// Operator Lost-to-Available reactivation requests (Document 05/06). MVP-only use case.
export interface MovementApproval {
  id: string;
  assetId: string;
  assetCode: string;
  assetName: string;
  requestedByUserId: string;
  requestedByUserName: string;
  decidedByUserId?: string | null;
  decidedByUserName?: string | null;
  requestedLocationId?: string | null;
  requestedLocationName?: string | null;
  requestedStatus: AssetStatus;
  requestedCondition: AssetCondition;
  requestedSourceType: SourceType;
  approvalStatus: ApprovalStatus;
  requestReason: string;
  decisionNote?: string | null;
  createdAt: string;
  decidedAt?: string | null;
}

export interface ApprovalDecisionRequest {
  decisionNote: string;
}

export interface MovementApprovalListQuery {
  status?: ApprovalStatus;
  assetId?: string;
  requestedByUserId?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}
