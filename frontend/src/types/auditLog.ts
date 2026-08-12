// Document 04/06/07: audit entries never carry secrets. metadataJson is a raw JSON
// string captured by the backend for traceability (previous/attempted values, reasons).
export interface AuditLog {
  id: string;
  userId?: string | null;
  userName?: string | null;
  action: string;
  entityType: string;
  entityId?: string | null;
  metadataJson?: string | null;
  createdAt: string;
  isSuspicious: boolean;
  suspiciousReason?: string | null;
}

export interface AuditLogListQuery {
  userId?: string;
  entityType?: string;
  entityId?: string;
  action?: string;
  isSuspicious?: boolean;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}
