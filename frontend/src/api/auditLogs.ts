import { apiClient } from "./client";
import type { PagedResult } from "../types/common";
import type { AuditLog, AuditLogListQuery } from "../types/auditLog";

export async function listAuditLogs(query: AuditLogListQuery = {}): Promise<PagedResult<AuditLog>> {
  const response = await apiClient.get<PagedResult<AuditLog>>("/audit-logs", { params: query });
  return response.data;
}
