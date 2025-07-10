import React, { createContext, useContext, useState } from "react";

const AuthContext = createContext();

/* -------- utilidades JWT -------- */
function base64UrlToBase64(str) {
  return (
    str.replace(/-/g, "+").replace(/_/g, "/") +
    "===".slice((str.length + 3) % 4)
  );
}

export function parseJwt(token) {
  if (!token) return null;
  try {
    const base64 = base64UrlToBase64(token.split(".")[1]);
    return JSON.parse(atob(base64));
  } catch {
    return null;
  }
}

function isTokenValid(token) {
  const p = parseJwt(token);
  return p && p.exp && p.exp > Date.now() / 1000;
}
/* -------------------------------- */

/* token inicial lido de cookie, já validado */
const getInitialToken = () => {
  const cookie = document.cookie
    .split("; ")
    .find((c) => c.startsWith("pdvnet_token="));
  if (!cookie) return null;

  const tk = cookie.split("=")[1];
  if (isTokenValid(tk)) return tk;

  // expirado ⇒ limpa
  document.cookie =
    "pdvnet_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
  return null;
};

export const AuthProvider = ({ children }) => {
  const [token, setToken] = useState(getInitialToken);

  const login = (tk, expirationUtc) => {
    /* expirationUtc já vem do backend */
    document.cookie = `pdvnet_token=${tk}; expires=${expirationUtc}; path=/;`;
    setToken(tk);
  };

  const logout = () => {
    document.cookie =
      "pdvnet_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    setToken(null);
  };

  return (
    <AuthContext.Provider
      value={{ token, isAuth: !!token && isTokenValid(token), login, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
