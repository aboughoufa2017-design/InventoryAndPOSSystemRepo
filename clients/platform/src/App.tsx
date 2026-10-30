import { Navigate, Route, Routes } from "react-router-dom";
import { usePlatformAuth } from "./auth/PlatformAuthContext";
import { LoginPage } from "./pages/LoginPage";
import { PlatformShell } from "./pages/PlatformShell";
import { ProvisionPage } from "./pages/ProvisionPage";
import { TenantDetailPage } from "./pages/TenantDetailPage";
import { TenantsPage } from "./pages/TenantsPage";

function PlatformProtected({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = usePlatformAuth();
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return children;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/"
        element={
          <PlatformProtected>
            <PlatformShell />
          </PlatformProtected>
        }
      >
        <Route index element={<TenantsPage />} />
        <Route path="provision" element={<ProvisionPage />} />
        <Route path="tenants/:tenantId" element={<TenantDetailPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
