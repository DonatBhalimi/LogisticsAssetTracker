import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { QRCodeSVG } from "qrcode.react";
import * as assetsApi from "../api/assets";
import * as movementsApi from "../api/movements";
import { useAuth } from "../auth/useAuth";
import { Badge } from "../components/Badge";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { MovementHistoryTable } from "../features/movements/MovementHistoryTable";
import { ApiError } from "../types/common";
import type { Asset, AssetQr } from "../types/asset";
import type { AssetMovement } from "../types/movement";

export function AssetDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const [asset, setAsset] = useState<Asset | null>(null);
  const [qr, setQr] = useState<AssetQr | null>(null);
  const [movements, setMovements] = useState<AssetMovement[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isDeactivating, setIsDeactivating] = useState(false);

  const canViewQr = user?.role === "Admin" || user?.role === "Manager";

  useEffect(() => {
    if (!id) return;
    setIsLoading(true);
    setError(null);

    const requests: [Promise<Asset>, Promise<AssetQr> | Promise<null>, Promise<AssetMovement[]>] = [
      assetsApi.getAsset(id),
      canViewQr ? assetsApi.getAssetQr(id) : Promise.resolve(null),
      movementsApi.getMovementHistory(id, { pageSize: 50, sortDirection: "desc" }).then((result) => result.items),
    ];

    Promise.all(requests)
      .then(([assetResult, qrResult, movementResult]) => {
        setAsset(assetResult);
        setQr(qrResult);
        setMovements(movementResult);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Unable to load asset."))
      .finally(() => setIsLoading(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, canViewQr]);

  async function handleDeactivate() {
    if (!id) return;
    setIsDeactivating(true);
    setError(null);
    try {
      await assetsApi.deactivateAsset(id);
      const refreshed = await assetsApi.getAsset(id);
      setAsset(refreshed);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to deactivate asset.");
    } finally {
      setIsDeactivating(false);
    }
  }

  if (isLoading) {
    return <LoadingState />;
  }

  if (error && !asset) {
    return <ErrorBanner message={error} />;
  }

  if (!asset) {
    return null;
  }

  return (
    <div>
      <p>
        <Link to="/assets">&larr; Back to Assets</Link>
      </p>
      <h1>{asset.name}</h1>
      {error && <ErrorBanner message={error} />}

      <dl>
        <dt>Asset Code</dt>
        <dd>{asset.assetCode}</dd>
        <dt>Type</dt>
        <dd>{asset.type}</dd>
        <dt>Current Location</dt>
        <dd>{asset.currentLocationName}</dd>
        <dt>Status</dt>
        <dd>
          <Badge text={asset.status} />
        </dd>
        <dt>Condition</dt>
        <dd>
          <Badge text={asset.condition} />
        </dd>
        <dt>Assigned User</dt>
        <dd>{asset.assignedToUserName ?? "Unassigned"}</dd>
        <dt>Active</dt>
        <dd>
          <Badge text={asset.isActive ? "Active" : "Inactive"} />
        </dd>
      </dl>

      {canViewQr && qr && (
        <div style={{ margin: "1.5rem 0" }}>
          <h2>QR Code</h2>
          <QRCodeSVG value={qr.qrUrl} size={160} />
          <p style={{ color: "#666" }}>{qr.qrCodeValue}</p>
        </div>
      )}

      <div style={{ marginTop: "1.5rem" }}>
        {/* Inactive or Retired assets must not show enabled movement actions (Document 07). */}
        {asset.isActive && asset.status !== "Retired" && <Link to={`/assets/${asset.id}/movement`}>Update Movement</Link>}{" "}
        {user?.role === "Admin" && (
          <>
            <Link to={`/assets/${asset.id}/edit`}>Edit Basic Information</Link>{" "}
            {asset.isActive && (
              <button onClick={() => void handleDeactivate()} disabled={isDeactivating}>
                {isDeactivating ? "Deactivating..." : "Deactivate Asset"}
              </button>
            )}
          </>
        )}
      </div>

      <div style={{ marginTop: "2rem" }}>
        <h2>Movement History</h2>
        <MovementHistoryTable movements={movements} />
      </div>

      <p style={{ color: "#666", marginTop: "2rem" }}>Risk recommendations are not yet available (later phases).</p>
    </div>
  );
}
