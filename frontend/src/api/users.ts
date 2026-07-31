import { apiClient } from "./client";
import type { PagedResult } from "../types/common";
import type { CreateUserRequest, UpdateUserRequest, User, UserListQuery } from "../types/user";

export async function listUsers(query: UserListQuery): Promise<PagedResult<User>> {
  const response = await apiClient.get<PagedResult<User>>("/users", { params: query });
  return response.data;
}

export async function createUser(request: CreateUserRequest): Promise<User> {
  const response = await apiClient.post<User>("/users", request);
  return response.data;
}

export async function updateUser(id: string, request: UpdateUserRequest): Promise<User> {
  const response = await apiClient.put<User>(`/users/${id}`, request);
  return response.data;
}
