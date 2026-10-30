import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { login as apiLogin, logout as apiLogout } from "../api/auth";
import {
  clearSession,
  getRefreshToken,
  getStoredUser,
  type StoredUser,
} from "../api/client";

type AuthContextValue = {
  user: StoredUser | null;
  isAuthenticated: boolean;
  login: (tenantCode: string, email: string, password: string) => Promise<string | null>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<StoredUser | null>(() => getStoredUser());

  const login = useCallback(async (tenantCode: string, email: string, password: string) => {
    const result = await apiLogin(tenantCode, email, password);
    if (result.error || !result.data) {
      return result.error?.message ?? "login failed";
    }
    setUser(getStoredUser());
    return null;
  }, []);

  const logout = useCallback(async () => {
    await apiLogout(getRefreshToken());
    clearSession();
    setUser(null);
  }, []);

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: Boolean(user),
      login,
      logout,
    }),
    [user, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}
