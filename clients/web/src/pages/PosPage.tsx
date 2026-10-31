import { useCallback, useEffect, useMemo, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { apiFetch } from "../api/client";
import {
  getPosSale,
  listPosSales,
  type PosSale,
  type Warehouse,
} from "../api/resources";
import { DataTable, DEFAULT_PAGE_SIZE, paginate } from "../components/DataTable";
import { Modal } from "../components/Modal";

type Product = {
  id: string;
  sku: string;
  name: string;
  salePrice: number;
  trackInventory: boolean;
};
type CartLine = {
  productId: string;
  sku: string;
  name: string;
  quantity: number;
  unitPrice: number;
};
type PosView = "checkout" | "receipts";

function newCommandId() {
  return crypto.randomUUID();
}

function toStartOfDayUtc(dateLocal: string): string | undefined {
  if (!dateLocal) return undefined;
  const d = new Date(`${dateLocal}T00:00:00`);
  return Number.isNaN(d.getTime()) ? undefined : d.toISOString();
}

function toEndOfDayUtc(dateLocal: string): string | undefined {
  if (!dateLocal) return undefined;
  const d = new Date(`${dateLocal}T23:59:59.999`);
  return Number.isNaN(d.getTime()) ? undefined : d.toISOString();
}

function paymentLabel(sale: PosSale): string {
  if (!sale.payments?.length) return "—";
  return sale.payments.map((p) => p.method).join(", ");
}

export function PosPage() {
  const { t } = useTranslation();
  const [view, setView] = useState<PosView>("checkout");
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [warehouseId, setWarehouseId] = useState("");
  const [barcode, setBarcode] = useState("");
  const [cart, setCart] = useState<CartLine[]>([]);
  const [paymentMethod, setPaymentMethod] = useState<"Cash" | "Credit">("Cash");
  const [error, setError] = useState<string | null>(null);
  const [receipt, setReceipt] = useState<PosSale | null>(null);
  const [busy, setBusy] = useState(false);
  const [shifts, setShifts] = useState<{ id: string; status: string; warehouseId: string }[]>([]);

  const [sales, setSales] = useState<PosSale[]>([]);
  const [receiptsWarehouseId, setReceiptsWarehouseId] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [page, setPage] = useState(1);
  const [receiptsBusy, setReceiptsBusy] = useState(false);
  const [detailOpen, setDetailOpen] = useState(false);

  const total = useMemo(
    () => cart.reduce((sum, line) => sum + line.quantity * line.unitPrice, 0),
    [cart],
  );

  const warehouseName = useCallback(
    (id: string) => {
      const w = warehouses.find((x) => x.id === id);
      return w ? `${w.code} — ${w.name}` : id;
    },
    [warehouses],
  );

  const loadReceipts = useCallback(async () => {
    setReceiptsBusy(true);
    setError(null);
    const result = await listPosSales({
      warehouseId: receiptsWarehouseId || undefined,
      fromUtc: toStartOfDayUtc(fromDate),
      toUtc: toEndOfDayUtc(toDate),
    });
    setReceiptsBusy(false);
    if (result.error) {
      setError(result.error.message ?? t("pos.loadError"));
      return;
    }
    setSales(result.data ?? []);
    setPage(1);
  }, [fromDate, receiptsWarehouseId, t, toDate]);

  useEffect(() => {
    void (async () => {
      const wh = await apiFetch<Warehouse[]>("/api/v1/warehouses");
      if (!wh.error && wh.data) {
        setWarehouses(wh.data);
        if (wh.data[0]) setWarehouseId(wh.data[0].id);
      }
      const sh = await apiFetch<{ id: string; status: string; warehouseId: string }[]>("/api/v1/pos/shifts");
      if (!sh.error && sh.data) setShifts(sh.data.filter((s) => s.status === "Open"));
    })();
  }, []);

  useEffect(() => {
    if (view === "receipts") void loadReceipts();
  }, [view, loadReceipts]);

  async function onLookup(e: FormEvent) {
    e.preventDefault();
    setError(null);
    const q = barcode.trim();
    if (!q) return;
    const result = await apiFetch<Product>(`/api/v1/pos/products/lookup?barcode=${encodeURIComponent(q)}`);
    if (result.error) {
      const bySku = await apiFetch<Product>(`/api/v1/pos/products/lookup?sku=${encodeURIComponent(q)}`);
      if (bySku.error || !bySku.data) {
        setError(result.error?.message ?? t("pos.lookupError"));
        return;
      }
      addToCart(bySku.data);
      setBarcode("");
      return;
    }
    if (result.data) {
      addToCart(result.data);
      setBarcode("");
    }
  }

  function addToCart(product: Product) {
    setCart((prev) => {
      const existing = prev.find((l) => l.productId === product.id);
      if (existing) {
        return prev.map((l) =>
          l.productId === product.id ? { ...l, quantity: l.quantity + 1 } : l,
        );
      }
      return [
        ...prev,
        {
          productId: product.id,
          sku: product.sku,
          name: product.name,
          quantity: 1,
          unitPrice: product.salePrice,
        },
      ];
    });
  }

  async function openShift() {
    if (!warehouseId) return;
    setBusy(true);
    const result = await apiFetch("/api/v1/pos/shifts", {
      method: "POST",
      body: JSON.stringify({ warehouseId, deviceId: null, openingFloat: 0 }),
    });
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    const sh = await apiFetch<{ id: string; status: string; warehouseId: string }[]>("/api/v1/pos/shifts");
    if (sh.data) setShifts(sh.data.filter((s) => s.status === "Open"));
  }

  async function checkout() {
    if (!warehouseId || cart.length === 0) return;
    setBusy(true);
    setError(null);
    const openShiftId = shifts.find((s) => s.warehouseId === warehouseId)?.id ?? null;
    const result = await apiFetch<PosSale>("/api/v1/pos/sales", {
      method: "POST",
      body: JSON.stringify({
        clientCommandId: newCommandId(),
        warehouseId,
        locationId: null,
        deviceId: null,
        customerId: null,
        cashShiftId: openShiftId,
        currencyCode: "LYD",
        notes: null,
        paymentMethod: paymentMethod === "Cash" ? 0 : 1,
        lines: cart.map((l) => ({
          productId: l.productId,
          quantity: l.quantity,
          unitPrice: l.unitPrice,
          discountAmount: 0,
        })),
      }),
    });
    setBusy(false);
    if (result.error) {
      setError(result.error.message);
      return;
    }
    setReceipt(result.data ?? null);
    setCart([]);
    if (result.data) {
      setSales((prev) => [result.data!, ...prev.filter((s) => s.id !== result.data!.id)]);
    }
  }

  async function viewReceipt(id: string) {
    setBusy(true);
    setError(null);
    const result = await getPosSale(id);
    setBusy(false);
    if (result.error || !result.data) {
      setError(result.error?.message ?? t("pos.loadError"));
      return;
    }
    setReceipt(result.data);
    setDetailOpen(true);
  }

  const pagedSales = paginate(sales, page, DEFAULT_PAGE_SIZE);

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("pos.title")}</h1>
          <p>{view === "checkout" ? t("pos.hint") : t("pos.receiptsHint")}</p>
        </div>
        <div className="toolbar">
          <button
            className={view === "checkout" ? "btn" : "btn secondary"}
            type="button"
            onClick={() => setView("checkout")}
          >
            {t("pos.tabCheckout")}
          </button>
          <button
            className={view === "receipts" ? "btn" : "btn secondary"}
            type="button"
            onClick={() => setView("receipts")}
          >
            {t("pos.tabReceipts")}
          </button>
          <button className="btn secondary" type="button" onClick={() => window.print()} disabled={!receipt}>
            {t("pos.printReceipt")}
          </button>
        </div>
      </div>
      {error ? <p className="error-text">{error}</p> : null}

      {view === "checkout" ? (
        <>
          <div className="panel toolbar">
            <label>
              {t("inventory.warehouses")}
              <select value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)}>
                {warehouses.map((w) => (
                  <option key={w.id} value={w.id}>
                    {w.code} — {w.name}
                  </option>
                ))}
              </select>
            </label>
            <button className="btn secondary" type="button" disabled={busy || !warehouseId} onClick={() => void openShift()}>
              {t("pos.openShift")}
            </button>
            <label>
              {t("pos.payment")}
              <select value={paymentMethod} onChange={(e) => setPaymentMethod(e.target.value as "Cash" | "Credit")}>
                <option value="Cash">{t("pos.cash")}</option>
                <option value="Credit">{t("pos.credit")}</option>
              </select>
            </label>
          </div>

          <form className="panel toolbar" onSubmit={(e) => void onLookup(e)}>
            <label style={{ flex: 1 }}>
              {t("pos.scan")}
              <input
                value={barcode}
                onChange={(e) => setBarcode(e.target.value)}
                placeholder={t("pos.scanPlaceholder")}
                autoFocus
              />
            </label>
            <button className="btn" type="submit">
              {t("pos.add")}
            </button>
          </form>

          <div className="panel">
            {cart.length === 0 ? <p className="muted">{t("pos.emptyCart")}</p> : null}
            {cart.length > 0 ? (
              <table className="data-table">
                <thead>
                  <tr>
                    <th>SKU</th>
                    <th>{t("products.name")}</th>
                    <th>{t("inventory.quantity")}</th>
                    <th>{t("pos.price")}</th>
                    <th>{t("pos.lineTotal")}</th>
                  </tr>
                </thead>
                <tbody>
                  {cart.map((l) => (
                    <tr key={l.productId}>
                      <td>{l.sku}</td>
                      <td>{l.name}</td>
                      <td>
                        <input
                          type="number"
                          min={0.001}
                          step="any"
                          value={l.quantity}
                          onChange={(e) => {
                            const q = Number(e.target.value);
                            setCart((prev) =>
                              prev.map((x) => (x.productId === l.productId ? { ...x, quantity: q } : x)),
                            );
                          }}
                        />
                      </td>
                      <td>{l.unitPrice}</td>
                      <td>{(l.quantity * l.unitPrice).toFixed(2)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : null}
            <div className="toolbar" style={{ marginTop: "1rem" }}>
              <strong>
                {t("pos.total")}: {total.toFixed(2)} LYD
              </strong>
              <button className="btn" type="button" disabled={busy || cart.length === 0} onClick={() => void checkout()}>
                {t("pos.checkout")}
              </button>
            </div>
          </div>

          {receipt && !detailOpen ? (
            <div className="panel receipt-print">
              <h2>{t("pos.receipt")}</h2>
              <p>
                {receipt.receiptNumber} · {new Date(receipt.soldAtUtc).toLocaleString()} · {receipt.status}
              </p>
              <ul>
                {receipt.lines.map((l, i) => (
                  <li key={`${l.skuSnapshot}-${i}`}>
                    {l.nameSnapshot} × {l.quantity} = {l.lineTotal}
                  </li>
                ))}
              </ul>
              <p>
                <strong>
                  {t("pos.total")}: {receipt.grandTotal} {receipt.currencyCode || "LYD"}
                </strong>
              </p>
            </div>
          ) : null}
        </>
      ) : (
        <>
          <div className="panel toolbar">
            <label>
              {t("inventory.warehouses")}
              <select value={receiptsWarehouseId} onChange={(e) => setReceiptsWarehouseId(e.target.value)}>
                <option value="">{t("common.all")}</option>
                {warehouses.map((w) => (
                  <option key={w.id} value={w.id}>
                    {w.code} — {w.name}
                  </option>
                ))}
              </select>
            </label>
            <label>
              {t("pos.fromDate")}
              <input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
            </label>
            <label>
              {t("pos.toDate")}
              <input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
            </label>
            <button
              className="btn secondary"
              type="button"
              disabled={receiptsBusy}
              onClick={() => void loadReceipts()}
            >
              {t("pos.refresh")}
            </button>
          </div>

          <div className="panel">
            <DataTable
              page={page}
              total={sales.length}
              onPageChange={setPage}
              empty={<p className="muted">{t("pos.emptyReceipts")}</p>}
            >
              <table className="data-table">
                <thead>
                  <tr>
                    <th>{t("pos.receiptNumber")}</th>
                    <th>{t("pos.soldAt")}</th>
                    <th>{t("inventory.warehouses")}</th>
                    <th>{t("pos.status")}</th>
                    <th>{t("pos.paymentMethod")}</th>
                    <th>{t("pos.total")}</th>
                    <th />
                  </tr>
                </thead>
                <tbody>
                  {pagedSales.map((s) => (
                    <tr key={s.id}>
                      <td>{s.receiptNumber}</td>
                      <td>{new Date(s.soldAtUtc).toLocaleString()}</td>
                      <td>{warehouseName(s.warehouseId)}</td>
                      <td>{s.status}</td>
                      <td>{paymentLabel(s)}</td>
                      <td>
                        {s.grandTotal} {s.currencyCode || "LYD"}
                      </td>
                      <td>
                        <button
                          type="button"
                          className="btn secondary"
                          disabled={busy}
                          onClick={() => void viewReceipt(s.id)}
                        >
                          {t("pos.viewReceipt")}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </DataTable>
          </div>
        </>
      )}

      <Modal
        open={detailOpen && !!receipt}
        title={t("pos.receipt")}
        onClose={() => setDetailOpen(false)}
        footer={
          <>
            <button type="button" className="btn secondary" onClick={() => setDetailOpen(false)}>
              {t("common.close")}
            </button>
            <button type="button" className="btn" onClick={() => window.print()}>
              {t("pos.printReceipt")}
            </button>
          </>
        }
      >
        {receipt ? (
          <div className="receipt-print stack">
            <p>
              {receipt.receiptNumber} · {new Date(receipt.soldAtUtc).toLocaleString()} · {receipt.status}
            </p>
            <p className="muted">{warehouseName(receipt.warehouseId)}</p>
            {receipt.payments?.length ? (
              <p>
                {t("pos.paymentMethod")}: {paymentLabel(receipt)}
              </p>
            ) : null}
            <table className="data-table">
              <thead>
                <tr>
                  <th>SKU</th>
                  <th>{t("products.name")}</th>
                  <th>{t("inventory.quantity")}</th>
                  <th>{t("pos.price")}</th>
                  <th>{t("pos.lineTotal")}</th>
                </tr>
              </thead>
              <tbody>
                {receipt.lines.map((l) => (
                  <tr key={l.id}>
                    <td>{l.skuSnapshot}</td>
                    <td>{l.nameSnapshot}</td>
                    <td>{l.quantity}</td>
                    <td>{l.unitPrice}</td>
                    <td>{l.lineTotal}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <p>
              <strong>
                {t("pos.total")}: {receipt.grandTotal} {receipt.currencyCode || "LYD"}
              </strong>
            </p>
          </div>
        ) : null}
      </Modal>
    </div>
  );
}
