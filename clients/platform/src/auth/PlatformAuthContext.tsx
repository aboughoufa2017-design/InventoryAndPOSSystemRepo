import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import {
  clearPlatformSession,
  getPlatformOperator,
  platformLogin,
  type PlatformOperator,
} from "../api/platform";

type PlatformAuthContextValue = {
  operator: PlatformOperator | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<string | null>;
  logout: () => void;
};

const PlatformAuthContext = createContext<PlatformAuthContextValue | null>(null);

export function PlatformAuthProvider({ children }: { children: ReactNode }) {
  const [operator, setOperator] = useState<PlatformOperator | null>(() => getPlatformOperator());

  const login = useCallback(async (email: string, password: string) => {
    const result = await platformLogin(email, password);
    if (result.error || !result.data) {
      return result.error?.message ?? "login failed";
    }
    setOperator(getPlatformOperator());
    return null;
  }, []);

  const logout = useCallback(() => {
    clearPlatformSession();
    setOperator(null);
  }, []);

  const value = useMemo(
    () => ({
      operator,
      isAuthenticated: Boolean(operator),
      login,
      logout,
    }),
    [operator, login, logout],
  );

  return <PlatformAuthContext.Provider value={value}>{children}</PlatformAuthContext.Provider>;
}

export function usePlatformAuth() {
  const ctx = useContext(PlatformAuthContext);
  if (!ctx) {
    throw new Error("usePlatformAuth must be used within PlatformAuthProvider");
  }
  return ctx;
}
