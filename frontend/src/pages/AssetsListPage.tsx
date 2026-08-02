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
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <h1>Assets</h1>
        {user?.role === "Admin" && <Link to="/assets/new">Create Asset</Link>}
      </div>

      <form
        onSubmit={(event) => {
          event.preventDefault();
          void refresh();
        }}
        style={{ margin: "1rem 0" }}
      >
        <input
          placeholder="Search by name or asset code"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />{" "}
        <button type="submit">Search</button>
      </form>

      {error && <ErrorBanner message={error} />}
      {isLoading && <LoadingState />}
      {!isLoading && assets.length === 0 && <EmptyState />}
      {!isLoading && assets.length > 0 && (
        <table style={{ width: "100%", borderCollapse: "collapse" }}>
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
              <tr key={asset.id} style={{ borderTop: "1px solid #eee" }}>
                <td>{asset.assetCode}</td>
                <td>{asset.name}</td>
                <td>{asset.type}</td>
                <td>{asset.currentLocationName}</td>
                <td>
                  <Badge text={asset.status} />
                </td>
                <td>
                  <Badge text={asset.condition} />
                </td>
                <td>{new Date(asset.updatedAt).toLocaleString()}</td>
                <td>
                  <Link to={`/assets/${asset.id}`}>View</Link>{" "}
                  {asset.isActive && asset.status !== "Retired" && (
                    <Link to={`/assets/${asset.id}/movement`}>Update Movement</Link>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
