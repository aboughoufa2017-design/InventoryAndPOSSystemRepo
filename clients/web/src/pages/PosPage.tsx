import { useEffect, useMemo, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { apiFetch } from "../api/client";

type Warehouse = { id: string; code: string; name: string };
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
type PosSale = {
  id: string;
  receiptNumber: string;
  grandTotal: number;
  status: string;
  soldAtUtc: string;
  lines: { skuSnapshot: string; nameSnapshot: string; quantity: number; lineTotal: number }[];
};

function newCommandId() {
  return crypto.randomUUID();
}

export function PosPage() {
  const { t } = useTranslation();
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [warehouseId, setWarehouseId] = useState("");
  const [barcode, setBarcode] = useState("");
  const [cart, setCart] = useState<CartLine[]>([]);
  const [paymentMethod, setPaymentMethod] = useState<"Cash" | "Credit">("Cash");
  const [error, setError] = useState<string | null>(null);
  const [receipt, setReceipt] = useState<PosSale | null>(null);
  const [busy, setBusy] = useState(false);
  const [shifts, setShifts] = useState<{ id: string; status: string; warehouseId: string }[]>([]);

  const total = useMemo(
    () => cart.reduce((sum, line) => sum + line.quantity * line.unitPrice, 0),
    [cart],
  );

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
  }

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("pos.title")}</h1>
          <p>{t("pos.hint")}</p>
        </div>
        <button className="btn secondary" type="button" onClick={() => window.print()} disabled={!receipt}>
          {t("pos.printReceipt")}
        </button>
      </div>
      {error ? <p className="error-text">{error}</p> : null}

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

      {receipt ? (
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
              {t("pos.total")}: {receipt.grandTotal} LYD
            </strong>
          </p>
        </div>
      ) : null}
    </div>
  );
}
