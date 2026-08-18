import { useState, type FormEvent } from "react";
import { ErrorBanner } from "../../components/ErrorBanner";
import { ApiError } from "../../types/common";
import type { UserRole } from "../../types/auth";

export interface UserFormValues {
  fullName: string;
  email: string;
  role: UserRole;
  password?: string;
  isActive?: boolean;
}

interface UserFormProps {
  roles: UserRole[];
  initialValues?: Partial<UserFormValues>;
  onSubmit: (values: UserFormValues) => Promise<void>;
  onCancel: () => void;
}

export function UserForm({ roles, initialValues, onSubmit, onCancel }: UserFormProps) {
  const isEditing = Boolean(initialValues);
  const [fullName, setFullName] = useState(initialValues?.fullName ?? "");
  const [email, setEmail] = useState(initialValues?.email ?? "");
  const [role, setRole] = useState<UserRole>(initialValues?.role ?? roles[0]);
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await onSubmit({ fullName, email, role, password: isEditing ? undefined : password });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to save user.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={(event) => void handleSubmit(event)} className="form-card">
      <h3>{isEditing ? "Edit User" : "Create User"}</h3>
      {error && <ErrorBanner message={error} />}

      <div className="field">
        <label htmlFor="fullName">Full Name</label>
        <input id="fullName" value={fullName} onChange={(event) => setFullName(event.target.value)} required />
      </div>

      <div className="field">
        <label htmlFor="email">Email</label>
        <input id="email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} required />
      </div>

      {!isEditing && (
        <div className="field">
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            minLength={8}
          />
        </div>
      )}

      <div className="field">
        <label htmlFor="role">Role</label>
        <select id="role" value={role} onChange={(event) => setRole(event.target.value as UserRole)}>
          {roles.map((r) => (
            <option key={r} value={r}>
              {r}
            </option>
          ))}
        </select>
      </div>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
          {isSubmitting ? "Saving..." : "Save"}
        </button>
        <button type="button" className="btn btn-secondary" onClick={onCancel}>
          Cancel
        </button>
      </div>
    </form>
  );
}
