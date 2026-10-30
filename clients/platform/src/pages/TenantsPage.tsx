import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { listTenants, type PlatformTenant } from "../api/platform";

export function TenantsPage() {
  const { t } = useTranslation();
  const [tenants, setTenants] = useState<PlatformTenant[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    const result = await listTenants();
    if (result.error) {
      setError(t("platform.tenants.loadError"));
      setTenants([]);
    } else {
      setTenants(result.data ?? []);
    }
    setLoading(false);
  }, [t]);

  useEffect(() => {
    void load();
  }, [load]);

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("platform.tenants.title")}</h1>
          <p>{t("platform.tenants.hint")}</p>
        </div>
        <div className="toolbar">
          <button type="button" className="btn secondary" onClick={() => void load()}>
            {t("products.refresh")}
          </button>
          <Link className="btn" to="/provision">
            {t("platform.nav.provision")}
          </Link>
        </div>
      </div>
      {loading ? <p className="muted">{t("app.loading")}</p> : null}
      {error ? <p className="error-text">{error}</p> : null}
      {!loading && !error && tenants.length === 0 ? (
        <p className="muted">{t("platform.tenants.empty")}</p>
      ) : null}
      {tenants.length > 0 ? (
        <div className="panel table-wrap">
          <table className="data-table">
            <thead>
              <tr>
                <th>{t("platform.tenants.code")}</th>
                <th>{t("platform.tenants.name")}</th>
                <th>{t("platform.tenants.currency")}</th>
                <th>{t("platform.tenants.status")}</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {tenants.map((tenant) => (
                <tr key={tenant.id}>
                  <td>{tenant.code}</td>
                  <td>{tenant.name}</td>
                  <td>{tenant.currencyCode}</td>
                  <td>
                    {tenant.isSuspended ? (
                      <span className="badge danger">{t("platform.tenants.suspended")}</span>
                    ) : tenant.isActive ? (
                      <span className="badge ok">{t("platform.tenants.active")}</span>
                    ) : (
                      <span className="badge">{t("platform.tenants.inactive")}</span>
                    )}
                  </td>
                  <td>
                    <Link to={`/tenants/${tenant.id}`}>{t("platform.tenants.open")}</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}
    </div>
  );
}
