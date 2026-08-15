import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import * as assetsApi from "../api/assets";
import { Badge } from "../components/Badge";
import { EmptyState } from "../components/EmptyState";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingState } from "../components/LoadingState";
import { useAuth } from "../auth/useAuth";
import { ApiError } from "../types/common";
import type { Asset } from "../types/asset";

export function AssetsListPage() {
  const { user } = useAuth();
  const [assets, setAssets] = useState<Asset[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState("");

  async function refresh() {
    setIsLoading(true);
    setError(null);
    try {
      const result = await assetsApi.listAssets({
        page: 1,
        pageSize: 50,
        sortBy: "updatedAt",
        sortDirection: "desc",
        search: search || undefined,
      });
      setAssets(result.items);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to load assets.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void refresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Assets</h1>
          <p className="page-header-desc">Search, filter, and open logistics assets.</p>
        </div>
        {user?.role === "Admin" && (
          <div className="page-header-actions">
            <Link to="/assets/new" className="btn btn-primary">
              Create Asset
            </Link>
          </div>
        )}
      </div>

      <form
        onSubmit={(event) => {
          event.preventDefault();
          void refresh();
        }}
        className="filters-bar"
      >
        <div className="field">
          <label htmlFor="search">Search</label>
          <input
            id="search"
            placeholder="Name or asset code"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        </div>
        <button type="submit" className="btn btn-secondary">
          Search
        </button>
      </form>

      {error && <ErrorBanner message={error} />}

      <div className="table-card">
        {isLoading && <LoadingState />}
        {!isLoading && assets.length === 0 && <EmptyState />}
        {!isLoading && assets.length > 0 && (
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th align="left">Asset Code</th>
                  <th align="left">Name</th>
                  <th align="left">Type</th>
                  <th align="left">Current Location</th>
                  <th align="left">Status</th>
                  <th align="left">Condition</th>
                  <th align="left">Last Updated</th>
                  <th align="left">Actions</th>
                </tr>
              </thead>
              <tbody>
                {assets.map((asset) => (
                  <tr key={asset.id}>
                    <td>{asset.assetCode}</td>
                    <td>{asset.name}</td>
                    <td className="cell-muted">{asset.type}</td>
                    <td className="cell-muted">{asset.currentLocationName}</td>
                    <td>
                      <Badge text={asset.status} />
                    </td>
                    <td>
                      <Badge text={asset.condition} />
                    </td>
                    <td className="cell-muted">{new Date(asset.updatedAt).toLocaleString()}</td>
                    <td>
                      <div className="btn-row">
                        <Link to={`/assets/${asset.id}`} className="btn btn-secondary btn-sm">
                          View
                        </Link>
                        {asset.isActive && asset.status !== "Retired" && (
                          <Link to={`/assets/${asset.id}/movement`} className="btn btn-secondary btn-sm">
                            Update Movement
                          </Link>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
