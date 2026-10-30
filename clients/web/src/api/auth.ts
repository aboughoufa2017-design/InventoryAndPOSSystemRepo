import { apiFetch, storeSession, type StoredUser } from "./client";

export type AuthTokenResponse = {
  accessToken: string;
  refreshToken: string;
  userId: string;
  tenantId: string;
  email: string;
  displayName: string;
  deviceId?: string | null;
  permissions: string[];
  roles: string[];
};

function deviceKey() {
  const key = "warehouse.deviceKey";
  let value = localStorage.getItem(key);
  if (!value) {
    value = crypto.randomUUID();
    localStorage.setItem(key, value);
  }
  return value;
}

export async function login(tenantCode: string, email: string, password: string) {
  const result = await apiFetch<AuthTokenResponse>("/api/v1/auth/login", {
    method: "POST",
    body: JSON.stringify({
      tenantCode,
      email,
      password,
      deviceKey: deviceKey(),
      deviceName: "Web",
      applicationVersion: "0.1.0-web",
    }),
  });

  if (result.data) {
    const user: StoredUser = {
      userId: result.data.userId,
      tenantId: result.data.tenantId,
      email: result.data.email,
      displayName: result.data.displayName,
      deviceId: result.data.deviceId,
      permissions: result.data.permissions,
      roles: result.data.roles,
    };
    storeSession(result.data.accessToken, result.data.refreshToken, user);
  }

  return result;
}

export async function logout(refreshToken: string | null) {
  return apiFetch<void>("/api/v1/auth/logout", {
    method: "POST",
    body: JSON.stringify({ refreshToken }),
  });
}
