import axios, { type AxiosError } from "axios";
import { ApiError, type ApiErrorBody } from "../types/common";

const TOKEN_STORAGE_KEY = "logistics_asset_tracker_token";

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_STORAGE_KEY);
}

export function setStoredToken(token: string): void {
  localStorage.setItem(TOKEN_STORAGE_KEY, token);
}

export function clearStoredToken(): void {
  localStorage.removeItem(TOKEN_STORAGE_KEY);
}

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5080/api",
});

apiClient.interceptors.request.use((config) => {
  const token = getStoredToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorBody>) => {
    if (error.response?.data?.code) {
      return Promise.reject(new ApiError(error.response.status, error.response.data));
    }
    return Promise.reject(
      new ApiError(error.response?.status ?? 0, {
        code: "NETWORK_ERROR",
        message: error.message || "Unable to reach the server.",
      })
    );
  }
);
