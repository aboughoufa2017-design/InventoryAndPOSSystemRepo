import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import {
  confirmPurchase,
  createPurchase,
  createSupplier,
  getPurchase,
  listProducts,
  listPurchases,
  listSuppliers,
  listWarehouses,
  receivePurchase,
  updatePurchasePayment,
  type Product,
  type PurchaseOrder,
  type Supplier,
  type Warehouse,
} from "../api/resources";
import { DataTable, DEFAULT_PAGE_SIZE, paginate } from "../components/DataTable";
import { Modal } from "../components/Modal";

export function PurchasingPage() {
  const { t } = useTranslation();
  const [orders, setOrders] = useState<PurchaseOrder[]>([]);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [page, setPage] = useState(1);
  const [supplierOpen, setSupplierOpen] = useState(false);
  const [poOpen, setPoOpen] = useState(false);
  const [supplierForm, setSupplierForm] = useState({ code: "", name: "" });
  const [poForm, setPoForm] = useState({
    supplierId: "",
    warehouseId: "",
    productId: "",
    quantity: "1",
    unitPrice: "",
    reference: "",
  });

  const load = useCallback(async () => {
    setError(null);
    const [o, s, w, p] = await Promise.all([
      listPurchases(),
      listSuppliers(),
      listWarehouses(),
      listProducts(),
    ]);
    if (o.error || s.error || w.error || p.error) {
      setError(t("purchasing.loadError"));
      return;
    }
    setOrders(o.data ?? []);
    setSuppliers(s.data ?? []);
    setWarehouses(w.data ?? []);
    setProducts(p.data ?? []);
    setPoForm((prev) => ({
      ...prev,
      supplierId: prev.supplierId || s.data?.[0]?.id || "",
      warehouseId: prev.warehouseId || w.data?.[0]?.id || "",
      productId: prev.productId || p.data?.[0]?.id || "",
    }));
  }, [t]);

  useEffect(() => {
    void load();
  }, [load]);

  async function onCreateSupplier(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    const result = await createSupplier({
      code: supplierForm.code.trim(),
      name: supplierForm.name.trim(),
    });
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setOk(t("purchasing.supplierCreated"));
    setSupplierForm({ code: "", name: "" });
    setSupplierOpen(false);
    void load();
  }

  async function onCreatePo(e: FormEvent) {
    e.preventDefault();
    const warehouse = warehouses.find((w) => w.id === poForm.warehouseId);
    if (!warehouse) return;
    setBusy(true);
    setError(null);
    setOk(null);
    const product = products.find((p) => p.id === poForm.productId);
    const result = await createPurchase({
      supplierId: poForm.supplierId,
      warehouseId: warehouse.id,
      locationId: warehouse.generalLocationId,
      reference: poForm.reference || undefined,
      lines: [
        {
          productId: poForm.productId,
          quantity: Number(poForm.quantity),
          unitPrice: poForm.unitPrice ? Number(poForm.unitPrice) : product?.purchasePrice,
        },
      ],
    });
    setBusy(false);
    if (result.error || !result.data) {
      setError(result.error?.message ?? t("purchasing.createError"));
      return;
    }
    setOk(t("purchasing.created"));
    setPoOpen(false);
    void load();
  }

  async function onConfirm(id: string) {
    setBusy(true);
    setError(null);
    const detail = await getPurchase(id);
    if (detail.error || !detail.data) {
      setBusy(false);
      setError(detail.error?.message ?? t("purchasing.actionError"));
      return;
    }
    const result = await confirmPurchase(id, detail.data.rowVersion);
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setOk(t("purchasing.confirmed"));
    void load();
  }

  async function onReceive(id: string) {
    setBusy(true);
    setError(null);
    const detail = await getPurchase(id);
    if (detail.error || !detail.data) {
      setBusy(false);
      setError(detail.error?.message ?? t("purchasing.actionError"));
      return;
    }
    const lines = detail.data.lines
      .map((l) => ({
        purchaseOrderLineId: l.id,
        quantity: Math.max(0, Number(l.quantityOrdered) - Number(l.quantityReceived)),
      }))
      .filter((l) => l.quantity > 0);
    const result = await receivePurchase(id, detail.data.rowVersion, lines);
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setOk(t("purchasing.received"));
    void load();
  }

  async function onMarkPaid(id: string) {
    setBusy(true);
    setError(null);
    setOk(null);
    const detail = await getPurchase(id);
    if (detail.error || !detail.data) {
      setBusy(false);
      setError(detail.error?.message ?? t("purchasing.actionError"));
      return;
    }
    const result = await updatePurchasePayment(id, detail.data.grandTotal, detail.data.rowVersion);
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setOk(t("purchasing.markedPaid"));
    void load();
  }

  const rows = paginate(orders, page, DEFAULT_PAGE_SIZE);
  const sameId = (a: string, b: string) => a.toLowerCase() === b.toLowerCase();
  const warehouseLabel = (id: string) => {
    const w = warehouses.find((x) => sameId(x.id, id));
    return w ? `${w.name} (${w.code})` : id.slice(0, 8);
  };
  const productLabel = (id: string) => {
    const p = products.find((x) => sameId(x.id, id));
    return p ? `${p.name} (${p.sku})` : id.slice(0, 8);
  };
  const orderProducts = (o: PurchaseOrder) =>
    o.lines.map((l) => productLabel(l.productId)).join(", ") || "—";
  const statusLabel = (status: string) => t(`status.${status}`, { defaultValue: status });
  const paymentLabel = (status: string) => t(`payment.${status}`, { defaultValue: status });

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("purchasing.title")}</h1>
          <p>{t("purchasing.hint")}</p>
        </div>
        <div className="page-actions">
          <button type="button" className="btn secondary" onClick={() => void load()}>
            {t("products.refresh")}
          </button>
          <button type="button" className="btn secondary" onClick={() => setSupplierOpen(true)}>
            {t("purchasing.addSupplier")}
          </button>
          <button type="button" className="btn" onClick={() => setPoOpen(true)}>
            {t("purchasing.addOrder")}
          </button>
        </div>
      </div>
      {ok ? <p className="ok-text">{ok}</p> : null}
      {error ? <p className="error-text">{error}</p> : null}

      <div className="panel">
        <DataTable
          page={page}
          total={orders.length}
          onPageChange={setPage}
          empty={<p className="muted">{t("purchasing.empty")}</p>}
        >
          <table className="data-table">
            <thead>
              <tr>
                <th>{t("purchasing.reference")}</th>
                <th>{t("inventory.warehouse")}</th>
                <th>{t("inventory.product")}</th>
                <th>{t("purchasing.status")}</th>
                <th>{t("purchasing.payment")}</th>
                <th>{t("purchasing.total")}</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {rows.map((o) => (
                <tr key={o.id}>
                  <td>{o.reference || o.id.slice(0, 8)}</td>
                  <td>{warehouseLabel(o.warehouseId)}</td>
                  <td>{orderProducts(o)}</td>
                  <td>{statusLabel(o.status)}</td>
                  <td>{paymentLabel(o.paymentStatus)}</td>
                  <td>{o.grandTotal}</td>
                  <td className="toolbar">
                    {o.status === "Draft" ? (
                      <button
                        type="button"
                        className="btn secondary"
                        disabled={busy}
                        onClick={() => void onConfirm(o.id)}
                      >
                        {t("purchasing.confirm")}
                      </button>
                    ) : null}
                    {o.status === "Confirmed" || o.status === "PartiallyReceived" ? (
                      <button type="button" className="btn" disabled={busy} onClick={() => void onReceive(o.id)}>
                        {t("purchasing.receive")}
                      </button>
                    ) : null}
                    {o.paymentStatus !== "Paid" && o.status !== "Draft" && o.status !== "Cancelled" ? (
                      <button
                        type="button"
                        className="btn secondary"
                        disabled={busy}
                        onClick={() => void onMarkPaid(o.id)}
                      >
                        {t("purchasing.markPaid")}
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
        open={supplierOpen}
        title={t("purchasing.addSupplier")}
        onClose={() => setSupplierOpen(false)}
        footer={
          <>
            <button type="button" className="btn secondary" onClick={() => setSupplierOpen(false)}>
              {t("common.cancel")}
            </button>
            <button type="submit" form="supplier-form" className="btn" disabled={busy}>
              {busy ? t("common.saving") : t("purchasing.saveSupplier")}
            </button>
          </>
        }
      >
        <form id="supplier-form" className="stack" onSubmit={(e) => void onCreateSupplier(e)}>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="sup-code">{t("purchasing.supplierCode")}</label>
              <input
                id="sup-code"
                value={supplierForm.code}
                onChange={(e) => setSupplierForm({ ...supplierForm, code: e.target.value })}
                required
              />
            </div>
            <div className="field">
              <label htmlFor="sup-name">{t("purchasing.supplierName")}</label>
              <input
                id="sup-name"
                value={supplierForm.name}
                onChange={(e) => setSupplierForm({ ...supplierForm, name: e.target.value })}
                required
              />
            </div>
          </div>
        </form>
      </Modal>

      <Modal
        open={poOpen}
        title={t("purchasing.addOrder")}
        onClose={() => setPoOpen(false)}
        footer={
          <>
            <button type="button" className="btn secondary" onClick={() => setPoOpen(false)}>
              {t("common.cancel")}
            </button>
            <button
              type="submit"
              form="po-form"
              className="btn"
              disabled={busy || !suppliers.length || !warehouses.length || !products.length}
            >
              {busy ? t("common.saving") : t("purchasing.saveOrder")}
            </button>
          </>
        }
      >
        <form id="po-form" className="stack" onSubmit={(e) => void onCreatePo(e)}>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="po-sup">{t("purchasing.supplier")}</label>
              <select
                id="po-sup"
                value={poForm.supplierId}
                onChange={(e) => setPoForm({ ...poForm, supplierId: e.target.value })}
                required
              >
                {suppliers.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.code} — {s.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="field">
              <label htmlFor="po-wh">{t("inventory.warehouses")}</label>
              <select
                id="po-wh"
                value={poForm.warehouseId}
                onChange={(e) => setPoForm({ ...poForm, warehouseId: e.target.value })}
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
              <label htmlFor="po-prod">{t("inventory.product")}</label>
              <select
                id="po-prod"
                value={poForm.productId}
                onChange={(e) => setPoForm({ ...poForm, productId: e.target.value })}
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
              <label htmlFor="po-qty">{t("inventory.quantity")}</label>
              <input
                id="po-qty"
                type="number"
                min="0.000001"
                step="any"
                value={poForm.quantity}
                onChange={(e) => setPoForm({ ...poForm, quantity: e.target.value })}
                required
              />
            </div>
            <div className="field">
              <label htmlFor="po-ref">{t("purchasing.reference")}</label>
              <input
                id="po-ref"
                value={poForm.reference}
                onChange={(e) => setPoForm({ ...poForm, reference: e.target.value })}
              />
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
}
