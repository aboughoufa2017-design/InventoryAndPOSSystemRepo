import { useState } from "react";
import { useTranslation } from "react-i18next";
import { pullSync, type SyncPullResponse } from "../api/resources";

export function SyncPage() {
  const { t } = useTranslation();
  const [afterVersion, setAfterVersion] = useState(0);
  const [result, setResult] = useState<SyncPullResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function onPull() {
    setBusy(true);
    setError(null);
    const response = await pullSync(afterVersion);
    if (response.error) {
      setError(t("sync.loadError"));
      setResult(null);
    } else {
      setResult(response.data ?? null);
    }
    setBusy(false);
  }

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("sync.title")}</h1>
          <p>{t("sync.subtitle")}</p>
        </div>
      </div>
      <div className="panel">
        <div className="field" style={{ maxWidth: 240 }}>
          <label htmlFor="after">{t("sync.afterVersion")}</label>
          <input
            id="after"
            type="number"
            min={0}
            value={afterVersion}
            onChange={(e) => setAfterVersion(Number(e.target.value) || 0)}
          />
        </div>
        <button type="button" className="btn" disabled={busy} onClick={() => void onPull()}>
          {busy ? t("sync.pulling") : t("sync.pull")}
        </button>
        <p className="muted" style={{ marginTop: "1rem", fontSize: "0.9rem" }}>
          {t("sync.note")}
        </p>
        {error ? <p className="error-text">{error}</p> : null}
      </div>
      {result ? (
        <>
          <div className="panel">
            <h2 style={{ marginTop: 0, fontSize: "1.1rem" }}>{t("sync.security")}</h2>
            <ul style={{ margin: 0, paddingInlineStart: "1.2rem" }}>
              <li>
                {t("sync.deviceRevoked")}:{" "}
                <span className={`badge${result.security.deviceRevoked ? " warn" : ""}`}>
                  {result.security.deviceRevoked ? t("sync.blocked") : t("sync.ok")}
                </span>
              </li>
              <li>
                {t("sync.userDisabled")}:{" "}
                <span className={`badge${result.security.userDisabled ? " warn" : ""}`}>
                  {result.security.userDisabled ? t("sync.blocked") : t("sync.ok")}
                </span>
              </li>
              <li>
                {t("sync.tenantSuspended")}:{" "}
                <span className={`badge${result.security.tenantSuspended ? " warn" : ""}`}>
                  {result.security.tenantSuspended ? t("sync.blocked") : t("sync.ok")}
                </span>
              </li>
              <li>
                {t("sync.blocksApply")}:{" "}
                <span className={`badge${result.security.blocksApply ? " warn" : ""}`}>
                  {result.security.blocksApply ? t("sync.blocked") : t("sync.ok")}
                </span>
              </li>
            </ul>
          </div>
          <div className="panel">
            {result.changes.length === 0 ? <p className="muted">{t("sync.empty")}</p> : null}
            {result.changes.length > 0 ? (
              <div className="table-wrap">
                <table className="data">
                  <thead>
                    <tr>
                      <th>{t("sync.version")}</th>
                      <th>{t("sync.entity")}</th>
                      <th>{t("sync.operation")}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {result.changes.map((c) => (
                      <tr key={`${c.changeVersion}-${c.entityId}`}>
                        <td>{c.changeVersion}</td>
                        <td>
                          {c.entityType} ({c.entityId.slice(0, 8)})
                        </td>
                        <td>{c.operation}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : null}
          </div>
        </>
      ) : null}
    </div>
  );
}
