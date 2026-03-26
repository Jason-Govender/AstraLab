export interface StoredAuthSession {
  accessToken: string;
  userId: number;
  expiresAt: string;
  tenantId?: string | null;
}

const AUTH_STORAGE_KEY = "astralab.auth-session";

function isBrowser() {
  return typeof window !== "undefined";
}

export function loadAuthSession(): StoredAuthSession | null {
  if (!isBrowser()) {
    return null;
  }

  const rawValue = window.localStorage.getItem(AUTH_STORAGE_KEY);
  if (!rawValue) {
    return null;
  }

  try {
    return JSON.parse(rawValue) as StoredAuthSession;
  } catch {
    window.localStorage.removeItem(AUTH_STORAGE_KEY);
    return null;
  }
}

export function saveAuthSession(session: StoredAuthSession) {
  if (!isBrowser()) {
    return;
  }

  window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
}

export function clearAuthSession() {
  if (!isBrowser()) {
    return;
  }

  window.localStorage.removeItem(AUTH_STORAGE_KEY);
}
