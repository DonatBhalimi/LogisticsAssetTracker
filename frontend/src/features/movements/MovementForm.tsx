import { useEffect, useState, type FormEvent } from "react";
import * as locationsApi from "../../api/locations";
import { useAuth } from "../../auth/useAuth";
import { ErrorBanner } from "../../components/ErrorBanner";
import { ApiError } from "../../types/common";
import type { AssetCondition, AssetStatus } from "../../types/asset";
import type { Location } from "../../types/location";
import type { MovementRequest, SourceType } from "../../types/movement";

const ALL_STATUSES: AssetStatus[] = ["Available", "InTransit", "Delivered", "Lost", "Retired"];
const ALL_CONDITIONS: AssetCondition[] = ["Good", "Damaged", "NeedsInspection"];

// Phase 4 scope (Document 05/06). Users must not choose sourceType — sourceType is
// decided by the parent page (which endpoint it calls), not by this form.
interface MovementFormProps {
  currentLocationId: string;
  currentStatus: AssetStatus;
  currentCondition: AssetCondition;
  sourceType: SourceType;
  onSubmit: (request: MovementRequest) => Promise<void>;
}

export function MovementForm({ currentLocationId, currentStatus, currentCondition, sourceType, onSubmit }: MovementFormProps) {
  const { user } = useAuth();
  const [locations, setLocations] = useState<Location[]>([]);
  const [toLocationId, setToLocationId] = useState("");
  const [newStatus, setNewStatus] = useState<AssetStatus | "">("");
  const [newCondition, setNewCondition] = useState<AssetCondition | "">("");
  const [notes, setNotes] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoadingLocations, setIsLoadingLocations] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    setIsLoadingLocations(true);
    // Inactive locations can never be a valid movement destination (Document 05/06),
    // so the destination list only ever offers active locations.
    locationsApi
      .listLocations({ isActive: true, pageSize: 200 })
      .then((result) => setLocations(result.items))
      .catch((err) => setError(err instanceof ApiError ? err.message : "Unable to load locations."))
      .finally(() => setIsLoadingLocations(false));
  }, []);

  const destinationOptions = locations.filter((l) => l.id !== currentLocationId);

  // Option lists mirror documented backend restrictions only (Document 05/06) — the
  // backend remains the source of truth and returns the authoritative error either way.
  const statusOptions = ALL_STATUSES.filter((s) => {
    if (s === currentStatus) return false;
    if (s === "Retired") {
      // Lost-to-Retired: Admin-only, manual flow only, and only from Lost.
      return currentStatus === "Lost" && user?.role === "Admin" && sourceType === "Manual";
    }
    if (s === "Available" && currentStatus === "Lost") {
      // Manager/Admin must use the dedicated Reactivate Lost Asset action instead.
      return user?.role === "Operator";
    }
    return true;
  });

  const conditionOptions = ALL_CONDITIONS.filter((c) => {
    if (c === currentCondition) return false;
    if (c === "Good") {
      // Inspection clearance: Manager/Admin only, manual flow only.
      return user?.role !== "Operator" && sourceType === "Manual";
    }
    return true;
  });

  const canSubmit = toLocationId !== "" || newStatus !== "" || newCondition !== "";

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!canSubmit) return;
    setError(null);
    setIsSubmitting(true);
    try {
      await onSubmit({
        toLocationId: toLocationId || undefined,
        newStatus: newStatus || undefined,
        newCondition: newCondition || undefined,
        notes: notes.trim() || undefined,
      });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to submit movement.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={(event) => void handleSubmit(event)} style={{ border: "1px solid #ddd", padding: "1rem", maxWidth: 480 }}>
      <h3>Update Movement</h3>
      {error && <ErrorBanner message={error} />}

      <div style={{ marginBottom: "0.75rem" }}>
        <label htmlFor="toLocationId">New Location</label>
        <select
          id="toLocationId"
          value={toLocationId}
          onChange={(event) => setToLocationId(event.target.value)}
          disabled={isLoadingLocations}
          style={{ display: "block", width: "100%" }}
        >
          <option value="">{isLoadingLocations ? "Loading locations..." : "No change"}</option>
          {destinationOptions.map((l) => (
            <option key={l.id} value={l.id}>
              {l.name}
            </option>
          ))}
        </select>
      </div>

      <div style={{ marginBottom: "0.75rem" }}>
        <label htmlFor="newStatus">New Status</label>
        <select
          id="newStatus"
          value={newStatus}
          onChange={(event) => setNewStatus(event.target.value as AssetStatus | "")}
          style={{ display: "block", width: "100%" }}
        >
          <option value="">No change</option>
          {statusOptions.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
      </div>

      <div style={{ marginBottom: "0.75rem" }}>
        <label htmlFor="newCondition">New Condition</label>
        <select
          id="newCondition"
          value={newCondition}
          onChange={(event) => setNewCondition(event.target.value as AssetCondition | "")}
          style={{ display: "block", width: "100%" }}
        >
          <option value="">No change</option>
          {conditionOptions.map((c) => (
            <option key={c} value={c}>
              {c}
            </option>
          ))}
        </select>
      </div>

      <div style={{ marginBottom: "0.75rem" }}>
        <label htmlFor="notes">Notes (optional)</label>
        <textarea
          id="notes"
          value={notes}
          onChange={(event) => setNotes(event.target.value)}
          rows={3}
          style={{ display: "block", width: "100%" }}
        />
        <p style={{ color: "#666", fontSize: "0.8rem" }}>
          Notes are required for some transitions (Lost reactivation requests, inspection clearance, Lost-to-Retired) — the
          backend will report if a note is missing.
        </p>
      </div>

      <button type="submit" disabled={!canSubmit || isSubmitting}>
        {isSubmitting ? "Submitting..." : "Submit Movement"}
      </button>
      {!canSubmit && !isLoadingLocations && (
        <p style={{ color: "#666", fontSize: "0.85rem" }}>Change the location, status, or condition to submit a movement.</p>
      )}
    </form>
  );
}
