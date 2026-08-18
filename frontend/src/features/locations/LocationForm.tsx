import { useState, type FormEvent } from "react";
import { ErrorBanner } from "../../components/ErrorBanner";
import { ApiError } from "../../types/common";
import type { LocationType } from "../../types/location";

const LOCATION_TYPES: LocationType[] = [
  "Warehouse",
  "Truck",
  "DistributionCenter",
  "ClientSite",
  "MaintenanceArea",
  "Unknown",
];

export interface LocationFormValues {
  name: string;
  type: LocationType;
  address?: string;
  description?: string;
}

interface LocationFormProps {
  initialValues?: Partial<LocationFormValues>;
  onSubmit: (values: LocationFormValues) => Promise<void>;
  onCancel: () => void;
}

export function LocationForm({ initialValues, onSubmit, onCancel }: LocationFormProps) {
  const [name, setName] = useState(initialValues?.name ?? "");
  const [type, setType] = useState<LocationType>(initialValues?.type ?? LOCATION_TYPES[0]);
  const [address, setAddress] = useState(initialValues?.address ?? "");
  const [description, setDescription] = useState(initialValues?.description ?? "");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await onSubmit({ name, type, address: address || undefined, description: description || undefined });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to save location.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={(event) => void handleSubmit(event)} className="form-card">
      <h3>{initialValues ? "Edit Location" : "Create Location"}</h3>
      {error && <ErrorBanner message={error} />}

      <div className="field">
        <label htmlFor="name">Name</label>
        <input id="name" value={name} onChange={(event) => setName(event.target.value)} required />
      </div>

      <div className="field">
        <label htmlFor="type">Type</label>
        <select id="type" value={type} onChange={(event) => setType(event.target.value as LocationType)}>
          {LOCATION_TYPES.map((t) => (
            <option key={t} value={t}>
              {t}
            </option>
          ))}
        </select>
      </div>

      <div className="field">
        <label htmlFor="address">Address (optional)</label>
        <input id="address" value={address} onChange={(event) => setAddress(event.target.value)} />
      </div>

      <div className="field">
        <label htmlFor="description">Description (optional)</label>
        <input id="description" value={description} onChange={(event) => setDescription(event.target.value)} />
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
