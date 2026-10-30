import { useEffect, useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { listPlans, provisionTenant, type Plan } from "../api/platform";

export function ProvisionPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [plans, setPlans] = useState<Plan[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({
    tenantCode: "",
    tenantName: "",
    currencyCode: "LYD",
    timeZoneId: "Africa/Tripoli",
    ownerEmail: "",
    ownerPassword: "",
    ownerDisplayName: "",
    planCode: "TRIAL",
  });

  useEffect(() => {
    void (async () => {
      const result = await listPlans();
      if (result.data?.length) {
        setPlans(result.data);
        setForm((prev) => ({ ...prev, planCode: result.data![0].code }));
      }
    })();
  }, []);

  function setField<K extends keyof typeof form>(key: K, value: (typeof form)[K]) {
    setForm((prev) => ({ ...prev, [key]: value }));
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    const result = await provisionTenant({
      ...form,
      tenantCode: form.tenantCode.trim(),
      tenantName: form.tenantName.trim(),
      currencyCode: form.currencyCode.trim().toUpperCase(),
      timeZoneId: form.timeZoneId.trim(),
      ownerEmail: form.ownerEmail.trim(),
      ownerDisplayName: form.ownerDisplayName.trim(),
      planCode: form.planCode,
    });
    setBusy(false);
    if (result.error || !result.data) {
      setError(result.error?.message ?? t("platform.provision.error"));
      return;
    }
    navigate(`/tenants/${result.data.id}`);
  }

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("platform.provision.title")}</h1>
          <p>{t("platform.provision.hint")}</p>
        </div>
        <Link className="btn secondary" to="/">
          {t("common.back")}
        </Link>
      </div>
      <form className="panel stack" onSubmit={(e) => void onSubmit(e)}>
        <div className="form-grid">
          <div className="field">
            <label htmlFor="tenantCode">{t("platform.provision.tenantCode")}</label>
            <input
              id="tenantCode"
              value={form.tenantCode}
              onChange={(e) => setField("tenantCode", e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="tenantName">{t("platform.provision.tenantName")}</label>
            <input
              id="tenantName"
              value={form.tenantName}
              onChange={(e) => setField("tenantName", e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="currencyCode">{t("platform.provision.currency")}</label>
            <input
              id="currencyCode"
              value={form.currencyCode}
              onChange={(e) => setField("currencyCode", e.target.value)}
              maxLength={3}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="timeZoneId">{t("platform.provision.timezone")}</label>
            <input
              id="timeZoneId"
              value={form.timeZoneId}
              onChange={(e) => setField("timeZoneId", e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="planCode">{t("platform.provision.plan")}</label>
            <select
              id="planCode"
              value={form.planCode}
              onChange={(e) => setField("planCode", e.target.value)}
            >
              {(plans.length ? plans : [{ code: "TRIAL", name: "Trial" }]).map((plan) => (
                <option key={plan.code} value={plan.code}>
                  {plan.name} ({plan.code})
                </option>
              ))}
            </select>
          </div>
          <div className="field">
            <label htmlFor="ownerDisplayName">{t("platform.provision.ownerName")}</label>
            <input
              id="ownerDisplayName"
              value={form.ownerDisplayName}
              onChange={(e) => setField("ownerDisplayName", e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="ownerEmail">{t("platform.provision.ownerEmail")}</label>
            <input
              id="ownerEmail"
              type="email"
              value={form.ownerEmail}
              onChange={(e) => setField("ownerEmail", e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="ownerPassword">{t("platform.provision.ownerPassword")}</label>
            <input
              id="ownerPassword"
              type="password"
              value={form.ownerPassword}
              onChange={(e) => setField("ownerPassword", e.target.value)}
              required
              minLength={8}
            />
          </div>
        </div>
        {error ? <p className="error-text">{error}</p> : null}
        <button className="btn" type="submit" disabled={busy}>
          {busy ? t("platform.provision.submitting") : t("platform.provision.submit")}
        </button>
      </form>
    </div>
  );
}
