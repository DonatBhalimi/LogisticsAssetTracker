import { useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../auth/useAuth";
import { Badge } from "../../components/Badge";
import type { Asset } from "../../types/asset";
import type { MovementRequest, MovementResult, SourceType } from "../../types/movement";
import { MovementForm } from "./MovementForm";

interface MovementUpdateViewProps {
  asset: Asset;
  sourceType: SourceType;
  onSubmitMovement: (request: MovementRequest) => Promise<MovementResult>;
}

// Shared by ManualMovementPage and QrMovementPage (Document 07 Screen 7: Movement Update).
// Which backend endpoint gets called is decided entirely by the parent page's
// onSubmitMovement — this view never lets the user pick a source type.
export function MovementUpdateView({ asset, sourceType, onSubmitMovement }: MovementUpdateViewProps) {
  const { user } = useAuth();
  const [result, setResult] = useState<MovementResult | null>(null);

  const eligible = asset.isActive && asset.status !== "Retired";
  const showReactivateHint = asset.status === "Lost" && (user?.role === "Admin" || user?.role === "Manager");

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
      {showReactivateHint && (
        <p style={{ color: "#666" }}>
          To reactivate this Lost asset directly, use the <strong>Reactivate Lost Asset</strong> action on the asset details
          page instead of this form.
        </p>
      )}

      {result ? (
        result.resultType === "ApprovalRequired" && result.approval ? (
          <div style={{ border: "1px solid #e65100", padding: "1rem", maxWidth: 480 }}>
            <h3>Reactivation Requested</h3>
            <p>
              A Lost-reactivation request was submitted and is now <Badge text={result.approval.approvalStatus} />. A
              Manager or Admin must approve it before the asset becomes Available.
            </p>
            <Link to={`/assets/${asset.id}`}>View Asset</Link>
          </div>
        ) : (
          result.movement && result.asset && (
            <div style={{ border: "1px solid #1b5e20", padding: "1rem", maxWidth: 480 }}>
              <h3>Movement Completed</h3>
              <p>
                Moved to <strong>{result.movement.toLocationName}</strong> via {result.movement.sourceType}.
              </p>
              <Link to={`/assets/${result.asset.id}`}>View Asset</Link>
            </div>
          )
        )
      ) : (
        eligible && (
          <MovementForm
            currentLocationId={asset.currentLocationId}
            currentStatus={asset.status}
            currentCondition={asset.condition}
            sourceType={sourceType}
            onSubmit={async (request) => setResult(await onSubmitMovement(request))}
          />
        )
      )}
    </div>
  );
}
