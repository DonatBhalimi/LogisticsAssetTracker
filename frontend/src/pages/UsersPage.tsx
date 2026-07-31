import { useEffect, useState } from "react";
import * as usersApi from "../api/users";
import { Badge } from "../components/Badge";
import { EmptyState } from "../components/EmptyState";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { ApiError } from "../types/common";
import type { UserRole } from "../types/auth";
import type { User } from "../types/user";
import { UserForm } from "../features/users/UserForm";

const ROLES: UserRole[] = ["Admin", "Manager", "Operator"];

export function UsersPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isCreating, setIsCreating] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);

  async function refresh() {
    setIsLoading(true);
    setError(null);
    try {
      const result = await usersApi.listUsers({ page: 1, pageSize: 50, sortBy: "fullName" });
      setUsers(result.items);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to load users.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void refresh();
  }, []);

  async function toggleActive(user: User) {
    setError(null);
    try {
      await usersApi.updateUser(user.id, {
        fullName: user.fullName,
        email: user.email,
        role: user.role,
        isActive: !user.isActive,
      });
      await refresh();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to update user.");
    }
  }

  return (
    <div>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <h1>Users</h1>
        <button onClick={() => setIsCreating(true)}>Create User</button>
      </div>

      {error && <ErrorBanner message={error} />}

      {isCreating && (
        <UserForm
          roles={ROLES}
          onCancel={() => setIsCreating(false)}
          onSubmit={async (values) => {
            if (!values.password) {
              throw new Error("Password is required.");
            }
            await usersApi.createUser({
              fullName: values.fullName,
              email: values.email,
              role: values.role,
              password: values.password,
            });
            setIsCreating(false);
            await refresh();
          }}
        />
      )}

      {editingUser && (
        <UserForm
          roles={ROLES}
          initialValues={editingUser}
          onCancel={() => setEditingUser(null)}
          onSubmit={async (values) => {
            await usersApi.updateUser(editingUser.id, {
              fullName: values.fullName,
              email: values.email,
              role: values.role,
              isActive: values.isActive ?? editingUser.isActive,
            });
            setEditingUser(null);
            await refresh();
          }}
        />
      )}

      {isLoading && <LoadingState />}
      {!isLoading && users.length === 0 && <EmptyState />}
      {!isLoading && users.length > 0 && (
        <table style={{ width: "100%", borderCollapse: "collapse", marginTop: "1rem" }}>
          <thead>
            <tr>
              <th align="left">Full Name</th>
              <th align="left">Email</th>
              <th align="left">Role</th>
              <th align="left">Status</th>
              <th align="left">Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id} style={{ borderTop: "1px solid #eee" }}>
                <td>{user.fullName}</td>
                <td>{user.email}</td>
                <td>{user.role}</td>
                <td>
                  <Badge text={user.isActive ? "Active" : "Inactive"} />
                </td>
                <td>
                  <button onClick={() => setEditingUser(user)}>Edit</button>{" "}
                  <button onClick={() => void toggleActive(user)}>
                    {user.isActive ? "Deactivate" : "Activate"}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
