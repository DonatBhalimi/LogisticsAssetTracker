import { useEffect, useState } from "react";
import * as locationsApi from "../api/locations";
import { Badge } from "../components/Badge";
import { EmptyState } from "../components/EmptyState";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { ApiError } from "../types/common";
import type { Location } from "../types/location";
import { LocationForm } from "../features/locations/LocationForm";

export function LocationsPage() {
  const [locations, setLocations] = useState<Location[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isCreating, setIsCreating] = useState(false);
  const [editingLocation, setEditingLocation] = useState<Location | null>(null);

  async function refresh() {
    setIsLoading(true);
    setError(null);
    try {
      const result = await locationsApi.listLocations({ page: 1, pageSize: 50, sortBy: "name" });
      setLocations(result.items);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to load locations.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void refresh();
  }, []);

  async function handleDeactivate(location: Location) {
    setError(null);
    try {
      await locationsApi.deactivateLocation(location.id);
      await refresh();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to deactivate location.");
    }
  }

  return (
    <div>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <h1>Locations</h1>
        <button onClick={() => setIsCreating(true)}>Create Location</button>
      </div>

      {error && <ErrorBanner message={error} />}

      {isCreating && (
        <LocationForm
          onCancel={() => setIsCreating(false)}
          onSubmit={async (values) => {
            await locationsApi.createLocation(values);
            setIsCreating(false);
            await refresh();
          }}
        />
      )}

      {editingLocation && (
        <LocationForm
          initialValues={{
            name: editingLocation.name,
            type: editingLocation.type,
            address: editingLocation.address ?? undefined,
            description: editingLocation.description ?? undefined,
          }}
          onCancel={() => setEditingLocation(null)}
          onSubmit={async (values) => {
            await locationsApi.updateLocation(editingLocation.id, values);
            setEditingLocation(null);
            await refresh();
          }}
        />
      )}

      {isLoading && <LoadingState />}
      {!isLoading && locations.length === 0 && <EmptyState />}
      {!isLoading && locations.length > 0 && (
        <table style={{ width: "100%", borderCollapse: "collapse", marginTop: "1rem" }}>
          <thead>
            <tr>
              <th align="left">Name</th>
              <th align="left">Type</th>
              <th align="left">Address</th>
              <th align="left">Status</th>
              <th align="left">Actions</th>
            </tr>
          </thead>
          <tbody>
            {locations.map((location) => (
              <tr key={location.id} style={{ borderTop: "1px solid #eee" }}>
                <td>{location.name}</td>
                <td>{location.type}</td>
                <td>{location.address ?? "-"}</td>
                <td>
                  <Badge text={location.isActive ? "Active" : "Inactive"} />
                </td>
                <td>
                  <button onClick={() => setEditingLocation(location)}>Edit</button>{" "}
                  {location.isActive && (
                    <button onClick={() => void handleDeactivate(location)}>Deactivate</button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
