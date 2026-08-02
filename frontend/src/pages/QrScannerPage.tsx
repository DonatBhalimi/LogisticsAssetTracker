import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { Scanner, type IDetectedBarcode, type IScannerError } from "@yudiel/react-qr-scanner";
import * as assetsApi from "../api/assets";
import { ErrorBanner } from "../components/ErrorBanner";
import { ApiError } from "../types/common";

// The QR image (rendered on Asset Details via qrcode.react, Phase 2) encodes the
// qrUrl shape "/assets/qr/{qrCodeValue}/update" (Document 04). Extract the token
// from that shape rather than assuming the raw scan value is the token itself.
const QR_URL_PATTERN = /^\/assets\/qr\/([^/]+)\/update$/;

export function QrScannerPage() {
  const navigate = useNavigate();
  const [scanError, setScanError] = useState<string | null>(null);
  const [assetCode, setAssetCode] = useState("");
  const [fallbackError, setFallbackError] = useState<string | null>(null);
  const [isResolving, setIsResolving] = useState(false);

  function handleScan(detectedCodes: IDetectedBarcode[]) {
    const rawValue = detectedCodes[0]?.rawValue;
    if (!rawValue) return;

    const match = QR_URL_PATTERN.exec(rawValue);
    if (!match) {
      setScanError("This QR code is not a recognized asset code.");
      return;
    }

    setScanError(null);
    navigate(`/assets/qr/${match[1]}/movement`);
  }

  function handleScanError(error: IScannerError) {
    setScanError(error.message || "Unable to access the camera.");
  }

  async function handleFallbackSubmit(event: FormEvent) {
    event.preventDefault();
    if (!assetCode.trim()) return;
    setFallbackError(null);
    setIsResolving(true);
    try {
      // Manual asset-code fallback uses the manual movement flow (Document 05),
      // not the QR movement endpoint.
      const asset = await assetsApi.getAssetByCode(assetCode.trim());
      navigate(`/assets/${asset.id}/movement`);
    } catch (err) {
      setFallbackError(err instanceof ApiError ? err.message : "Asset not found.");
    } finally {
      setIsResolving(false);
    }
  }

  return (
    <div>
      <h1>QR Scanner</h1>

      {scanError && <ErrorBanner message={scanError} />}
      <div style={{ maxWidth: 480 }}>
        <Scanner onScan={handleScan} onError={handleScanError} />
      </div>

      <div style={{ marginTop: "2rem", maxWidth: 480 }}>
        <h3>Manual Asset Code Fallback</h3>
        <p style={{ color: "#666" }}>If the camera isn't available, enter the asset code directly.</p>
        {fallbackError && <ErrorBanner message={fallbackError} />}
        <form onSubmit={(event) => void handleFallbackSubmit(event)}>
          <input
            placeholder="e.g. ASSET-000001"
            value={assetCode}
            onChange={(event) => setAssetCode(event.target.value)}
            style={{ display: "block", width: "100%", marginBottom: "0.5rem" }}
          />
          <button type="submit" disabled={isResolving}>
            {isResolving ? "Looking up..." : "Continue"}
          </button>
        </form>
      </div>
    </div>
  );
}
