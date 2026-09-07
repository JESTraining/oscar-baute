import { createContext, useContext, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { setUnauthorizedHandler } from "../api/api";
import { decodeJwtPayload } from "./jwt";
import { getToken, setToken as persistToken } from "./token";

const ROLE_CLAIM_URI = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

function roleFromToken(token: string | null): string | null {
  if (!token) {
    return null;
  }
  const payload = decodeJwtPayload(token);
  const role = payload?.[ROLE_CLAIM_URI] ?? payload?.role;
  return typeof role === "string" ? role : null;
}

interface AuthState {
  isAuthenticated: boolean;
  isAdmin: boolean;
  login: (token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthState | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => getToken());

  useEffect(() => {
    setUnauthorizedHandler(() => {
      persistToken(null);
      setToken(null);
    });
    return () => setUnauthorizedHandler(null);
  }, []);

  const value = useMemo<AuthState>(() => {
    const role = roleFromToken(token);
    return {
      isAuthenticated: token !== null,
      isAdmin: role === "Admin",
      login: (newToken: string) => {
        persistToken(newToken);
        setToken(newToken);
      },
      logout: () => {
        persistToken(null);
        setToken(null);
      },
    };
  }, [token]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return ctx;
}
