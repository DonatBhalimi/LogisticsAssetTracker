import { useEffect, useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import * as assetsApi from "../api/assets";
import * as locationsApi from "../api/locations";
import * as usersApi from "../api/users";
import { ErrorBanner } from "../components/ErrorBanner";
import { ApiError } from "../types/common";
import type { AssetCondition, AssetStatus, AssetType } from "../types/asset";
import type { Location } from "../types/location";
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
const ASSET_STATUSES: AssetStatus[] = ["Available", "InTransit", "Delivered", "Lost"];
const ASSET_CONDITIONS: AssetCondition[] = ["Good", "Damaged", "NeedsInspection"];

export function CreateAssetPage() {
  const navigate = useNavigate();
  const [locations, setLocations] = useState<Location[]>([]);
  const [users, setUsers] = useState<User[]>([]);

  const [name, setName] = useState("");
  const [type, setType] = useState<AssetType>(ASSET_TYPES[0]);
  const [currentLocationId, setCurrentLocationId] = useState("");
  const [status, setStatus] = useState<AssetStatus>("Available");
  const [condition, setCondition] = useState<AssetCondition>("Good");
  const [assignedToUserId, setAssignedToUserId] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    void locationsApi.listLocations({ isActive: true, page: 1, pageSize: 100 }).then((result) => {
      setLocations(result.items);
      if (result.items.length > 0) {
        setCurrentLocationId(result.items[0].id);
      }
    });
    void usersApi.listUsers({ page: 1, pageSize: 100 }).then((result) => setUsers(result.items));
  }, []);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const asset = await assetsApi.createAsset({
        name,
        type,
        currentLocationId,
        status,
        condition,
        assignedToUserId: assignedToUserId || undefined,
      });
      navigate(`/assets/${asset.id}`);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to create asset.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div style={{ maxWidth: 480 }}>
      <h1>Create Asset</h1>
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
          <label htmlFor="location">Initial Location</label>
          <select
            id="location"
            value={currentLocationId}
            onChange={(event) => setCurrentLocationId(event.target.value)}
            required
            style={{ display: "block", width: "100%" }}
          >
            {locations.map((l) => (
              <option key={l.id} value={l.id}>
                {l.name}
              </option>
            ))}
          </select>
        </div>
        <div style={{ marginBottom: "0.75rem" }}>
          <label htmlFor="status">Initial Status</label>
          <select
            id="status"
            value={status}
            onChange={(event) => setStatus(event.target.value as AssetStatus)}
            style={{ display: "block", width: "100%" }}
          >
            {ASSET_STATUSES.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </div>
        <div style={{ marginBottom: "0.75rem" }}>
          <label htmlFor="condition">Initial Condition</label>
          <select
            id="condition"
            value={condition}
            onChange={(event) => setCondition(event.target.value as AssetCondition)}
            style={{ display: "block", width: "100%" }}
          >
            {ASSET_CONDITIONS.map((c) => (
              <option key={c} value={c}>
                {c}
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
        <button type="submit" disabled={isSubmitting || !currentLocationId}>
          {isSubmitting ? "Creating..." : "Create Asset"}
        </button>
      </form>
    </div>
  );
}
