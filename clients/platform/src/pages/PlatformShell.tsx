import { NavLink, Outlet } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { usePlatformAuth } from "../auth/PlatformAuthContext";
import { BrandMark } from "../components/BrandMark";
import { LanguageSwitcher } from "../components/LanguageSwitcher";
import { ThemeSwitcher } from "../components/ThemeSwitcher";

export function PlatformShell() {
  const { t } = useTranslation();
  const { operator, logout } = usePlatformAuth();

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand-block">
          <BrandMark size="md" title={t("app.brand")} tagline={t("platform.shell.tagline")} />
        </div>
        <nav className="nav-list" aria-label="platform">
          <NavLink to="/" end className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}>
            {t("platform.nav.tenants")}
          </NavLink>
          <NavLink to="/provision" className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}>
            {t("platform.nav.provision")}
          </NavLink>
        </nav>
        <div className="sidebar-footer">
          <div className="muted" style={{ fontSize: "0.85rem" }}>
            {operator?.displayName || operator?.email}
          </div>
          <LanguageSwitcher />
          <ThemeSwitcher />
          <button type="button" className="btn secondary" onClick={() => logout()}>
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
