import { NavLink, Outlet } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../auth/AuthContext";
import { BrandMark } from "./BrandMark";
import { LanguageSwitcher } from "./LanguageSwitcher";
import { ThemeSwitcher } from "./ThemeSwitcher";

const links = [
  { to: "/", key: "dashboard", end: true },
  { to: "/products", key: "products" },
  { to: "/inventory", key: "inventory" },
  { to: "/purchasing", key: "purchasing" },
  { to: "/sales", key: "sales" },
  { to: "/pos", key: "pos" },
  { to: "/transfers", key: "transfers" },
  { to: "/stock-counts", key: "stockCounts" },
  { to: "/reports", key: "reports" },
  { to: "/billing", key: "billing" },
  { to: "/sync", key: "sync" },
  { to: "/admin", key: "admin" },
] as const;

export function AppShell() {
  const { t } = useTranslation();
  const { user, logout } = useAuth();

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand-block">
          <BrandMark size="md" title={t("app.brand")} tagline={t("app.tagline")} />
        </div>
        <nav className="nav-list" aria-label="main">
          {links.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              end={"end" in link ? link.end : false}
              className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}
            >
              {t(`nav.${link.key}`)}
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-footer">
          <div className="muted" style={{ fontSize: "0.85rem" }}>
            {user?.displayName || user?.email}
          </div>
          <LanguageSwitcher />
          <ThemeSwitcher />
          <button type="button" className="btn secondary" onClick={() => void logout()}>
            {t("nav.logout")}
          </button>
        </div>
      </aside>
      <main className="main">
        <Outlet />
      </main>
    </div>
  );
}
