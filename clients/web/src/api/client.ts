export type ApiError = {
  code: string;
  message: string;
  correlationId?: string;
};

const TOKEN_KEY = "warehouse.accessToken";
const REFRESH_KEY = "warehouse.refreshToken";
const USER_KEY = "warehouse.user";

export type StoredUser = {
  userId: string;
  tenantId: string;
  email: string;
  displayName: string;
  deviceId?: string | null;
  permissions: string[];
  roles: string[];
};

export function getAccessToken() {
  return sessionStorage.getItem(TOKEN_KEY);
}

export function getRefreshToken() {
  return localStorage.getItem(REFRESH_KEY);
}

export function getStoredUser(): StoredUser | null {
  const raw = sessionStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as StoredUser;
  } catch {
    return null;
  }
}

export function storeSession(accessToken: string, refreshToken: string, user: StoredUser) {
  sessionStorage.setItem(TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_KEY, refreshToken);
  sessionStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function clearSession() {
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(USER_KEY);
  localStorage.removeItem(REFRESH_KEY);
}

function baseUrl() {
  return import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ?? "";
}

export async function apiFetch<T>(
  path: string,
  init: RequestInit = {},
): Promise<{ data?: T; error?: ApiError; status: number }> {
  const headers = new Headers(init.headers);
  if (!headers.has("Content-Type") && init.body) {
    headers.set("Content-Type", "application/json");
  }
  const token = getAccessToken();
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${baseUrl()}${path}`, { ...init, headers });
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
