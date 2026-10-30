import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "./auth/AuthContext";
import { AppShell } from "./components/AppShell";
import { AdminPage } from "./pages/AdminPage";
import { DashboardPage } from "./pages/DashboardPage";
import { InventoryPage } from "./pages/InventoryPage";
import { LoginPage } from "./pages/LoginPage";
import { ProductsPage } from "./pages/ProductsPage";
import { PurchasingPage } from "./pages/PurchasingPage";
import { SalesPage } from "./pages/SalesPage";
import { BillingPage } from "./pages/BillingPage";
import { ReportsPage } from "./pages/ReportsPage";
import { PosPage } from "./pages/PosPage";
import { StockCountsPage } from "./pages/StockCountsPage";
import { SyncPage } from "./pages/SyncPage";
import { TransfersPage } from "./pages/TransfersPage";

function Protected({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuth();
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
          <Protected>
            <AppShell />
          </Protected>
        }
      >
        <Route index element={<DashboardPage />} />
        <Route path="products" element={<ProductsPage />} />
        <Route path="inventory" element={<InventoryPage />} />
        <Route path="purchasing" element={<PurchasingPage />} />
        <Route path="sales" element={<SalesPage />} />
        <Route path="pos" element={<PosPage />} />
        <Route path="transfers" element={<TransfersPage />} />
        <Route path="stock-counts" element={<StockCountsPage />} />
        <Route path="reports" element={<ReportsPage />} />
        <Route path="billing" element={<BillingPage />} />
        <Route path="sync" element={<SyncPage />} />
        <Route path="admin" element={<AdminPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
