import { useCallback, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  getReadiness,
  getSubscription,
  getTenant,
  getUsage,
  listPlans,
  resumeTenant,
  suspendTenant,
  updateSubscription,
  type Plan,
  type PlatformTenant,
  type Subscription,
  type TenantReadiness,
  type TenantUsage,
} from "../api/platform";

export function TenantDetailPage() {
  const { tenantId = "" } = useParams();
  const { t } = useTranslation();
  const [tenant, setTenant] = useState<PlatformTenant | null>(null);
  const [usage, setUsage] = useState<TenantUsage | null>(null);
  const [readiness, setReadiness] = useState<TenantReadiness | null>(null);
  const [subscription, setSubscription] = useState<Subscription | null>(null);
  const [plans, setPlans] = useState<Plan[]>([]);
  const [planCode, setPlanCode] = useState("TRIAL");
  const [status, setStatus] = useState<Subscription["status"]>("Trial");
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    if (!tenantId) return;
    setLoading(true);
    setError(null);
    const [tenantResult, usageResult, readinessResult, subResult, plansResult] = await Promise.all([
      getTenant(tenantId),
      getUsage(tenantId),
      getReadiness(tenantId),
      getSubscription(tenantId),
      listPlans(),
    ]);
    if (tenantResult.error || !tenantResult.data) {
      setError(t("platform.detail.loadError"));
      setLoading(false);
      return;
    }
    setTenant(tenantResult.data);
    setUsage(usageResult.data ?? null);
    setReadiness(readinessResult.data ?? null);
    setSubscription(subResult.data ?? null);
    setPlans(plansResult.data ?? []);
    if (subResult.data) {
      setPlanCode(subResult.data.planCode);
      setStatus(subResult.data.status);
    }
    setLoading(false);
  }, [t, tenantId]);

  useEffect(() => {
    void load();
  }, [load]);

  async function onSuspendToggle() {
    if (!tenant) return;
    setBusy(true);
    setMessage(null);
    const result = tenant.isSuspended
      ? await resumeTenant(tenant.id)
      : await suspendTenant(tenant.id);
    setBusy(false);
    if (result.error || !result.data) {
      setError(result.error?.message ?? t("platform.detail.actionError"));
      return;
    }
    setTenant(result.data);
    setMessage(
      result.data.isSuspended ? t("platform.detail.suspendedOk") : t("platform.detail.resumedOk"),
    );
    void load();
  }

  async function onSaveSubscription() {
    if (!subscription) return;
    setBusy(true);
    setMessage(null);
    const result = await updateSubscription(subscription.tenantId, {
      planCode,
      status,
      trialEndsAtUtc: subscription.trialEndsAtUtc,
      currentPeriodStartUtc: subscription.currentPeriodStartUtc,
      currentPeriodEndUtc: subscription.currentPeriodEndUtc,
      externalReference: subscription.externalReference,
      expectedRowVersion: subscription.rowVersion,
    });
    setBusy(false);
    if (result.error || !result.data) {
      setError(result.error?.message ?? t("platform.detail.actionError"));
      return;
    }
    setSubscription(result.data);
    setMessage(t("platform.detail.subscriptionSaved"));
  }

  function formatLimit(used: number, max: number | null | undefined) {
    return max == null ? `${used} / ∞` : `${used} / ${max}`;
  }

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{tenant?.name ?? t("platform.detail.title")}</h1>
          <p>{tenant ? `${tenant.code} · ${tenant.currencyCode}` : t("platform.detail.hint")}</p>
        </div>
        <Link className="btn secondary" to="/">
          {t("common.back")}
        </Link>
      </div>
      {loading ? <p className="muted">{t("app.loading")}</p> : null}
      {error ? <p className="error-text">{error}</p> : null}
      {message ? <p className="ok-text">{message}</p> : null}

      {tenant ? (
        <div className="panel stack">
          <div className="toolbar" style={{ justifyContent: "space-between" }}>
            <div>
              {tenant.isSuspended ? (
                <span className="badge danger">{t("platform.tenants.suspended")}</span>
              ) : (
                <span className="badge ok">{t("platform.tenants.active")}</span>
              )}
              <span className="muted" style={{ marginInlineStart: "0.75rem" }}>
                {tenant.timeZoneId}
              </span>
            </div>
            <button type="button" className="btn secondary" disabled={busy} onClick={() => void onSuspendToggle()}>
              {tenant.isSuspended ? t("platform.detail.resume") : t("platform.detail.suspend")}
            </button>
          </div>
        </div>
      ) : null}

      {usage ? (
        <div className="panel">
          <h2 style={{ marginTop: 0, fontSize: "1.1rem" }}>{t("platform.detail.usage")}</h2>
          <ul className="metric-list">
            <li>
              {t("platform.detail.users")}: {formatLimit(usage.userCount, usage.planMaxUsers)}
            </li>
            <li>
              {t("platform.detail.warehouses")}:{" "}
              {formatLimit(usage.warehouseCount, usage.planMaxWarehouses)}
            </li>
            <li>
              {t("platform.detail.devices")}: {formatLimit(usage.deviceCount, usage.planMaxDevices)}
            </li>
            <li>
              {t("platform.detail.products")}: {formatLimit(usage.productCount, usage.planMaxProducts)}
            </li>
          </ul>
        </div>
      ) : null}

      {readiness ? (
        <div className="panel">
          <h2 style={{ marginTop: 0, fontSize: "1.1rem" }}>{t("platform.detail.readiness")}</h2>
          <ul className="metric-list">
            <li>
              {t("platform.detail.hasOwner")}:{" "}
              {readiness.hasOwnerUser ? t("common.yes") : t("common.no")}
            </li>
            <li>
              {t("platform.detail.hasWarehouse")}:{" "}
              {readiness.hasWarehouse ? t("common.yes") : t("common.no")}
            </li>
            <li>
              {t("platform.detail.hasProduct")}:{" "}
              {readiness.hasProduct ? t("common.yes") : t("common.no")}
            </li>
            <li>
              {t("platform.detail.hasSubscription")}:{" "}
              {readiness.hasActiveSubscription ? t("common.yes") : t("common.no")}
            </li>
          </ul>
          {readiness.missingSteps.length > 0 ? (
            <p className="muted" style={{ marginBottom: 0 }}>
              {t("platform.detail.missing")}: {readiness.missingSteps.join(", ")}
            </p>
          ) : (
            <p className="ok-text" style={{ marginBottom: 0 }}>
              {t("platform.detail.ready")}
            </p>
          )}
        </div>
      ) : null}

      {subscription ? (
        <div className="panel stack">
          <h2 style={{ marginTop: 0, fontSize: "1.1rem" }}>{t("platform.detail.subscription")}</h2>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="plan">{t("platform.provision.plan")}</label>
              <select id="plan" value={planCode} onChange={(e) => setPlanCode(e.target.value)}>
                {(plans.length ? plans : [{ code: subscription.planCode, name: subscription.planName }]).map(
                  (plan) => (
                    <option key={plan.code} value={plan.code}>
                      {plan.name} ({plan.code})
                    </option>
                  ),
                )}
              </select>
            </div>
            <div className="field">
              <label htmlFor="status">{t("platform.detail.subStatus")}</label>
              <select
                id="status"
                value={status}
                onChange={(e) => setStatus(e.target.value as Subscription["status"])}
              >
                {(["Trial", "Active", "PastDue", "Cancelled"] as const).map((value) => (
                  <option key={value} value={value}>
                    {value}
                  </option>
                ))}
              </select>
            </div>
          </div>
          <button type="button" className="btn" disabled={busy} onClick={() => void onSaveSubscription()}>
            {t("platform.detail.saveSubscription")}
          </button>
        </div>
      ) : null}
    </div>
  );
}
