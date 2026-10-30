import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import {
  createAdjustment,
  createWarehouse,
  listBalances,
  listProducts,
  listWarehouses,
  postAdjustment,
  updateWarehouse,
  type Product,
  type StockBalance,
  type Warehouse,
} from "../api/resources";
import { DataTable, DEFAULT_PAGE_SIZE, paginate } from "../components/DataTable";
import { Modal } from "../components/Modal";

export function InventoryPage() {
  const { t } = useTranslation();
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [balances, setBalances] = useState<StockBalance[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [whPage, setWhPage] = useState(1);
  const [balPage, setBalPage] = useState(1);
  const [whDialog, setWhDialog] = useState<"create" | "edit" | null>(null);
  const [editingWh, setEditingWh] = useState<Warehouse | null>(null);
  const [openingOpen, setOpeningOpen] = useState(false);
  const [whForm, setWhForm] = useState({ code: "", name: "", isActive: true });
  const [adjForm, setAdjForm] = useState({
    warehouseId: "",
    productId: "",
    quantity: "10",
    reason: "Opening balance",
  });

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    const [w, b, p] = await Promise.all([listWarehouses(), listBalances(), listProducts()]);
    if (w.error || b.error || p.error) {
      setError(t("inventory.loadError"));
    } else {
      const wh = w.data ?? [];
      setWarehouses(wh);
      setBalances(b.data ?? []);
      setProducts(p.data ?? []);
      setAdjForm((prev) => ({
        ...prev,
        warehouseId: prev.warehouseId || wh[0]?.id || "",
        productId: prev.productId || p.data?.[0]?.id || "",
      }));
    }
    setLoading(false);
  }, [t]);

  useEffect(() => {
    void load();
  }, [load]);

  function openCreateWarehouse() {
    setEditingWh(null);
    setWhForm({ code: "", name: "", isActive: true });
    setWhDialog("create");
  }

  function openEditWarehouse(wh: Warehouse) {
    setEditingWh(wh);
    setWhForm({ code: wh.code, name: wh.name, isActive: wh.isActive });
    setWhDialog("edit");
  }

  async function onSaveWarehouse(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    setOk(null);
    if (whDialog === "create") {
      const result = await createWarehouse(whForm.code.trim(), whForm.name.trim());
      setBusy(false);
      if (result.error || !result.data) {
        setError(result.error?.message ?? t("inventory.warehouseError"));
        return;
      }
      setOk(t("inventory.warehouseCreated"));
    } else if (editingWh) {
      const result = await updateWarehouse(editingWh.id, {
        name: whForm.name.trim(),
        isActive: whForm.isActive,
      });
      setBusy(false);
      if (result.error || !result.data) {
        setError(result.error?.message ?? t("inventory.warehouseError"));
        return;
      }
      setOk(t("inventory.warehouseUpdated"));
    } else {
      setBusy(false);
      return;
    }
    setWhDialog(null);
    setEditingWh(null);
    void load();
  }

  async function onOpeningBalance(e: FormEvent) {
    e.preventDefault();
    const warehouse = warehouses.find((w) => w.id === adjForm.warehouseId);
    if (!warehouse) return;
    setBusy(true);
    setError(null);
    setOk(null);
    const created = await createAdjustment({
      warehouseId: warehouse.id,
      kind: "OpeningBalance",
      reason: adjForm.reason,
      lines: [
        {
          productId: adjForm.productId,
          locationId: warehouse.generalLocationId,
          quantityChange: Number(adjForm.quantity),
        },
      ],
    });
    if (created.error || !created.data) {
      setBusy(false);
      setError(created.error?.message ?? t("inventory.adjustError"));
      return;
    }
    const posted = await postAdjustment(created.data.id);
    setBusy(false);
    if (posted.error || !posted.data) {
      setError(posted.error?.message ?? t("inventory.adjustError"));
      return;
    }
    setOk(t("inventory.adjustPosted"));
    setOpeningOpen(false);
    void load();
  }

  const sameId = (a: string, b: string) => a.toLowerCase() === b.toLowerCase();
  const productLabel = (id: string) => {
    const p = products.find((x) => sameId(x.id, id));
    return p ? `${p.name} (${p.sku})` : id.slice(0, 8);
  };
  const warehouseLabel = (id: string) => {
    const w = warehouses.find((x) => sameId(x.id, id));
    return w ? `${w.name} (${w.code})` : id.slice(0, 8);
  };
  const warehouseRows = paginate(warehouses, whPage, DEFAULT_PAGE_SIZE);
  const balanceRows = paginate(balances, balPage, DEFAULT_PAGE_SIZE);

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("inventory.title")}</h1>
          <p>{t("inventory.hint")}</p>
        </div>
        <div className="page-actions">
          <button type="button" className="btn secondary" onClick={() => void load()}>
            {t("products.refresh")}
          </button>
          <button type="button" className="btn secondary" onClick={() => setOpeningOpen(true)}>
            {t("inventory.openingBalance")}
          </button>
          <button type="button" className="btn" onClick={openCreateWarehouse}>
            {t("inventory.addWarehouse")}
          </button>
        </div>
      </div>

      {ok ? <p className="ok-text">{ok}</p> : null}
      {error ? <p className="error-text">{error}</p> : null}
      {loading ? <p className="muted">{t("app.loading")}</p> : null}

      <div className="panel">
        <h2 style={{ marginTop: 0, fontSize: "1.1rem" }}>{t("inventory.warehouses")}</h2>
        <DataTable
          page={whPage}
          total={warehouses.length}
          onPageChange={setWhPage}
          empty={<p className="muted">{t("inventory.emptyWarehouses")}</p>}
        >
          <table className="data-table">
            <thead>
              <tr>
                <th>{t("inventory.warehouseCode")}</th>
                <th>{t("inventory.warehouseName")}</th>
                <th>{t("inventory.actions")}</th>
              </tr>
            </thead>
            <tbody>
              {warehouseRows.map((w) => (
                <tr key={w.id}>
                  <td>
                    <strong>{w.code}</strong>
                  </td>
                  <td>{w.name}</td>
                  <td>
                    <button type="button" className="btn secondary" onClick={() => openEditWarehouse(w)}>
                      {t("common.edit")}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </DataTable>
      </div>

      <div className="panel">
        <h2 style={{ marginTop: 0, fontSize: "1.1rem" }}>{t("inventory.balances")}</h2>
        <DataTable
          page={balPage}
          total={balances.length}
          onPageChange={setBalPage}
          empty={<p className="muted">{t("inventory.empty")}</p>}
        >
          <table className="data-table">
            <thead>
              <tr>
                <th>{t("inventory.warehouse")}</th>
                <th>{t("inventory.product")}</th>
                <th>{t("inventory.onHand")}</th>
                <th>{t("inventory.reserved")}</th>
                <th>{t("inventory.available")}</th>
              </tr>
            </thead>
            <tbody>
              {balanceRows.map((b) => (
                <tr key={b.id}>
                  <td>{warehouseLabel(b.warehouseId)}</td>
                  <td>{productLabel(b.productId)}</td>
                  <td>{b.onHand}</td>
                  <td>{b.reserved}</td>
                  <td>{b.available}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </DataTable>
      </div>

      <Modal
        open={whDialog !== null}
        title={whDialog === "edit" ? t("inventory.editWarehouse") : t("inventory.addWarehouse")}
        onClose={() => setWhDialog(null)}
        footer={
          <>
            <button type="button" className="btn secondary" onClick={() => setWhDialog(null)}>
              {t("common.cancel")}
            </button>
            <button type="submit" form="warehouse-form" className="btn" disabled={busy}>
              {busy ? t("common.saving") : t("inventory.saveWarehouse")}
            </button>
          </>
        }
      >
        <form id="warehouse-form" className="stack" onSubmit={(e) => void onSaveWarehouse(e)}>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="wh-code">{t("inventory.warehouseCode")}</label>
              <input
                id="wh-code"
                value={whForm.code}
                onChange={(e) => setWhForm({ ...whForm, code: e.target.value })}
                required
                disabled={whDialog === "edit"}
              />
              {whDialog === "edit" ? <p className="muted">{t("inventory.codeImmutable")}</p> : null}
            </div>
            <div className="field">
              <label htmlFor="wh-name">{t("inventory.warehouseName")}</label>
              <input
                id="wh-name"
                value={whForm.name}
                onChange={(e) => setWhForm({ ...whForm, name: e.target.value })}
                required
              />
            </div>
          </div>
        </form>
      </Modal>

      <Modal
        open={openingOpen}
        title={t("inventory.openingBalance")}
        onClose={() => setOpeningOpen(false)}
        footer={
          <>
            <button type="button" className="btn secondary" onClick={() => setOpeningOpen(false)}>
              {t("common.cancel")}
            </button>
            <button
              type="submit"
              form="opening-form"
              className="btn"
              disabled={busy || !warehouses.length || !products.length}
            >
              {busy ? t("common.saving") : t("inventory.postOpening")}
            </button>
          </>
        }
      >
        <form id="opening-form" className="stack" onSubmit={(e) => void onOpeningBalance(e)}>
          <p className="muted" style={{ margin: 0 }}>
            {t("inventory.openingHint")}
          </p>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="adj-wh">{t("inventory.warehouses")}</label>
              <select
                id="adj-wh"
                value={adjForm.warehouseId}
                onChange={(e) => setAdjForm({ ...adjForm, warehouseId: e.target.value })}
                required
              >
                {warehouses.map((w) => (
                  <option key={w.id} value={w.id}>
                    {w.name} ({w.code})
                  </option>
                ))}
              </select>
            </div>
            <div className="field">
              <label htmlFor="adj-prod">{t("inventory.product")}</label>
              <select
                id="adj-prod"
                value={adjForm.productId}
                onChange={(e) => setAdjForm({ ...adjForm, productId: e.target.value })}
                required
              >
                {products.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name} ({p.sku})
                  </option>
                ))}
              </select>
            </div>
            <div className="field">
              <label htmlFor="adj-qty">{t("inventory.quantity")}</label>
              <input
                id="adj-qty"
                type="number"
                step="0.000001"
                value={adjForm.quantity}
                onChange={(e) => setAdjForm({ ...adjForm, quantity: e.target.value })}
                required
              />
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
}
