import { useEffect, useState, type FormEvent } from "react";
import * as locationsApi from "../../api/locations";
import { ErrorBanner } from "../../components/ErrorBanner";
import { ApiError } from "../../types/common";
import type { Location } from "../../types/location";
import type { ReactivateAssetRequest } from "../../types/movement";

// Manager/Admin dedicated direct Lost reactivation (Document 06/07). Never reachable
// from the normal movement form — sourceType is fixed to Manual by the backend.
interface ReactivateAssetFormProps {
  currentLocationId: string;
  onSubmit: (request: ReactivateAssetRequest) => Promise<void>;
  onCancel: () => void;
}

export function ReactivateAssetForm({ currentLocationId, onSubmit, onCancel }: ReactivateAssetFormProps) {
  const [locations, setLocations] = useState<Location[]>([]);
  const [toLocationId, setToLocationId] = useState("");
  const [decisionNote, setDecisionNote] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoadingLocations, setIsLoadingLocations] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    setIsLoadingLocations(true);
    locationsApi
      .listLocations({ isActive: true, pageSize: 200 })
      .then((result) => setLocations(result.items))
      .catch((err) => setError(err instanceof ApiError ? err.message : "Unable to load locations."))
      .finally(() => setIsLoadingLocations(false));
  }, []);

  const destinationOptions = locations.filter((l) => l.id !== currentLocationId);
  const canSubmit = decisionNote.trim() !== "";

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!canSubmit) return;
    setError(null);
    setIsSubmitting(true);
    try {
      await onSubmit({ toLocationId: toLocationId || undefined, decisionNote: decisionNote.trim() });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to reactivate asset.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={(event) => void handleSubmit(event)} className="form-card" style={{ boxShadow: "inset 3px 0 0 var(--warning-text)" }}>
      <h3>Reactivate Lost Asset</h3>
      {error && <ErrorBanner message={error} />}

      <div className="field">
        <label htmlFor="reactivateLocation">Destination (optional)</label>
        <select
          id="reactivateLocation"
          value={toLocationId}
          onChange={(event) => setToLocationId(event.target.value)}
          disabled={isLoadingLocations}
        >
          <option value="">Keep current location</option>
          {destinationOptions.map((l) => (
            <option key={l.id} value={l.id}>
              {l.name}
            </option>
          ))}
        </select>
      </div>

      <div className="field">
        <label htmlFor="decisionNote">Decision Note (required)</label>
        <textarea id="decisionNote" value={decisionNote} onChange={(event) => setDecisionNote(event.target.value)} rows={3} />
      </div>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={!canSubmit || isSubmitting}>
          {isSubmitting ? "Reactivating..." : "Confirm Reactivation"}
        </button>
        <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </button>
      </div>
    </form>
  );
}
