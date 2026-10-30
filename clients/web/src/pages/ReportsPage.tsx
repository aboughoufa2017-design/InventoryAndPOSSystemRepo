import { useMemo, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { apiFetch } from "../api/client";

type ReportKind = "stock" | "movements" | "purchases" | "sales";

type StockRow = {
  sku: string;
  productName: string;
  warehouseCode: string;
  locationCode: string;
  onHand: number;
  available: number;
};

type MovementRow = {
  id: string;
  occurredAtUtc: string;
  movementType: string;
  productId: string;
  warehouseId: string;
  quantityChange: number;
  sourceDocumentType: string | null;
};

type DocumentRow = {
  id: string;
  documentType: string;
  status: string;
  grandTotal: number;
  createdAtUtc: string;
  reference: string | null;
};

function defaultFrom() {
  const d = new Date();
  d.setUTCDate(d.getUTCDate() - 30);
  return d.toISOString().slice(0, 16);
}

function defaultTo() {
  return new Date().toISOString().slice(0, 16);
}

export function ReportsPage() {
  const { t } = useTranslation();
  const [kind, setKind] = useState<ReportKind>("stock");
  const [fromUtc, setFromUtc] = useState(defaultFrom);
  const [toUtc, setToUtc] = useState(defaultTo);
  const [stockRows, setStockRows] = useState<StockRow[]>([]);
  const [movementRows, setMovementRows] = useState<MovementRow[]>([]);
  const [documentRows, setDocumentRows] = useState<DocumentRow[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const hasRows = useMemo(() => {
    if (kind === "stock") return stockRows.length > 0;
    if (kind === "movements") return movementRows.length > 0;
    return documentRows.length > 0;
  }, [kind, stockRows, movementRows, documentRows]);

  async function load(next: ReportKind) {
    setBusy(true);
    setError(null);
    setKind(next);

    const from = new Date(fromUtc).toISOString();
    const to = new Date(toUtc).toISOString();
    const range = `fromUtc=${encodeURIComponent(from)}&toUtc=${encodeURIComponent(to)}`;

    if (next === "stock") {
      const result = await apiFetch<StockRow[]>("/api/v1/reports/stock-on-hand");
      setBusy(false);
      if (result.error) {
        setError(result.error.message);
        return;
      }
      setStockRows(result.data ?? []);
      setMovementRows([]);
      setDocumentRows([]);
      return;
    }

    const path =
      next === "movements"
        ? `/api/v1/reports/movements?${range}`
        : next === "purchases"
          ? `/api/v1/reports/purchases?${range}`
          : `/api/v1/reports/sales?${range}`;

    if (next === "movements") {
      const result = await apiFetch<MovementRow[]>(path);
      setBusy(false);
      if (result.error) {
        setError(result.error.message);
        return;
      }
      setMovementRows(result.data ?? []);
      setStockRows([]);
      setDocumentRows([]);
      return;
    }

    const result = await apiFetch<DocumentRow[]>(path);
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setDocumentRows(result.data ?? []);
    setStockRows([]);
    setMovementRows([]);
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    await load(kind);
  }

  function downloadCsv() {
    let header = "";
    let body = "";
    let filename = "report.csv";

    if (kind === "stock") {
      header = "sku,product,warehouse,location,onHand,available";
      body = stockRows
        .map((r) =>
          [r.sku, r.productName, r.warehouseCode, r.locationCode, r.onHand, r.available]
            .map((v) => `"${String(v).replaceAll('"', '""')}"`)
            .join(","),
        )
        .join("\n");
      filename = "stock-on-hand.csv";
    } else if (kind === "movements") {
      header = "id,occurredAtUtc,type,productId,warehouseId,quantityChange,source";
      body = movementRows
        .map((r) =>
          [
            r.id,
            r.occurredAtUtc,
            r.movementType,
            r.productId,
            r.warehouseId,
            r.quantityChange,
            r.sourceDocumentType ?? "",
          ]
            .map((v) => `"${String(v).replaceAll('"', '""')}"`)
            .join(","),
        )
        .join("\n");
      filename = "movements.csv";
    } else {
      header = "id,type,status,total,createdAtUtc,reference";
      body = documentRows
        .map((r) =>
          [r.id, r.documentType, r.status, r.grandTotal, r.createdAtUtc, r.reference ?? ""]
            .map((v) => `"${String(v).replaceAll('"', '""')}"`)
            .join(","),
        )
        .join("\n");
      filename = `${kind}-summary.csv`;
    }

    const blob = new Blob([`${header}\n${body}`], { type: "text/csv;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("reports.title")}</h1>
          <p>{t("reports.hint")}</p>
        </div>
      </div>
      {error ? <p className="error-text">{error}</p> : null}
      <form className="panel toolbar" onSubmit={(e) => void onSubmit(e)}>
        <label>
          {t("reports.from")}
          <input type="datetime-local" value={fromUtc} onChange={(e) => setFromUtc(e.target.value)} />
        </label>
        <label>
          {t("reports.to")}
          <input type="datetime-local" value={toUtc} onChange={(e) => setToUtc(e.target.value)} />
        </label>
        <button className="btn" type="button" disabled={busy} onClick={() => void load("stock")}>
          {t("reports.loadStock")}
        </button>
        <button className="btn secondary" type="button" disabled={busy} onClick={() => void load("movements")}>
          {t("reports.loadMovements")}
        </button>
        <button className="btn secondary" type="button" disabled={busy} onClick={() => void load("purchases")}>
          {t("reports.loadPurchases")}
        </button>
        <button className="btn secondary" type="button" disabled={busy} onClick={() => void load("sales")}>
          {t("reports.loadSales")}
        </button>
        <button className="btn secondary" type="button" disabled={!hasRows} onClick={downloadCsv}>
          {t("reports.csv")}
        </button>
        <button className="btn secondary" type="button" disabled={!hasRows} onClick={() => window.print()}>
          {t("reports.print")}
        </button>
      </form>
      <div className="panel">
        {!hasRows ? <p className="muted">{t("reports.empty")}</p> : null}
        {kind === "stock" && stockRows.length > 0 ? (
          <table className="data-table">
            <thead>
              <tr>
                <th>SKU</th>
                <th>{t("products.name")}</th>
                <th>{t("inventory.warehouses")}</th>
                <th>{t("inventory.onHand")}</th>
                <th>{t("inventory.available")}</th>
              </tr>
            </thead>
            <tbody>
              {stockRows.map((r, i) => (
                <tr key={`${r.sku}-${r.warehouseCode}-${r.locationCode}-${i}`}>
                  <td>{r.sku}</td>
                  <td>{r.productName}</td>
                  <td>
                    {r.warehouseCode} / {r.locationCode}
                  </td>
                  <td>{r.onHand}</td>
                  <td>{r.available}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : null}
        {kind === "movements" && movementRows.length > 0 ? (
          <table className="data-table">
            <thead>
              <tr>
                <th>{t("sync.operation")}</th>
                <th>UTC</th>
                <th>{t("inventory.product")}</th>
                <th>{t("inventory.quantity")}</th>
              </tr>
            </thead>
            <tbody>
              {movementRows.map((r) => (
                <tr key={r.id}>
                  <td>{r.movementType}</td>
                  <td>{new Date(r.occurredAtUtc).toLocaleString()}</td>
                  <td>{r.productId}</td>
                  <td>{r.quantityChange}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : null}
        {(kind === "purchases" || kind === "sales") && documentRows.length > 0 ? (
          <table className="data-table">
            <thead>
              <tr>
                <th>{t("purchasing.reference")}</th>
                <th>{t("purchasing.status")}</th>
                <th>{t("purchasing.total")}</th>
                <th>UTC</th>
              </tr>
            </thead>
            <tbody>
              {documentRows.map((r) => (
                <tr key={r.id}>
                  <td>{r.reference ?? r.id}</td>
                  <td>{r.status}</td>
                  <td>{r.grandTotal}</td>
                  <td>{new Date(r.createdAtUtc).toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : null}
      </div>
    </div>
  );
}
