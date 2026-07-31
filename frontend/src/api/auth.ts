import { apiClient } from "./client";
import type { LoginRequest, LoginResponse, UserSummary } from "../types/auth";

export async function login(request: LoginRequest): Promise<LoginResponse> {
  const response = await apiClient.post<LoginResponse>("/auth/login", request);
  return response.data;
}

export async function getCurrentUser(): Promise<UserSummary> {
  const response = await apiClient.get<UserSummary>("/auth/me");
  return response.data;
}

export async function logout(): Promise<void> {
  await apiClient.post("/auth/logout");
}
