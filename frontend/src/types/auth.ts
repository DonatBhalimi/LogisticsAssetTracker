export type UserRole = "Admin" | "Manager" | "Operator";

export interface UserSummary {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  isActive: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: UserSummary;
}
