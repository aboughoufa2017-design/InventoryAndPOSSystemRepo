import type { ApiError } from "./client";

const PLATFORM_TOKEN_KEY = "warehouse.platform.accessToken";
const PLATFORM_USER_KEY = "warehouse.platform.operator";

export type PlatformOperator = {
  operatorId: string;
  email: string;
  displayName: string;
};

export type PlatformAuthResponse = {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  operatorId: string;
  email: string;
  displayName: string;
};

export type Plan = {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  maxUsers: number | null;
  maxWarehouses: number | null;
  maxDevices: number | null;
  maxProducts: number | null;
  featureCodes: string[];
};

export type PlatformTenant = {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  isSuspended: boolean;
  currencyCode: string;
  timeZoneId: string;
  taxEnabled: boolean;
  taxName: string | null;
  taxRate: number | null;
  maxOfflineDays: number | null;
  requireOnlineRevalidation: boolean;
  negativeStockPolicy: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  rowVersion: number;
};

export type Subscription = {
  id: string;
  tenantId: string;
  planId: string;
  planCode: string;
  planName: string;
  status: "Trial" | "Active" | "PastDue" | "Cancelled";
  trialEndsAtUtc: string | null;
  currentPeriodStartUtc: string | null;
  currentPeriodEndUtc: string | null;
  externalReference: string | null;
  rowVersion: number;
};

export type TenantUsage = {
  tenantId: string;
  userCount: number;
  warehouseCount: number;
  deviceCount: number;
  activeDeviceCount: number;
  productCount: number;
  planMaxUsers: number | null;
  planMaxWarehouses: number | null;
  planMaxDevices: number | null;
  planMaxProducts: number | null;
};

export type TenantReadiness = {
  tenantId: string;
  hasOwnerUser: boolean;
  hasWarehouse: boolean;
  hasProduct: boolean;
  hasActiveSubscription: boolean;
  isSuspended: boolean;
  missingSteps: string[];
};

export type ProvisionTenantInput = {
  tenantCode: string;
  tenantName: string;
  currencyCode: string;
  timeZoneId: string;
  ownerEmail: string;
  ownerPassword: string;
  ownerDisplayName: string;
  planCode?: string | null;
};

function apiBase() {
  return import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ?? "";
}

export function getPlatformToken() {
  return sessionStorage.getItem(PLATFORM_TOKEN_KEY);
}

export function getPlatformOperator(): PlatformOperator | null {
  const raw = sessionStorage.getItem(PLATFORM_USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as PlatformOperator;
  } catch {
    return null;
  }
}

export function storePlatformSession(accessToken: string, operator: PlatformOperator) {
  sessionStorage.setItem(PLATFORM_TOKEN_KEY, accessToken);
  sessionStorage.setItem(PLATFORM_USER_KEY, JSON.stringify(operator));
}

export function clearPlatformSession() {
  sessionStorage.removeItem(PLATFORM_TOKEN_KEY);
  sessionStorage.removeItem(PLATFORM_USER_KEY);
}

async function platformFetch<T>(
  path: string,
  init: RequestInit = {},
): Promise<{ data?: T; error?: ApiError; status: number }> {
  const headers = new Headers(init.headers);
  if (!headers.has("Content-Type") && init.body) {
    headers.set("Content-Type", "application/json");
  }
  const token = getPlatformToken();
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${apiBase()}${path}`, { ...init, headers });
  if (response.status === 204) {
    return { status: 204 };
  }

  const text = await response.text();
  const payload = text ? (JSON.parse(text) as unknown) : null;

  if (!response.ok) {
    const err = payload as ApiError | null;
    return {
      status: response.status,
      error: {
        code: err?.code ?? "INTERNAL_ERROR",
        message: err?.message ?? response.statusText,
        correlationId: err?.correlationId,
      },
    };
  }

  return { status: response.status, data: payload as T };
}

export async function platformLogin(email: string, password: string) {
  const response = await fetch(`${apiBase()}/api/v1/platform/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });
  const text = await response.text();
  const payload = text ? (JSON.parse(text) as PlatformAuthResponse & ApiError) : null;
  if (!response.ok || !payload || !("accessToken" in payload) || !payload.accessToken) {
    return {
      status: response.status,
      error: {
        code: (payload as ApiError | null)?.code ?? "UNAUTHORIZED",
        message: (payload as ApiError | null)?.message ?? response.statusText,
      } satisfies ApiError,
    };
  }

  const data = payload as PlatformAuthResponse;
  storePlatformSession(data.accessToken, {
    operatorId: data.operatorId,
    email: data.email,
    displayName: data.displayName,
  });
  return { status: response.status, data };
}

export async function listPlans() {
  return platformFetch<Plan[]>("/api/v1/platform/plans");
}

export async function listTenants() {
  return platformFetch<PlatformTenant[]>("/api/v1/platform/tenants");
}

export async function getTenant(tenantId: string) {
  return platformFetch<PlatformTenant>(`/api/v1/platform/tenants/${tenantId}`);
}

export async function provisionTenant(input: ProvisionTenantInput) {
  return platformFetch<PlatformTenant>("/api/v1/platform/tenants", {
    method: "POST",
    body: JSON.stringify(input),
  });
}

export async function suspendTenant(tenantId: string) {
  return platformFetch<PlatformTenant>(`/api/v1/platform/tenants/${tenantId}/suspend`, {
    method: "POST",
  });
}

export async function resumeTenant(tenantId: string) {
  return platformFetch<PlatformTenant>(`/api/v1/platform/tenants/${tenantId}/resume`, {
    method: "POST",
  });
}

export async function getSubscription(tenantId: string) {
  return platformFetch<Subscription>(`/api/v1/platform/tenants/${tenantId}/subscription`);
}

export async function updateSubscription(
  tenantId: string,
  body: {
    planCode: string;
    status: Subscription["status"];
    trialEndsAtUtc?: string | null;
    currentPeriodStartUtc?: string | null;
    currentPeriodEndUtc?: string | null;
    externalReference?: string | null;
    expectedRowVersion: number;
  },
) {
  return platformFetch<Subscription>(`/api/v1/platform/tenants/${tenantId}/subscription`, {
    method: "PUT",
    body: JSON.stringify(body),
  });
}

export async function getUsage(tenantId: string) {
  return platformFetch<TenantUsage>(`/api/v1/platform/tenants/${tenantId}/usage`);
}

export async function getReadiness(tenantId: string) {
  return platformFetch<TenantReadiness>(`/api/v1/platform/tenants/${tenantId}/readiness`);
}
