import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import * as assetsApi from "../api/assets";
import * as movementsApi from "../api/movements";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { MovementUpdateView } from "../features/movements/MovementUpdateView";
import { ApiError } from "../types/common";
import type { Asset } from "../types/asset";

// Route: /assets/qr/:qrCodeValue/movement (Document 06 QR movement endpoint).
// Reached only after a successful camera scan (Document 07 Screen 6).
export function QrMovementPage() {
  const { qrCodeValue } = useParams<{ qrCodeValue: string }>();
  const [asset, setAsset] = useState<Asset | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!qrCodeValue) return;
    setIsLoading(true);
    assetsApi
      .getAssetByQr(qrCodeValue)
      .then(setAsset)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Unable to load asset."))
      .finally(() => setIsLoading(false));
  }, [qrCodeValue]);

  if (isLoading) return <LoadingState />;
  if (error && !asset) return <ErrorBanner message={error} />;
  if (!asset || !qrCodeValue) return null;

  return (
    <div>
      <p>
        <Link to="/scan">&larr; Back to Scanner</Link>
      </p>
      {error && <ErrorBanner message={error} />}
      <MovementUpdateView asset={asset} onSubmitMovement={(request) => movementsApi.createQrMovement(qrCodeValue, request)} />
    </div>
  );
}
