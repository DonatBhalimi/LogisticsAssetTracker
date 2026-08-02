import { useState } from "react";
import { Link } from "react-router-dom";
import { Badge } from "../../components/Badge";
import type { Asset } from "../../types/asset";
import type { MovementRequest, MovementResult } from "../../types/movement";
import { MovementForm } from "./MovementForm";

interface MovementUpdateViewProps {
  asset: Asset;
  onSubmitMovement: (request: MovementRequest) => Promise<MovementResult>;
}

// Shared by ManualMovementPage and QrMovementPage (Document 07 Screen 7: Movement Update).
// Which backend endpoint gets called is decided entirely by the parent page's
// onSubmitMovement — this view never lets the user pick a source type.
export function MovementUpdateView({ asset, onSubmitMovement }: MovementUpdateViewProps) {
  const [result, setResult] = useState<MovementResult | null>(null);

  const eligible = asset.isActive && asset.status !== "Retired";

  return (
    <div>
      <h1>{asset.name}</h1>
      <dl>
        <dt>Asset Code</dt>
        <dd>{asset.assetCode}</dd>
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
      </dl>

      {!asset.isActive && <p style={{ color: "#b71c1c" }}>This asset is inactive and cannot receive movement updates.</p>}
      {asset.isActive && asset.status === "Retired" && (
        <p style={{ color: "#b71c1c" }}>This asset is Retired and cannot receive normal movement updates.</p>
      )}

      {result ? (
        <div style={{ border: "1px solid #1b5e20", padding: "1rem", maxWidth: 480 }}>
          <h3>Movement Completed</h3>
          <p>
            Moved to <strong>{result.movement.toLocationName}</strong> via {result.movement.sourceType}.
          </p>
          <Link to={`/assets/${result.asset.id}`}>View Asset</Link>
        </div>
      ) : (
        eligible && <MovementForm currentLocationId={asset.currentLocationId} onSubmit={async (request) => setResult(await onSubmitMovement(request))} />
      )}
    </div>
  );
}
