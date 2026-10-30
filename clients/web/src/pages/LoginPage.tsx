import { useState, type FormEvent } from "react";
import { Navigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../auth/AuthContext";
import { BrandMark } from "../components/BrandMark";
import { LanguageSwitcher } from "../components/LanguageSwitcher";
import { ThemeSwitcher } from "../components/ThemeSwitcher";

export function LoginPage() {
  const { t } = useTranslation();
  const { isAuthenticated, login } = useAuth();
  const [tenantCode, setTenantCode] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    const message = await login(tenantCode.trim(), email.trim(), password);
    if (message) {
      setError(t("login.error"));
    }
    setBusy(false);
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <BrandMark size="lg" title={t("app.brand")} tagline={t("app.tagline")} />
        <div className="login-toolbar">
          <LanguageSwitcher />
          <ThemeSwitcher />
        </div>
        <form onSubmit={(e) => void onSubmit(e)} style={{ marginTop: "1.25rem" }}>
          <div className="field">
            <label htmlFor="tenant">{t("login.tenantCode")}</label>
            <input
              id="tenant"
              autoComplete="organization"
              value={tenantCode}
              onChange={(e) => setTenantCode(e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="email">{t("login.email")}</label>
            <input
              id="email"
              type="email"
              autoComplete="username"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="password">{t("login.password")}</label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          <p className="muted" style={{ fontSize: "0.88rem" }}>
            {t("login.hint")}
          </p>
          {error ? <p className="error-text">{error}</p> : null}
          <button className="btn" type="submit" disabled={busy} style={{ width: "100%", marginTop: "1rem" }}>
            {busy ? t("login.submitting") : t("login.submit")}
          </button>
        </form>
      </div>
    </div>
  );
}
