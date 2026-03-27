import { AUTH_SESSION_COOKIE_NAME } from "@/constants/auth";
import type { AuthSession } from "@/types/auth";

const COOKIE_PATH = "Path=/";
const COOKIE_SAME_SITE = "SameSite=Lax";

function getCookieSource(cookieSource?: string) {
  if (cookieSource !== undefined) {
    return cookieSource;
  }

  if (typeof document === "undefined") {
    return "";
  }

  return document.cookie;
}

function getCookieValue(name: string, cookieSource?: string) {
  const source = getCookieSource(cookieSource);

  if (!source) {
    return null;
  }

  const cookies = source.split(";").map((entry) => entry.trim());
  const match = cookies.find((entry) => entry.startsWith(`${name}=`));

  return match ? match.slice(name.length + 1) : null;
}

function getCookieSecuritySegment() {
  if (typeof window === "undefined") {
    return "";
  }

  return window.location.protocol === "https:" ? "; Secure" : "";
}

export function isAuthSessionValid(session: AuthSession | null | undefined) {
  if (!session?.accessToken || !session.expiresAt) {
    return false;
  }

  return new Date(session.expiresAt).getTime() > Date.now();
}

export function parseAuthSession(cookieValue?: string | null) {
  if (!cookieValue) {
    return null;
  }

  try {
    const parsed = JSON.parse(
      decodeURIComponent(cookieValue),
    ) as AuthSession;

    return isAuthSessionValid(parsed) ? parsed : null;
  } catch {
    return null;
  }
}

export function getAuthSession(cookieSource?: string) {
  return parseAuthSession(getCookieValue(AUTH_SESSION_COOKIE_NAME, cookieSource));
}

export function getAuthToken(cookieSource?: string) {
  return getAuthSession(cookieSource)?.accessToken ?? null;
}

export function setAuthSession(session: AuthSession) {
  if (typeof document === "undefined") {
    return;
  }

  const encodedValue = encodeURIComponent(JSON.stringify(session));
  const maxAge = Math.max(
    0,
    Math.floor((new Date(session.expiresAt).getTime() - Date.now()) / 1000),
  );

  document.cookie = `${AUTH_SESSION_COOKIE_NAME}=${encodedValue}; ${COOKIE_PATH}; ${COOKIE_SAME_SITE}; Max-Age=${maxAge}${getCookieSecuritySegment()}`;
}

export function clearAuthSession() {
  if (typeof document === "undefined") {
    return;
  }

  document.cookie = `${AUTH_SESSION_COOKIE_NAME}=; ${COOKIE_PATH}; ${COOKIE_SAME_SITE}; Max-Age=0${getCookieSecuritySegment()}`;
}
