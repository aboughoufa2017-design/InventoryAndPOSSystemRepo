import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import {
  confirmSale,
  createCustomer,
  createSale,
  getSale,
  listCustomers,
  listProducts,
  listSales,
  listWarehouses,
  shipSale,
  updateSalePayment,
  type Customer,
  type Product,
  type SalesOrder,
  type Warehouse,
} from "../api/resources";
import { DataTable, DEFAULT_PAGE_SIZE, paginate } from "../components/DataTable";
import { Modal } from "../components/Modal";

export function SalesPage() {
  const { t } = useTranslation();
  const [orders, setOrders] = useState<SalesOrder[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [page, setPage] = useState(1);
  const [customerOpen, setCustomerOpen] = useState(false);
  const [saleOpen, setSaleOpen] = useState(false);
  const [customerForm, setCustomerForm] = useState({ code: "", name: "" });
  const [saleForm, setSaleForm] = useState({
    customerId: "",
    warehouseId: "",
    productId: "",
    quantity: "1",
    reference: "",
  });

  const load = useCallback(async () => {
    setError(null);
    const [o, c, w, p] = await Promise.all([listSales(), listCustomers(), listWarehouses(), listProducts()]);
    if (o.error || c.error || w.error || p.error) {
      setError(t("sales.loadError"));
      return;
    }
    setOrders(o.data ?? []);
    setCustomers(c.data ?? []);
    setWarehouses(w.data ?? []);
    setProducts(p.data ?? []);
    setSaleForm((prev) => ({
      ...prev,
      customerId: prev.customerId || c.data?.[0]?.id || "",
      warehouseId: prev.warehouseId || w.data?.[0]?.id || "",
      productId: prev.productId || p.data?.[0]?.id || "",
    }));
  }, [t]);

  useEffect(() => {
    void load();
  }, [load]);

  async function onCreateCustomer(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    const result = await createCustomer({
      code: customerForm.code.trim(),
      name: customerForm.name.trim(),
    });
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setOk(t("sales.customerCreated"));
    setCustomerForm({ code: "", name: "" });
    setCustomerOpen(false);
    void load();
  }

  async function onCreateSale(e: FormEvent) {
    e.preventDefault();
    const warehouse = warehouses.find((w) => w.id === saleForm.warehouseId);
    if (!warehouse) return;
    setBusy(true);
    setError(null);
    setOk(null);
    const product = products.find((p) => p.id === saleForm.productId);
    const result = await createSale({
      customerId: saleForm.customerId,
      warehouseId: warehouse.id,
      locationId: warehouse.generalLocationId,
      reference: saleForm.reference || undefined,
      lines: [
        {
          productId: saleForm.productId,
          quantity: Number(saleForm.quantity),
          unitPrice: product?.salePrice,
        },
      ],
    });
    setBusy(false);
    if (result.error || !result.data) {
      setError(result.error?.message ?? t("sales.createError"));
      return;
    }
    setOk(t("sales.created"));
    setSaleOpen(false);
    void load();
  }

  async function onConfirm(id: string) {
    setBusy(true);
    const detail = await getSale(id);
    if (detail.error || !detail.data) {
      setBusy(false);
      setError(detail.error?.message ?? t("sales.actionError"));
      return;
    }
    const result = await confirmSale(id, detail.data.rowVersion);
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setOk(t("sales.confirmed"));
    void load();
  }

  async function onShip(id: string) {
    setBusy(true);
    const detail = await getSale(id);
    if (detail.error || !detail.data) {
      setBusy(false);
      setError(detail.error?.message ?? t("sales.actionError"));
      return;
    }
    const lines = detail.data.lines
      .map((l) => ({
        salesOrderLineId: l.id,
        quantity: Math.max(0, Number(l.quantityOrdered) - Number(l.quantityShipped)),
      }))
      .filter((l) => l.quantity > 0);
    const result = await shipSale(id, detail.data.rowVersion, lines);
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setOk(t("sales.shipped"));
    void load();
  }

  async function onMarkPaid(id: string) {
    setBusy(true);
    setError(null);
    setOk(null);
    const detail = await getSale(id);
    if (detail.error || !detail.data) {
      setBusy(false);
      setError(detail.error?.message ?? t("sales.actionError"));
      return;
    }
    const result = await updateSalePayment(id, detail.data.grandTotal, detail.data.rowVersion);
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setOk(t("sales.markedPaid"));
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
  const orderProducts = (o: SalesOrder) =>
    o.lines.map((l) => productLabel(l.productId)).join(", ") || "—";
  const statusLabel = (status: string) => t(`status.${status}`, { defaultValue: status });
  const paymentLabel = (status: string) => t(`payment.${status}`, { defaultValue: status });

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("sales.title")}</h1>
          <p>{t("sales.hint")}</p>
        </div>
        <div className="page-actions">
          <button type="button" className="btn secondary" onClick={() => void load()}>
            {t("products.refresh")}
          </button>
          <button type="button" className="btn secondary" onClick={() => setCustomerOpen(true)}>
            {t("sales.addCustomer")}
          </button>
          <button type="button" className="btn" onClick={() => setSaleOpen(true)}>
            {t("sales.addOrder")}
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
          empty={<p className="muted">{t("sales.empty")}</p>}
        >
          <table className="data-table">
            <thead>
              <tr>
                <th>{t("sales.reference")}</th>
                <th>{t("inventory.warehouse")}</th>
                <th>{t("inventory.product")}</th>
                <th>{t("sales.status")}</th>
                <th>{t("sales.payment")}</th>
                <th>{t("sales.total")}</th>
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
                        {t("sales.confirm")}
                      </button>
                    ) : null}
                    {o.status === "Confirmed" || o.status === "PartiallyFulfilled" ? (
                      <button type="button" className="btn" disabled={busy} onClick={() => void onShip(o.id)}>
                        {t("sales.ship")}
                      </button>
                    ) : null}
                    {o.paymentStatus !== "Paid" && o.status !== "Draft" && o.status !== "Cancelled" ? (
                      <button
                        type="button"
                        className="btn secondary"
                        disabled={busy}
                        onClick={() => void onMarkPaid(o.id)}
                      >
                        {t("sales.markPaid")}
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
        open={customerOpen}
        title={t("sales.addCustomer")}
        onClose={() => setCustomerOpen(false)}
        footer={
          <>
            <button type="button" className="btn secondary" onClick={() => setCustomerOpen(false)}>
              {t("common.cancel")}
            </button>
            <button type="submit" form="customer-form" className="btn" disabled={busy}>
              {busy ? t("common.saving") : t("sales.saveCustomer")}
            </button>
          </>
        }
      >
        <form id="customer-form" className="stack" onSubmit={(e) => void onCreateCustomer(e)}>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="cu-code">{t("sales.customerCode")}</label>
              <input
                id="cu-code"
                value={customerForm.code}
                onChange={(e) => setCustomerForm({ ...customerForm, code: e.target.value })}
                required
              />
            </div>
            <div className="field">
              <label htmlFor="cu-name">{t("sales.customerName")}</label>
              <input
                id="cu-name"
                value={customerForm.name}
                onChange={(e) => setCustomerForm({ ...customerForm, name: e.target.value })}
                required
              />
            </div>
          </div>
        </form>
      </Modal>

      <Modal
        open={saleOpen}
        title={t("sales.addOrder")}
        onClose={() => setSaleOpen(false)}
        footer={
          <>
            <button type="button" className="btn secondary" onClick={() => setSaleOpen(false)}>
              {t("common.cancel")}
            </button>
            <button
              type="submit"
              form="sale-form"
              className="btn"
              disabled={busy || !customers.length || !warehouses.length || !products.length}
            >
              {busy ? t("common.saving") : t("sales.saveOrder")}
            </button>
          </>
        }
      >
        <form id="sale-form" className="stack" onSubmit={(e) => void onCreateSale(e)}>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="sale-cu">{t("sales.customer")}</label>
              <select
                id="sale-cu"
                value={saleForm.customerId}
                onChange={(e) => setSaleForm({ ...saleForm, customerId: e.target.value })}
                required
              >
                {customers.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.code} — {c.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="field">
              <label htmlFor="sale-wh">{t("inventory.warehouses")}</label>
              <select
                id="sale-wh"
                value={saleForm.warehouseId}
                onChange={(e) => setSaleForm({ ...saleForm, warehouseId: e.target.value })}
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
              <label htmlFor="sale-prod">{t("inventory.product")}</label>
              <select
                id="sale-prod"
                value={saleForm.productId}
                onChange={(e) => setSaleForm({ ...saleForm, productId: e.target.value })}
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
              <label htmlFor="sale-qty">{t("inventory.quantity")}</label>
              <input
                id="sale-qty"
                type="number"
                min="0.000001"
                step="any"
                value={saleForm.quantity}
                onChange={(e) => setSaleForm({ ...saleForm, quantity: e.target.value })}
                required
              />
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
}
