import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import {
  createProduct,
  listProducts,
  listUnits,
  updateProduct,
  type Product,
  type UnitOfMeasure,
} from "../api/resources";
import { DataTable, DEFAULT_PAGE_SIZE, paginate } from "../components/DataTable";
import { Modal } from "../components/Modal";

const emptyForm = {
  sku: "",
  name: "",
  unitOfMeasureId: "",
  purchasePrice: "0",
  salePrice: "0",
  barcode: "",
  isActive: true,
};

export function ProductsPage() {
  const { t } = useTranslation();
  const [items, setItems] = useState<Product[]>([]);
  const [units, setUnits] = useState<UnitOfMeasure[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [page, setPage] = useState(1);
  const [dialog, setDialog] = useState<"create" | "edit" | null>(null);
  const [editing, setEditing] = useState<Product | null>(null);
  const [form, setForm] = useState(emptyForm);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    const [products, unitList] = await Promise.all([listProducts(), listUnits()]);
    if (products.error || unitList.error) {
      setError(t("products.loadError"));
    } else {
      setItems(products.data ?? []);
      const activeUnits = (unitList.data ?? []).filter((u) => u.isActive);
      setUnits(activeUnits);
      setForm((prev) => ({
        ...prev,
        unitOfMeasureId: prev.unitOfMeasureId || activeUnits[0]?.id || "",
      }));
    }
    setLoading(false);
  }, [t]);

  useEffect(() => {
    void load();
  }, [load]);

  function openCreate() {
    setEditing(null);
    setForm({
      ...emptyForm,
      unitOfMeasureId: units[0]?.id || "",
    });
    setDialog("create");
  }

  function openEdit(product: Product) {
    setEditing(product);
    setForm({
      sku: product.sku,
      name: product.name,
      unitOfMeasureId: product.unitOfMeasureId,
      purchasePrice: String(product.purchasePrice),
      salePrice: String(product.salePrice),
      barcode: product.barcodes.find((b) => b.isPrimary)?.barcode ?? product.barcodes[0]?.barcode ?? "",
      isActive: product.isActive,
    });
    setDialog("edit");
  }

  async function onSave(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    setOk(null);
    if (dialog === "create") {
      const result = await createProduct({
        sku: form.sku.trim(),
        name: form.name.trim(),
        unitOfMeasureId: form.unitOfMeasureId,
        purchasePrice: Number(form.purchasePrice),
        salePrice: Number(form.salePrice),
        barcodes: form.barcode.trim() ? [form.barcode.trim()] : undefined,
      });
      setBusy(false);
      if (result.error || !result.data) {
        setError(result.error?.message ?? t("products.createError"));
        return;
      }
      setOk(t("products.created"));
    } else if (editing) {
      const result = await updateProduct(editing.id, {
        name: form.name.trim(),
        unitOfMeasureId: form.unitOfMeasureId,
        purchasePrice: Number(form.purchasePrice),
        salePrice: Number(form.salePrice),
        isTaxable: editing.isTaxable,
        trackInventory: editing.trackInventory,
        isActive: form.isActive,
        categoryId: editing.categoryId,
        description: editing.description,
        brand: editing.brand,
        minimumStock: editing.minimumStock,
        maximumStock: editing.maximumStock,
        reorderPoint: editing.reorderPoint,
        expectedRowVersion: editing.rowVersion,
      });
      setBusy(false);
      if (result.error || !result.data) {
        setError(result.error?.message ?? t("products.updateError"));
        return;
      }
      setOk(t("products.updated"));
    } else {
      setBusy(false);
      return;
    }
    setDialog(null);
    setEditing(null);
    void load();
  }

  const rows = paginate(items, page, DEFAULT_PAGE_SIZE);

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("products.title")}</h1>
          <p>{t("products.hint")}</p>
        </div>
        <div className="page-actions">
          <button type="button" className="btn secondary" onClick={() => void load()}>
            {t("products.refresh")}
          </button>
          <button type="button" className="btn" onClick={openCreate}>
            {t("products.add")}
          </button>
        </div>
      </div>

      {ok ? <p className="ok-text">{ok}</p> : null}
      {error ? <p className="error-text">{error}</p> : null}

      <div className="panel">
        {loading ? <p className="muted">{t("app.loading")}</p> : null}
        <DataTable
          page={page}
          total={items.length}
          onPageChange={setPage}
          empty={!loading ? <p className="muted">{t("products.empty")}</p> : null}
        >
          <table className="data-table">
            <thead>
              <tr>
                <th>{t("products.sku")}</th>
                <th>{t("products.name")}</th>
                <th>{t("products.unit")}</th>
                <th>{t("products.purchasePrice")}</th>
                <th>{t("products.salePrice")}</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {rows.map((p) => (
                <tr key={p.id}>
                  <td>{p.sku}</td>
                  <td>{p.name}</td>
                  <td>{p.unitOfMeasureCode}</td>
                  <td>{p.purchasePrice}</td>
                  <td>{p.salePrice}</td>
                  <td className="toolbar">
                    <span className={`badge${p.isActive ? " ok" : " danger"}`}>
                      {p.isActive ? t("products.active") : t("products.inactive")}
                    </span>
                    <button type="button" className="btn secondary" onClick={() => openEdit(p)}>
                      {t("common.edit")}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </DataTable>
      </div>

      <Modal
        open={dialog !== null}
        title={dialog === "edit" ? t("products.edit") : t("products.add")}
        onClose={() => setDialog(null)}
        footer={
          <>
            <button type="button" className="btn secondary" onClick={() => setDialog(null)}>
              {t("common.cancel")}
            </button>
            <button type="submit" form="product-form" className="btn" disabled={busy || !form.unitOfMeasureId}>
              {busy ? t("common.saving") : t("products.save")}
            </button>
          </>
        }
      >
        <form id="product-form" className="stack" onSubmit={(e) => void onSave(e)}>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="sku">{t("products.sku")}</label>
              <input
                id="sku"
                value={form.sku}
                onChange={(e) => setForm({ ...form, sku: e.target.value })}
                required
                disabled={dialog === "edit"}
              />
            </div>
            <div className="field">
              <label htmlFor="pname">{t("products.name")}</label>
              <input
                id="pname"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                required
              />
            </div>
            <div className="field">
              <label htmlFor="unit">{t("products.unit")}</label>
              <select
                id="unit"
                value={form.unitOfMeasureId}
                onChange={(e) => setForm({ ...form, unitOfMeasureId: e.target.value })}
                required
              >
                {units.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.code} — {u.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="field">
              <label htmlFor="purchase">{t("products.purchasePrice")}</label>
              <input
                id="purchase"
                type="number"
                min="0"
                step="0.01"
                value={form.purchasePrice}
                onChange={(e) => setForm({ ...form, purchasePrice: e.target.value })}
                required
              />
            </div>
            <div className="field">
              <label htmlFor="sale">{t("products.salePrice")}</label>
              <input
                id="sale"
                type="number"
                min="0"
                step="0.01"
                value={form.salePrice}
                onChange={(e) => setForm({ ...form, salePrice: e.target.value })}
                required
              />
            </div>
            {dialog === "create" ? (
              <div className="field">
                <label htmlFor="barcode">{t("products.barcode")}</label>
                <input
                  id="barcode"
                  value={form.barcode}
                  onChange={(e) => setForm({ ...form, barcode: e.target.value })}
                />
              </div>
            ) : (
              <div className="field">
                <label htmlFor="p-active">{t("products.active")}</label>
                <select
                  id="p-active"
                  value={form.isActive ? "1" : "0"}
                  onChange={(e) => setForm({ ...form, isActive: e.target.value === "1" })}
                >
                  <option value="1">{t("common.yes")}</option>
                  <option value="0">{t("common.no")}</option>
                </select>
              </div>
            )}
          </div>
        </form>
      </Modal>
    </div>
  );
}
