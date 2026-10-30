import { apiFetch } from "./client";

export type UnitOfMeasure = {
  id: string;
  code: string;
  name: string;
  isSystem: boolean;
  isActive: boolean;
};

export type Product = {
  id: string;
  sku: string;
  name: string;
  description?: string | null;
  brand?: string | null;
  categoryId?: string | null;
  unitOfMeasureId: string;
  unitOfMeasureCode: string;
  purchasePrice: number;
  salePrice: number;
  isTaxable: boolean;
  isActive: boolean;
  trackInventory: boolean;
  minimumStock?: number | null;
  maximumStock?: number | null;
  reorderPoint?: number | null;
  applicableTaxRate: number;
  barcodes: { id: string; barcode: string; isPrimary: boolean }[];
  rowVersion: number;
};

export type Warehouse = {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  generalLocationId: string;
  inTransitLocationId: string;
};

export type WarehouseLocation = {
  id: string;
  warehouseId: string;
  code: string;
  name: string;
  kind: string;
  isDefault: boolean;
  isActive: boolean;
};

export type StockBalance = {
  id: string;
  productId: string;
  warehouseId: string;
  locationId: string;
  onHand: number;
  reserved: number;
  available: number;
  rowVersion: number;
};

export type StockAdjustment = {
  id: string;
  warehouseId: string;
  kind: string;
  status: string;
  reason?: string | null;
  rowVersion: number;
  lines: { id: string; productId: string; locationId: string; quantityChange: number }[];
};

export type Supplier = {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  rowVersion: number;
};

export type PurchaseOrder = {
  id: string;
  supplierId: string;
  warehouseId: string;
  locationId?: string | null;
  status: string;
  paymentStatus: string;
  reference?: string | null;
  grandTotal: number;
  paidAmount: number;
  rowVersion: number;
  lines: {
    id: string;
    productId: string;
    quantityOrdered: number;
    quantityReceived: number;
    unitPrice: number;
  }[];
};

export type Customer = {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  rowVersion: number;
};

export type SalesOrder = {
  id: string;
  customerId: string;
  warehouseId: string;
  locationId?: string | null;
  status: string;
  paymentStatus: string;
  reference?: string | null;
  grandTotal: number;
  paidAmount: number;
  rowVersion: number;
  lines: {
    id: string;
    productId: string;
    quantityOrdered: number;
    quantityShipped: number;
    unitPrice: number;
  }[];
};

export type Transfer = {
  id: string;
  sourceWarehouseId: string;
  destinationWarehouseId: string;
  sourceLocationId: string;
  destinationLocationId: string;
  status: string;
  notes?: string | null;
  rowVersion: number;
  lines: {
    id: string;
    productId: string;
    quantityRequested: number;
    quantityReceived: number;
    quantityInTransit: number;
  }[];
};

export type SyncPullResponse = {
  changes: {
    changeVersion: number;
    entityType: string;
    entityId: string;
    operation: string;
    payloadJson: string;
    occurredAtUtc: string;
  }[];
  latestChangeVersion: number;
  hasMore: boolean;
  security: {
    deviceRevoked: boolean;
    userDisabled: boolean;
    tenantSuspended: boolean;
    blocksApply: boolean;
  };
};

export type Role = { id: string; code: string; name: string; isSystem: boolean };
export type Permission = { id: string; code: string; name: string };

export const listUnits = () => apiFetch<UnitOfMeasure[]>("/api/v1/units");
export const listProducts = (search?: string) =>
  apiFetch<Product[]>(`/api/v1/products${search ? `?search=${encodeURIComponent(search)}` : ""}`);
export const createProduct = (body: {
  sku: string;
  name: string;
  unitOfMeasureId: string;
  purchasePrice: number;
  salePrice: number;
  isTaxable?: boolean;
  trackInventory?: boolean;
  categoryId?: string | null;
  description?: string | null;
  brand?: string | null;
  barcodes?: string[];
}) =>
  apiFetch<Product>("/api/v1/products", {
    method: "POST",
    body: JSON.stringify({
      isTaxable: false,
      trackInventory: true,
      ...body,
    }),
  });

export const updateProduct = (
  id: string,
  body: {
    name: string;
    unitOfMeasureId: string;
    purchasePrice: number;
    salePrice: number;
    isTaxable: boolean;
    trackInventory: boolean;
    isActive: boolean;
    categoryId?: string | null;
    description?: string | null;
    brand?: string | null;
    minimumStock?: number | null;
    maximumStock?: number | null;
    reorderPoint?: number | null;
    expectedRowVersion: number;
  },
) =>
  apiFetch<Product>(`/api/v1/products/${id}`, {
    method: "PUT",
    body: JSON.stringify(body),
  });

export const listWarehouses = () => apiFetch<Warehouse[]>("/api/v1/warehouses");
export const createWarehouse = (code: string, name: string) =>
  apiFetch<Warehouse>("/api/v1/warehouses", {
    method: "POST",
    body: JSON.stringify({ code, name }),
  });
export const updateWarehouse = (id: string, body: { name: string; isActive?: boolean }) =>
  apiFetch<Warehouse>(`/api/v1/warehouses/${id}`, {
    method: "PUT",
    body: JSON.stringify({ name: body.name, isActive: body.isActive ?? true }),
  });
export const listLocations = (warehouseId: string) =>
  apiFetch<WarehouseLocation[]>(`/api/v1/warehouses/${warehouseId}/locations`);

export const listBalances = (warehouseId?: string) =>
  apiFetch<StockBalance[]>(
    `/api/v1/inventory/balances${warehouseId ? `?warehouseId=${warehouseId}` : ""}`,
  );

export const createAdjustment = (body: {
  warehouseId: string;
  kind: string;
  reason?: string;
  lines: { productId: string; locationId: string; quantityChange: number; notes?: string }[];
}) =>
  apiFetch<StockAdjustment>("/api/v1/inventory/adjustments", {
    method: "POST",
    body: JSON.stringify(body),
  });

export const postAdjustment = (id: string) =>
  apiFetch<StockAdjustment>(`/api/v1/inventory/adjustments/${id}/post`, { method: "POST" });

export const listSuppliers = () => apiFetch<Supplier[]>("/api/v1/suppliers");
export const createSupplier = (body: { code: string; name: string; contactName?: string; email?: string; phone?: string }) =>
  apiFetch<Supplier>("/api/v1/suppliers", { method: "POST", body: JSON.stringify(body) });

export const listPurchases = () => apiFetch<PurchaseOrder[]>("/api/v1/purchases");
export const getPurchase = (id: string) => apiFetch<PurchaseOrder>(`/api/v1/purchases/${id}`);
export const createPurchase = (body: {
  supplierId: string;
  warehouseId: string;
  locationId?: string | null;
  reference?: string;
  notes?: string;
  lines: { productId: string; quantity: number; unitPrice?: number | null; notes?: string }[];
}) => apiFetch<PurchaseOrder>("/api/v1/purchases", { method: "POST", body: JSON.stringify(body) });
export const confirmPurchase = (id: string, expectedRowVersion: number) =>
  apiFetch<PurchaseOrder>(`/api/v1/purchases/${id}/confirm`, {
    method: "POST",
    body: JSON.stringify({ expectedRowVersion }),
  });
export const receivePurchase = (
  id: string,
  expectedRowVersion: number,
  lines: { purchaseOrderLineId: string; quantity: number }[],
) =>
  apiFetch<PurchaseOrder>(`/api/v1/purchases/${id}/receive`, {
    method: "POST",
    body: JSON.stringify({ expectedRowVersion, lines }),
  });
export const updatePurchasePayment = (id: string, paidAmount: number, expectedRowVersion: number) =>
  apiFetch<PurchaseOrder>(`/api/v1/purchases/${id}/payment`, {
    method: "PUT",
    body: JSON.stringify({ paidAmount, expectedRowVersion }),
  });

export const listCustomers = () => apiFetch<Customer[]>("/api/v1/customers");
export const createCustomer = (body: { code: string; name: string }) =>
  apiFetch<Customer>("/api/v1/customers", { method: "POST", body: JSON.stringify(body) });

export const listSales = () => apiFetch<SalesOrder[]>("/api/v1/sales");
export const getSale = (id: string) => apiFetch<SalesOrder>(`/api/v1/sales/${id}`);
export const createSale = (body: {
  customerId: string;
  warehouseId: string;
  locationId?: string | null;
  reference?: string;
  notes?: string;
  lines: { productId: string; quantity: number; unitPrice?: number | null; notes?: string }[];
}) => apiFetch<SalesOrder>("/api/v1/sales", { method: "POST", body: JSON.stringify(body) });
export const confirmSale = (id: string, expectedRowVersion: number) =>
  apiFetch<SalesOrder>(`/api/v1/sales/${id}/confirm`, {
    method: "POST",
    body: JSON.stringify({ expectedRowVersion }),
  });
export const shipSale = (
  id: string,
  expectedRowVersion: number,
  lines: { salesOrderLineId: string; quantity: number }[],
) =>
  apiFetch<SalesOrder>(`/api/v1/sales/${id}/ship`, {
    method: "POST",
    body: JSON.stringify({ expectedRowVersion, lines }),
  });
export const updateSalePayment = (id: string, paidAmount: number, expectedRowVersion: number) =>
  apiFetch<SalesOrder>(`/api/v1/sales/${id}/payment`, {
    method: "PUT",
    body: JSON.stringify({ paidAmount, expectedRowVersion }),
  });

export const listTransfers = () => apiFetch<Transfer[]>("/api/v1/transfers");
export const createTransfer = (body: {
  sourceWarehouseId: string;
  destinationWarehouseId: string;
  sourceLocationId?: string | null;
  destinationLocationId?: string | null;
  notes?: string;
  lines: { productId: string; quantity: number; notes?: string }[];
}) => apiFetch<Transfer>("/api/v1/transfers", { method: "POST", body: JSON.stringify(body) });
export const dispatchTransfer = (id: string, expectedRowVersion: number) =>
  apiFetch<Transfer>(`/api/v1/transfers/${id}/dispatch`, {
    method: "POST",
    body: JSON.stringify({ expectedRowVersion }),
  });
export const receiveTransfer = (
  id: string,
  expectedRowVersion: number,
  lines: { transferLineId: string; quantity: number }[],
) =>
  apiFetch<Transfer>(`/api/v1/transfers/${id}/receive`, {
    method: "POST",
    body: JSON.stringify({ expectedRowVersion, lines }),
  });

export const pullSync = (afterChangeVersion: number, limit = 50) =>
  apiFetch<SyncPullResponse>("/api/v1/sync/pull", {
    method: "POST",
    body: JSON.stringify({ afterChangeVersion, limit }),
  });

export type TenantUser = {
  id: string;
  email: string;
  displayName: string;
  isActive: boolean;
  isDisabled: boolean;
  rowVersion: number;
  roles: { roleId: string; roleCode: string; roleName: string }[];
};

export const listRoles = () => apiFetch<Role[]>("/api/v1/roles");
export const listPermissions = () => apiFetch<Permission[]>("/api/v1/permissions");
export const listUsers = () => apiFetch<TenantUser[]>("/api/v1/users");
export const createUser = (body: {
  email: string;
  password: string;
  displayName: string;
  roleIds: string[];
}) => apiFetch<TenantUser>("/api/v1/users", { method: "POST", body: JSON.stringify(body) });
export const setUserActive = (id: string, isActive: boolean, expectedRowVersion: number) =>
  apiFetch<TenantUser>(`/api/v1/users/${id}/set-active`, {
    method: "POST",
    body: JSON.stringify({ isActive, expectedRowVersion }),
  });
export const resetUserPassword = (id: string, newPassword: string, expectedRowVersion: number) =>
  apiFetch<TenantUser>(`/api/v1/users/${id}/reset-password`, {
    method: "POST",
    body: JSON.stringify({ newPassword, expectedRowVersion }),
  });
