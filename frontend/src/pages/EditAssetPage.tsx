import { useEffect, useState, type FormEvent } from "react";
import { useNavigate, useParams } from "react-router-dom";
import * as assetsApi from "../api/assets";
import * as usersApi from "../api/users";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { ApiError } from "../types/common";
import type { AssetType } from "../types/asset";
import type { User } from "../types/user";

const ASSET_TYPES: AssetType[] = [
  "Pallet",
  "Crate",
  "Box",
  "Container",
  "Tool",
  "Device",
  "WarehouseEquipment",
];

// Edit Asset intentionally cannot change location, status, condition, assetCode, or
// qrCodeValue (Document 04/06/07 edit boundary) — those change only via the movement flow.
export function EditAssetPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [users, setUsers] = useState<User[]>([]);
  const [name, setName] = useState("");
  const [type, setType] = useState<AssetType>(ASSET_TYPES[0]);
  const [assignedToUserId, setAssignedToUserId] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (!id) return;
    void Promise.all([assetsApi.getAsset(id), usersApi.listUsers({ page: 1, pageSize: 100 })])
      .then(([asset, userList]) => {
        setName(asset.name);
        setType(asset.type);
        setAssignedToUserId(asset.assignedToUserId ?? "");
        setUsers(userList.items);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Unable to load asset."))
      .finally(() => setIsLoading(false));
  }, [id]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!id) return;
    setError(null);
    setIsSubmitting(true);
    try {
      await assetsApi.updateAsset(id, { name, type, assignedToUserId: assignedToUserId || undefined });
      navigate(`/assets/${id}`);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to update asset.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isLoading) {
    return <LoadingState />;
  }

  return (
    <div style={{ maxWidth: 480 }}>
      <h1>Edit Asset</h1>
      <p style={{ color: "#666" }}>Location, status, condition, and QR code can only change through a movement update (Phase 3).</p>
      {error && <ErrorBanner message={error} />}
      <form onSubmit={(event) => void handleSubmit(event)}>
        <div style={{ marginBottom: "0.75rem" }}>
          <label htmlFor="name">Name</label>
          <input
            id="name"
            value={name}
            onChange={(event) => setName(event.target.value)}
            required
            style={{ display: "block", width: "100%" }}
          />
        </div>
        <div style={{ marginBottom: "0.75rem" }}>
          <label htmlFor="type">Type</label>
          <select
            id="type"
            value={type}
            onChange={(event) => setType(event.target.value as AssetType)}
            style={{ display: "block", width: "100%" }}
          >
            {ASSET_TYPES.map((t) => (
              <option key={t} value={t}>
                {t}
              </option>
            ))}
          </select>
        </div>
        <div style={{ marginBottom: "0.75rem" }}>
          <label htmlFor="assignedTo">Assigned User (optional)</label>
          <select
            id="assignedTo"
            value={assignedToUserId}
            onChange={(event) => setAssignedToUserId(event.target.value)}
            style={{ display: "block", width: "100%" }}
          >
            <option value="">Unassigned</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.fullName}
              </option>
            ))}
          </select>
        </div>
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Saving..." : "Save"}
        </button>
      </form>
    </div>
  );
}
