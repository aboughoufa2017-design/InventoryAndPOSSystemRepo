import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { apiFetch } from "../api/client";

export function BillingPage() {
  const { t } = useTranslation();
  const [planCode, setPlanCode] = useState("STANDARD");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function onCheckout(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    const result = await apiFetch<{ sessionId: string; url: string }>("/api/v1/billing/checkout", {
      method: "POST",
      body: JSON.stringify({ planCode }),
    });
    setBusy(false);
    if (result.error || !result.data?.url) {
      setError(result.error?.message ?? t("billing.error"));
      return;
    }
    window.location.href = result.data.url;
  }

  async function onPortal() {
    setBusy(true);
    setError(null);
    const result = await apiFetch<{ url: string }>("/api/v1/billing/portal", { method: "POST", body: "{}" });
    setBusy(false);
    if (result.error || !result.data?.url) {
      setError(result.error?.message ?? t("billing.error"));
      return;
    }
    window.location.href = result.data.url;
  }

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("billing.title")}</h1>
          <p>{t("billing.hint")}</p>
        </div>
      </div>
      {error ? <p className="error-text">{error}</p> : null}
      <form className="panel form-grid" onSubmit={(e) => void onCheckout(e)}>
        <div className="field">
          <label htmlFor="plan">{t("billing.plan")}</label>
          <select id="plan" value={planCode} onChange={(e) => setPlanCode(e.target.value)}>
            <option value="TRIAL">TRIAL</option>
            <option value="STANDARD">STANDARD</option>
            <option value="ENTERPRISE">ENTERPRISE</option>
          </select>
        </div>
        <button className="btn" type="submit" disabled={busy}>
          {t("billing.checkout")}
        </button>
        <button className="btn secondary" type="button" disabled={busy} onClick={() => void onPortal()}>
          {t("billing.portal")}
        </button>
      </form>
    </div>
  );
}
