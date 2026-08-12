import { apiClient } from "./client";
import type { PagedResult } from "../types/common";
import type { ApprovalDecisionRequest, MovementApproval, MovementApprovalListQuery } from "../types/approval";
import type { MovementResult } from "../types/movement";

export async function listApprovals(query: MovementApprovalListQuery = {}): Promise<PagedResult<MovementApproval>> {
  const response = await apiClient.get<PagedResult<MovementApproval>>("/approvals", { params: query });
  return response.data;
}

export async function listPendingApprovals(query: MovementApprovalListQuery = {}): Promise<PagedResult<MovementApproval>> {
  const response = await apiClient.get<PagedResult<MovementApproval>>("/approvals/pending", { params: query });
  return response.data;
}

// Approving creates the final movement (Document 05), so the response is a MovementResult.
export async function approveMovementRequest(id: string, request: ApprovalDecisionRequest): Promise<MovementResult> {
  const response = await apiClient.post<MovementResult>(`/approvals/${id}/approve`, request);
  return response.data;
}

export async function rejectMovementRequest(id: string, request: ApprovalDecisionRequest): Promise<MovementApproval> {
  const response = await apiClient.post<MovementApproval>(`/approvals/${id}/reject`, request);
  return response.data;
}
