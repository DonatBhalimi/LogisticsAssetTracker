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
import { ManualMovementPage } from "./pages/ManualMovementPage";
import { QrMovementPage } from "./pages/QrMovementPage";
import { QrScannerPage } from "./pages/QrScannerPage";
import { ApprovalsPage } from "./pages/ApprovalsPage";
import { AuditLogsPage } from "./pages/AuditLogsPage";
import { SuspiciousActivityPage } from "./pages/SuspiciousActivityPage";

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<Navigate to="/assets" replace />} />
          <Route path="/assets" element={<AssetsListPage />} />
          <Route path="/assets/:id" element={<AssetDetailsPage />} />
          <Route path="/assets/:id/movement" element={<ManualMovementPage />} />
          <Route path="/assets/qr/:qrCodeValue/movement" element={<QrMovementPage />} />
          <Route path="/scan" element={<QrScannerPage />} />

          <Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
            <Route path="/assets/new" element={<CreateAssetPage />} />
            <Route path="/assets/:id/edit" element={<EditAssetPage />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/locations" element={<LocationsPage />} />
          </Route>

          <Route element={<ProtectedRoute allowedRoles={["Admin", "Manager"]} />}>
            <Route path="/approvals" element={<ApprovalsPage />} />
            <Route path="/suspicious-activity" element={<SuspiciousActivityPage />} />
            <Route path="/audit-logs" element={<AuditLogsPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
