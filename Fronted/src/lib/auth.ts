export const AUTH_TOKEN_KEY = "sipad_auth_token";
export const AUTH_USER_KEY = "sipad_auth_user";

export interface AuthUser {
  email: string;
  fullName: string;
}

export function setAuthSession(token: string, user: AuthUser) {
  if (typeof window === "undefined") return;

  localStorage.setItem(AUTH_TOKEN_KEY, token);
  localStorage.setItem(AUTH_USER_KEY, JSON.stringify(user));

  const maxAge = 60 * 60 * 2;
  document.cookie = `sipad_token=${encodeURIComponent(
    token
  )}; path=/; max-age=${maxAge}; samesite=lax`;
}

export function clearAuthSession() {
  if (typeof window === "undefined") return;

  localStorage.removeItem(AUTH_TOKEN_KEY);
  localStorage.removeItem(AUTH_USER_KEY);
  document.cookie = "sipad_token=; path=/; max-age=0; samesite=lax";
}

export function getAuthToken() {
  if (typeof window === "undefined") return null;

  const localStorageToken = localStorage.getItem(AUTH_TOKEN_KEY);
  if (localStorageToken) return localStorageToken;

  const cookieToken = document.cookie
    .split("; ")
    .find((cookiePart) => cookiePart.startsWith("sipad_token="))
    ?.split("=")[1];

  return cookieToken ? decodeURIComponent(cookieToken) : null;
}

export function getAuthUser(): AuthUser | null {
  if (typeof window === "undefined") return null;

  const raw = localStorage.getItem(AUTH_USER_KEY);
  if (!raw) return null;

  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    return null;
  }
}
