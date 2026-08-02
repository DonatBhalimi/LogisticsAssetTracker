import { useEffect, useState, type FormEvent } from "react";
import * as locationsApi from "../../api/locations";
import { ErrorBanner } from "../../components/ErrorBanner";
import { ApiError } from "../../types/common";
import type { Location } from "../../types/location";
import type { MovementRequest } from "../../types/movement";

// Phase 3 scope (Document 10): location-only movement. Deliberately no status or
// condition inputs — those are Phase 4. Users must not choose sourceType either;
// which endpoint gets called is decided by the parent page, not this form.
interface MovementFormProps {
  currentLocationId: string;
  onSubmit: (request: MovementRequest) => Promise<void>;
}

export function MovementForm({ currentLocationId, onSubmit }: MovementFormProps) {
  const [locations, setLocations] = useState<Location[]>([]);
  const [toLocationId, setToLocationId] = useState("");
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
  const canSubmit = toLocationId !== "" && toLocationId !== currentLocationId;

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!canSubmit) return;
    setError(null);
    setIsSubmitting(true);
    try {
      await onSubmit({ toLocationId, notes: notes.trim() || undefined });
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
          <option value="">{isLoadingLocations ? "Loading locations..." : "Select a location"}</option>
          {destinationOptions.map((l) => (
            <option key={l.id} value={l.id}>
              {l.name}
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
      </div>

      <button type="submit" disabled={!canSubmit || isSubmitting}>
        {isSubmitting ? "Submitting..." : "Submit Movement"}
      </button>
      {!canSubmit && !isLoadingLocations && (
        <p style={{ color: "#666", fontSize: "0.85rem" }}>Select a different location to submit a movement.</p>
      )}
    </form>
  );
}
