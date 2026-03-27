import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import {
  AUTH_SESSION_COOKIE_NAME,
  DASHBOARD_ROUTE,
  LOGIN_ROUTE,
} from "@/constants/auth";
import { parseAuthSession } from "@/utils/authCookies";

export default async function Home() {
  const cookieStore = await cookies();
  const authSession = parseAuthSession(
    cookieStore.get(AUTH_SESSION_COOKIE_NAME)?.value,
  );

  redirect(authSession ? DASHBOARD_ROUTE : LOGIN_ROUTE);
}
