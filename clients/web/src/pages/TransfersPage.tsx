import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import {
  createTransfer,
  dispatchTransfer,
  listProducts,
  listTransfers,
  listWarehouses,
  receiveTransfer,
  type Product,
  type Transfer,
  type Warehouse,
} from "../api/resources";
import { DataTable, DEFAULT_PAGE_SIZE, paginate } from "../components/DataTable";
import { Modal } from "../components/Modal";

export function TransfersPage() {
  const { t } = useTranslation();
  const [items, setItems] = useState<Transfer[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [page, setPage] = useState(1);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [form, setForm] = useState({
    sourceWarehouseId: "",
    destinationWarehouseId: "",
    productId: "",
    quantity: "1",
    notes: "",
  });

  const load = useCallback(async () => {
    setError(null);
    const [tr, w, p] = await Promise.all([listTransfers(), listWarehouses(), listProducts()]);
    if (tr.error || w.error || p.error) {
      setError(t("transfers.loadError"));
      return;
    }
    setItems(tr.data ?? []);
    setWarehouses(w.data ?? []);
    setProducts(p.data ?? []);
    setForm((prev) => ({
      ...prev,
      sourceWarehouseId: prev.sourceWarehouseId || w.data?.[0]?.id || "",
      destinationWarehouseId: prev.destinationWarehouseId || w.data?.[1]?.id || w.data?.[0]?.id || "",
      productId: prev.productId || p.data?.[0]?.id || "",
    }));
  }, [t]);

  useEffect(() => {
    void load();
  }, [load]);

  async function onCreate(e: FormEvent) {
    e.preventDefault();
    if (form.sourceWarehouseId === form.destinationWarehouseId) {
      setError(t("transfers.sameWarehouse"));
      return;
    }
    setBusy(true);
    setError(null);
    setOk(null);
    const result = await createTransfer({
      sourceWarehouseId: form.sourceWarehouseId,
      destinationWarehouseId: form.destinationWarehouseId,
      notes: form.notes || undefined,
      lines: [{ productId: form.productId, quantity: Number(form.quantity) }],
    });
    setBusy(false);
    if (result.error || !result.data) {
      setError(result.error?.message ?? t("transfers.createError"));
      return;
    }
    setOk(t("transfers.created"));
    setDialogOpen(false);
    void load();
  }

  async function onDispatch(tr: Transfer) {
    setBusy(true);
    const result = await dispatchTransfer(tr.id, tr.rowVersion);
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setOk(t("transfers.dispatched"));
    void load();
  }

  async function onReceive(tr: Transfer) {
    setBusy(true);
    const lines = tr.lines
      .map((l) => ({
        transferLineId: l.id,
        quantity: Number(l.quantityInTransit > 0 ? l.quantityInTransit : l.quantityRequested - l.quantityReceived),
      }))
      .filter((l) => l.quantity > 0);
    const result = await receiveTransfer(tr.id, tr.rowVersion, lines);
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setOk(t("transfers.received"));
    void load();
  }

  const whCode = (id: string) => warehouses.find((w) => w.id === id)?.code ?? id.slice(0, 8);
  const rows = paginate(items, page, DEFAULT_PAGE_SIZE);

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("transfers.title")}</h1>
          <p>{t("transfers.hint")}</p>
        </div>
        <div className="page-actions">
          <button type="button" className="btn secondary" onClick={() => void load()}>
            {t("products.refresh")}
          </button>
          <button type="button" className="btn" onClick={() => setDialogOpen(true)}>
            {t("transfers.add")}
          </button>
        </div>
      </div>
      {ok ? <p className="ok-text">{ok}</p> : null}
      {error ? <p className="error-text">{error}</p> : null}

      <div className="panel">
        <DataTable
          page={page}
          total={items.length}
          onPageChange={setPage}
          empty={<p className="muted">{t("transfers.empty")}</p>}
        >
          <table className="data-table">
            <thead>
              <tr>
                <th>{t("transfers.source")}</th>
                <th>{t("transfers.destination")}</th>
                <th>{t("transfers.status")}</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {rows.map((tr) => (
                <tr key={tr.id}>
                  <td>{whCode(tr.sourceWarehouseId)}</td>
                  <td>{whCode(tr.destinationWarehouseId)}</td>
                  <td>{tr.status}</td>
                  <td className="toolbar">
                    {tr.status === "Draft" ? (
                      <button
                        type="button"
                        className="btn secondary"
                        disabled={busy}
                        onClick={() => void onDispatch(tr)}
                      >
                        {t("transfers.dispatch")}
                      </button>
                    ) : null}
                    {tr.status === "Dispatched" || tr.status === "PartiallyReceived" ? (
                      <button type="button" className="btn" disabled={busy} onClick={() => void onReceive(tr)}>
                        {t("transfers.receive")}
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
        title={t("transfers.add")}
        onClose={() => setDialogOpen(false)}
        footer={
          <>
            <button type="button" className="btn secondary" onClick={() => setDialogOpen(false)}>
              {t("common.cancel")}
            </button>
            <button
              type="submit"
              form="transfer-form"
              className="btn"
              disabled={busy || warehouses.length < 2 || !products.length}
            >
              {busy ? t("common.saving") : t("transfers.save")}
            </button>
          </>
        }
      >
        <form id="transfer-form" className="stack" onSubmit={(e) => void onCreate(e)}>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="src">{t("transfers.source")}</label>
              <select
                id="src"
                value={form.sourceWarehouseId}
                onChange={(e) => setForm({ ...form, sourceWarehouseId: e.target.value })}
                required
              >
                {warehouses.map((w) => (
                  <option key={w.id} value={w.id}>
                    {w.code} — {w.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="field">
              <label htmlFor="dst">{t("transfers.destination")}</label>
              <select
                id="dst"
                value={form.destinationWarehouseId}
                onChange={(e) => setForm({ ...form, destinationWarehouseId: e.target.value })}
                required
              >
                {warehouses.map((w) => (
                  <option key={w.id} value={w.id}>
                    {w.code} — {w.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="field">
              <label htmlFor="tr-prod">{t("inventory.product")}</label>
              <select
                id="tr-prod"
                value={form.productId}
                onChange={(e) => setForm({ ...form, productId: e.target.value })}
                required
              >
                {products.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.sku} — {p.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="field">
              <label htmlFor="tr-qty">{t("inventory.quantity")}</label>
              <input
                id="tr-qty"
                type="number"
                min="0.000001"
                step="any"
                value={form.quantity}
                onChange={(e) => setForm({ ...form, quantity: e.target.value })}
                required
              />
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
}
