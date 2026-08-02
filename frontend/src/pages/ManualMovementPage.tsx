import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import * as assetsApi from "../api/assets";
import * as movementsApi from "../api/movements";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { MovementUpdateView } from "../features/movements/MovementUpdateView";
import { ApiError } from "../types/common";
import type { Asset } from "../types/asset";

// Route: /assets/:id/movement (Document 06 manual movement endpoint).
// Reached from Asset Details "Update Movement" and from the QR Scanner's
// manual asset-code fallback (Document 05: fallback uses the manual flow).
export function ManualMovementPage() {
  const { id } = useParams<{ id: string }>();
  const [asset, setAsset] = useState<Asset | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    setIsLoading(true);
    assetsApi
      .getAsset(id)
      .then(setAsset)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Unable to load asset."))
      .finally(() => setIsLoading(false));
  }, [id]);

  if (isLoading) return <LoadingState />;
  if (error && !asset) return <ErrorBanner message={error} />;
  if (!asset || !id) return null;

  return (
    <div>
      <p>
        <Link to={`/assets/${id}`}>&larr; Back to Asset</Link>
      </p>
      {error && <ErrorBanner message={error} />}
      <MovementUpdateView asset={asset} onSubmitMovement={(request) => movementsApi.createManualMovement(id, request)} />
    </div>
  );
}
