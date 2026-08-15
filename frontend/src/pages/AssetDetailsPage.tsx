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
import { ReactivateAssetForm } from "../features/assets/ReactivateAssetForm";
import { ApiError } from "../types/common";
import type { Asset, AssetQr } from "../types/asset";
import type { AssetMovement, ReactivateAssetRequest } from "../types/movement";

export function AssetDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const [asset, setAsset] = useState<Asset | null>(null);
  const [qr, setQr] = useState<AssetQr | null>(null);
  const [movements, setMovements] = useState<AssetMovement[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isDeactivating, setIsDeactivating] = useState(false);
  const [showReactivateForm, setShowReactivateForm] = useState(false);

  const canViewQr = user?.role === "Admin" || user?.role === "Manager";
  const canReactivate = user?.role === "Admin" || user?.role === "Manager";

  async function loadAssetData() {
    if (!id) return;
    setIsLoading(true);
    setError(null);

    const requests: [Promise<Asset>, Promise<AssetQr> | Promise<null>, Promise<AssetMovement[]>] = [
      assetsApi.getAsset(id),
      canViewQr ? assetsApi.getAssetQr(id) : Promise.resolve(null),
      movementsApi.getMovementHistory(id, { pageSize: 50, sortDirection: "desc" }).then((result) => result.items),
    ];

    return Promise.all(requests)
      .then(([assetResult, qrResult, movementResult]) => {
        setAsset(assetResult);
        setQr(qrResult);
        setMovements(movementResult);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Unable to load asset."))
      .finally(() => setIsLoading(false));
  }

  useEffect(() => {
    void loadAssetData();
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

  async function handleReactivate(request: ReactivateAssetRequest) {
    if (!id) return;
    setError(null);
    await assetsApi.reactivateAsset(id, request);
    setShowReactivateForm(false);
    await loadAssetData();
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
      <p style={{ marginBottom: "12px" }}>
        <Link to="/assets">&larr; Back to Assets</Link>
      </p>

      <div className="page-header">
        <div>
          <h1>{asset.name}</h1>
          <p className="page-header-desc">{asset.assetCode}</p>
        </div>
        <div className="page-header-actions">
          {/* Inactive or Retired assets must not show enabled movement actions (Document 07). */}
          {asset.isActive && asset.status !== "Retired" && (
            <Link to={`/assets/${asset.id}/movement`} className="btn btn-primary">
              Update Movement
            </Link>
          )}
          {/* Manager/Admin direct Lost reactivation uses this dedicated action, never the normal movement form (Document 07). */}
          {canReactivate && asset.isActive && asset.status === "Lost" && (
            <button className="btn btn-secondary" onClick={() => setShowReactivateForm((v) => !v)}>
              {showReactivateForm ? "Cancel Reactivation" : "Reactivate Lost Asset"}
            </button>
          )}
          {user?.role === "Admin" && (
            <>
              <Link to={`/assets/${asset.id}/edit`} className="btn btn-secondary">
                Edit
              </Link>
              {asset.isActive && (
                <button className="btn btn-danger" onClick={() => void handleDeactivate()} disabled={isDeactivating}>
                  {isDeactivating ? "Deactivating..." : "Deactivate"}
                </button>
              )}
            </>
          )}
        </div>
      </div>

      {error && <ErrorBanner message={error} />}

      <div className="card card-padded">
        <dl className="detail-grid">
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
          <div style={{ marginTop: "24px", paddingTop: "24px", borderTop: "1px solid var(--border)" }}>
            <div className="section-label">QR Code</div>
            <QRCodeSVG value={qr.qrUrl} size={140} />
            <p className="cell-hint" style={{ marginTop: "8px" }}>
              {qr.qrCodeValue}
            </p>
          </div>
        )}
      </div>

      {canReactivate && asset.isActive && asset.status === "Lost" && showReactivateForm && (
        <div style={{ marginTop: "20px" }}>
          <ReactivateAssetForm
            currentLocationId={asset.currentLocationId}
            onSubmit={handleReactivate}
            onCancel={() => setShowReactivateForm(false)}
          />
        </div>
      )}

      <div style={{ marginTop: "32px" }}>
        <h2 style={{ marginBottom: "12px" }}>Movement History</h2>
        <MovementHistoryTable movements={movements} />
      </div>

      <p className="cell-hint" style={{ marginTop: "24px" }}>
        Risk recommendations are not yet available (later phases).
      </p>
    </div>
  );
}
