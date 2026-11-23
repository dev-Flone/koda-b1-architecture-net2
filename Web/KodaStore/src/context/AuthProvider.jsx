import { useCallback, useEffect, useMemo, useState } from "react";
import {
  ApiError,
  getCurrentUser,
  loginUser,
  logoutUser,
  refreshSession,
} from "../services/authApi";
import { AuthContext } from "./authContext";

const EMPTY_SESSION = { user: null, accessToken: null, expiresAtUtc: null };

export function AuthProvider({ children }) {
  const [auth, setAuth] = useState({ status: "checking", ...EMPTY_SESSION });

  // APPLY SESSION
  const applySession = useCallback(({ user, accessToken, expiresAtUtc }) => {
    setAuth({ status: "authenticated", user, accessToken, expiresAtUtc });
  }, []);

  // CLEAR SESSION
  const clearSession = useCallback(() => {
    setAuth({ status: "anonymous", ...EMPTY_SESSION });
  }, []);

  useEffect(() => {
    let active = true;

    refreshSession()
      .then((session) => active && applySession(session))
      .catch(() => active && clearSession());
    return () => {
      active = false;
    };
  }, [applySession, clearSession]);

  // LOGIN
  const login = useCallback(
    async (credentials) => {
      const session = await loginUser(credentials);
      applySession(session);
      return session;
    },
    [applySession],
  );

  // REFRESH ACCESS_TOKEN
  const refreshAccessToken = useCallback(async () => {
    try {
      const session = await refreshSession();
      applySession(session);
      return session;
    } catch (err) {
      if (err instanceof ApiError && [401, 403].includes(err.status))
        clearSession();
      throw err;
    }
  }, [applySession, clearSession]);

  // LOAD CURRENT_USER
  const loadCurrentUser = useCallback(async () => {
    if (!auth.accessToken) throw new ApiError(401, "AccessToken not available");

    try {
      return await getCurrentUser(auth.accessToken);
    } catch (error) {
      if (!(error instanceof ApiError) || error.status !== 401) throw error;
      const { accessToken } = await refreshAccessToken();
      return getCurrentUser(accessToken);
    }
  }, [auth.accessToken, refreshAccessToken]);

  // LOGOUT
  const logout = useCallback(async () => {
    await logoutUser();
    clearSession();
  }, [clearSession]);

  const contextValue = useMemo(
    () => ({
      ...auth,
      isAuthenticated:
        auth.status === "authenticated" && Boolean(auth.accessToken),
      login,
      refreshAccessToken,
      loadCurrentUser,
      logout,
    }),
    [auth, login, refreshAccessToken, loadCurrentUser, logout],
  );

  return (
    <AuthContext.Provider value={contextValue}>{children}</AuthContext.Provider>
  );
}
