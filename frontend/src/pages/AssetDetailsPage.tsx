import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { QRCodeSVG } from "qrcode.react";
import * as assetsApi from "../api/assets";
import { useAuth } from "../auth/useAuth";
import { Badge } from "../components/Badge";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { ApiError } from "../types/common";
import type { Asset, AssetQr } from "../types/asset";

export function AssetDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const [asset, setAsset] = useState<Asset | null>(null);
  const [qr, setQr] = useState<AssetQr | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isDeactivating, setIsDeactivating] = useState(false);

  const canViewQr = user?.role === "Admin" || user?.role === "Manager";

  useEffect(() => {
    if (!id) return;
    setIsLoading(true);
    setError(null);

    const requests: [Promise<Asset>, Promise<AssetQr> | Promise<null>] = [
      assetsApi.getAsset(id),
      canViewQr ? assetsApi.getAssetQr(id) : Promise.resolve(null),
    ];

    Promise.all(requests)
      .then(([assetResult, qrResult]) => {
        setAsset(assetResult);
        setQr(qrResult);
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

      {user?.role === "Admin" && (
        <div style={{ marginTop: "1.5rem" }}>
          <Link to={`/assets/${asset.id}/edit`}>Edit Basic Information</Link>{" "}
          {asset.isActive && (
            <button onClick={() => void handleDeactivate()} disabled={isDeactivating}>
              {isDeactivating ? "Deactivating..." : "Deactivate Asset"}
            </button>
          )}
        </div>
      )}

      <p style={{ color: "#666", marginTop: "2rem" }}>
        Movement history, movement updates, and risk recommendations are not yet available (later phases).
      </p>
    </div>
  );
}
