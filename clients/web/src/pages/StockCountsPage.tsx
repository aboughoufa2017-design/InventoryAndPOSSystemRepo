import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { apiFetch } from "../api/client";
import { listWarehouses, type Warehouse } from "../api/resources";
import { DataTable, DEFAULT_PAGE_SIZE, paginate } from "../components/DataTable";
import { Modal } from "../components/Modal";

type StockCount = {
  id: string;
  warehouseId: string;
  locationId: string;
  status: string;
  rowVersion: number;
  lines: {
    id: string;
    productId: string;
    systemQuantity: number;
    countedQuantity?: number | null;
    variance: number;
  }[];
};

export function StockCountsPage() {
  const { t } = useTranslation();
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [items, setItems] = useState<StockCount[]>([]);
  const [warehouseId, setWarehouseId] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [page, setPage] = useState(1);
  const [dialogOpen, setDialogOpen] = useState(false);

  const load = useCallback(async () => {
    setError(null);
    const [wh, counts] = await Promise.all([
      listWarehouses(),
      apiFetch<StockCount[]>("/api/v1/stock-counts"),
    ]);
    if (wh.error || counts.error) {
      setError(t("stockCounts.loadError"));
      return;
    }
    setWarehouses(wh.data ?? []);
    setItems(counts.data ?? []);
    setWarehouseId((prev) => prev || wh.data?.[0]?.id || "");
  }, [t]);

  useEffect(() => {
    void load();
  }, [load]);

  async function onCreate(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    setOk(null);
    const result = await apiFetch<StockCount>("/api/v1/stock-counts", {
      method: "POST",
      body: JSON.stringify({ warehouseId }),
    });
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setOk(t("stockCounts.created"));
    setDialogOpen(false);
    void load();
  }

  async function onPost(count: StockCount) {
    setBusy(true);
    setError(null);
    const lines = count.lines.map((l) => ({
      productId: l.productId,
      countedQuantity: l.countedQuantity ?? l.systemQuantity,
    }));
    const setLines = await apiFetch<StockCount>(`/api/v1/stock-counts/${count.id}/lines`, {
      method: "PUT",
      body: JSON.stringify({ expectedRowVersion: count.rowVersion, lines }),
    });
    if (setLines.error || !setLines.data) {
      setBusy(false);
      setError(setLines.error?.message ?? t("stockCounts.actionError"));
      return;
    }
    const posted = await apiFetch<StockCount>(`/api/v1/stock-counts/${count.id}/post`, {
      method: "POST",
      body: JSON.stringify({ expectedRowVersion: setLines.data.rowVersion }),
    });
    setBusy(false);
    if (posted.error) {
      setError(posted.error.message);
      return;
    }
    setOk(t("stockCounts.posted"));
    void load();
  }

  const rows = paginate(items, page, DEFAULT_PAGE_SIZE);
  const whLabel = (id: string) => {
    const wh = warehouses.find((w) => w.id === id);
    return wh ? `${wh.code} — ${wh.name}` : id.slice(0, 8);
  };

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("stockCounts.title")}</h1>
          <p>{t("stockCounts.hint")}</p>
        </div>
        <div className="page-actions">
          <button type="button" className="btn secondary" onClick={() => void load()}>
            {t("products.refresh")}
          </button>
          <button type="button" className="btn" onClick={() => setDialogOpen(true)}>
            {t("stockCounts.start")}
          </button>
        </div>
      </div>
      {error ? <p className="error-text">{error}</p> : null}
      {ok ? <p className="ok-text">{ok}</p> : null}

      <div className="panel">
        <DataTable
          page={page}
          total={items.length}
          onPageChange={setPage}
          empty={<p className="muted">{t("stockCounts.empty")}</p>}
        >
          <table className="data-table">
            <thead>
              <tr>
                <th>{t("inventory.warehouses")}</th>
                <th>{t("stockCounts.status")}</th>
                <th>{t("stockCounts.lines")}</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {rows.map((c) => (
                <tr key={c.id}>
                  <td>{whLabel(c.warehouseId)}</td>
                  <td>{c.status}</td>
                  <td>{c.lines.length}</td>
                  <td>
                    {c.status === "Draft" || c.status === "Counted" ? (
                      <button type="button" className="btn" disabled={busy} onClick={() => void onPost(c)}>
                        {t("stockCounts.post")}
                      </button>
                    ) : null}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </DataTable>
      </div>

      <Modal
        open={dialogOpen}
        title={t("stockCounts.start")}
        onClose={() => setDialogOpen(false)}
        footer={
          <>
            <button type="button" className="btn secondary" onClick={() => setDialogOpen(false)}>
              {t("common.cancel")}
            </button>
            <button type="submit" form="stock-count-form" className="btn" disabled={busy || !warehouseId}>
              {busy ? t("common.saving") : t("stockCounts.start")}
            </button>
          </>
        }
      >
        <form id="stock-count-form" className="stack" onSubmit={(e) => void onCreate(e)}>
          <div className="field">
            <label htmlFor="sc-wh">{t("inventory.warehouses")}</label>
            <select id="sc-wh" value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)} required>
              {warehouses.map((w) => (
                <option key={w.id} value={w.id}>
                  {w.code} — {w.name}
                </option>
              ))}
            </select>
          </div>
        </form>
      </Modal>
    </div>
  );
}
