import { Navigate, Route, Routes } from "react-router-dom";
import { AppLayout } from "./components/AppLayout";
import { ProtectedRoute } from "./auth/ProtectedRoute";
import { LoginPage } from "./pages/LoginPage";
import { UsersPage } from "./pages/UsersPage";
import { LocationsPage } from "./pages/LocationsPage";
import { AssetsListPage } from "./pages/AssetsListPage";
import { AssetDetailsPage } from "./pages/AssetDetailsPage";
import { CreateAssetPage } from "./pages/CreateAssetPage";
import { EditAssetPage } from "./pages/EditAssetPage";

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<Navigate to="/assets" replace />} />
          <Route path="/assets" element={<AssetsListPage />} />
          <Route path="/assets/:id" element={<AssetDetailsPage />} />

          <Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
            <Route path="/assets/new" element={<CreateAssetPage />} />
            <Route path="/assets/:id/edit" element={<EditAssetPage />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/locations" element={<LocationsPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
