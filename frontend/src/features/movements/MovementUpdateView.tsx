import { useState, type ReactNode } from "react";
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
      <div className="page-header">
        <div>
          <h1>{asset.name}</h1>
          <p className="page-header-desc">{asset.assetCode}</p>
        </div>
      </div>

      <div className="card card-padded" style={{ marginBottom: "20px" }}>
        <dl className="detail-grid">
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
      </div>

      {!asset.isActive && <ErrorNote>This asset is inactive and cannot receive movement updates.</ErrorNote>}
      {asset.isActive && asset.status === "Retired" && (
        <ErrorNote>This asset is Retired and cannot receive normal movement updates.</ErrorNote>
      )}
      {showReactivateHint && (
        <p className="cell-hint" style={{ marginBottom: "16px" }}>
          To reactivate this Lost asset directly, use the <strong>Reactivate Lost Asset</strong> action on the asset details
          page instead of this form.
        </p>
      )}

      {result ? (
        result.resultType === "ApprovalRequired" && result.approval ? (
          <div className="card card-padded" style={{ maxWidth: 480, boxShadow: "inset 3px 0 0 var(--warning-text)" }}>
            <h3>Reactivation Requested</h3>
            <p style={{ marginTop: "10px" }}>
              A Lost-reactivation request was submitted and is now <Badge text={result.approval.approvalStatus} />. A
              Manager or Admin must approve it before the asset becomes Available.
            </p>
            <p style={{ marginTop: "14px" }}>
              <Link to={`/assets/${asset.id}`}>View Asset</Link>
            </p>
          </div>
        ) : (
          result.movement && result.asset && (
            <div className="card card-padded" style={{ maxWidth: 480, boxShadow: "inset 3px 0 0 var(--success-text)" }}>
              <h3>Movement Completed</h3>
              <p style={{ marginTop: "10px" }}>
                Moved to <strong>{result.movement.toLocationName}</strong> via {result.movement.sourceType}.
              </p>
              <p style={{ marginTop: "14px" }}>
                <Link to={`/assets/${result.asset.id}`}>View Asset</Link>
              </p>
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

function ErrorNote({ children }: { children: ReactNode }) {
  return <div className="error-banner">{children}</div>;
}
