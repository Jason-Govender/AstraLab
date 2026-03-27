import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import {
  AUTH_SESSION_COOKIE_NAME,
  DASHBOARD_ROUTE,
  LOGIN_ROUTE,
  REGISTER_ROUTE,
} from "./constants/auth";
import { parseAuthSession } from "./utils/authCookies";

function clearInvalidAuthCookie(response: NextResponse) {
  response.cookies.delete(AUTH_SESSION_COOKIE_NAME);

  return response;
}

export function proxy(request: NextRequest) {
  const authCookieValue =
    request.cookies.get(AUTH_SESSION_COOKIE_NAME)?.value ?? null;
  const authSession = parseAuthSession(authCookieValue);
  const hasValidSession = Boolean(authSession);
  const hasSessionCookie = Boolean(authCookieValue);
  const { pathname } = request.nextUrl;

  if (
    (pathname === LOGIN_ROUTE || pathname === REGISTER_ROUTE) &&
    hasValidSession
  ) {
    return NextResponse.redirect(new URL(DASHBOARD_ROUTE, request.url));
  }

  if (pathname.startsWith(DASHBOARD_ROUTE) && !hasValidSession) {
    const response = NextResponse.redirect(new URL(LOGIN_ROUTE, request.url));

    return hasSessionCookie ? clearInvalidAuthCookie(response) : response;
  }

  if (
    (pathname === LOGIN_ROUTE || pathname === REGISTER_ROUTE) &&
    hasSessionCookie &&
    !hasValidSession
  ) {
    return clearInvalidAuthCookie(NextResponse.next());
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/login", "/register", "/dashboard/:path*"],
};
