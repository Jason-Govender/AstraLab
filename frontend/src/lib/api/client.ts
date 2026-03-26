import {
  API_HEADERS,
  buildApiUrl,
  getBrowserApiBaseUrl,
  shouldUseDirectBrowserApi,
} from "./runtime.js";
import {
  clearAuthSession,
  loadAuthSession,
  saveAuthSession,
  type StoredAuthSession,
} from "./storage";

type JsonRecord = Record<string, unknown>;

type AjaxResponse<T> = {
  result: T;
  success?: boolean;
  error?: {
    message: string;
    details?: string;
  } | null;
};

export interface ApiRequestOptions {
  path: string;
  method?: string;
  json?: JsonRecord;
  body?: BodyInit;
  headers?: HeadersInit;
  accessToken?: string | null;
  tenantId?: string | number | null;
  antiForgeryToken?: string | null;
}

export interface AuthenticateRequest {
  userNameOrEmailAddress: string;
  password: string;
  tenantId?: string | number | null;
}

export interface AuthenticateResult {
  accessToken: string;
  encryptedAccessToken: string;
  expireInSeconds: number;
  userId: number;
}

export interface TenantAvailabilityResult {
  state: number;
  tenantId?: number | null;
  serverRootAddress?: string | null;
}

export interface CurrentLoginInformations {
  application?: {
    version?: string;
    releaseDate?: string;
    features?: Record<string, boolean>;
  };
  user?: {
    name?: string;
    surname?: string;
    userName?: string;
    emailAddress?: string;
  };
  tenant?: {
    name?: string;
    tenancyName?: string;
  };
}

let antiForgeryTokenCache: string | null = null;

export class ApiError extends Error {
  readonly status: number;
  readonly detail: string;

  constructor(status: number, message: string, detail = "") {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.detail = detail;
  }
}

function isUnsafeMethod(method: string) {
  return !["GET", "HEAD", "OPTIONS"].includes(method.toUpperCase());
}

function resolveBrowserUrl(path: string) {
  return buildApiUrl(getBrowserApiBaseUrl(process.env), path);
}

function unwrapAjaxResponse<T>(payload: AjaxResponse<T> | T): T {
  if (payload && typeof payload === "object" && "result" in (payload as AjaxResponse<T>)) {
    const ajaxPayload = payload as AjaxResponse<T>;
    if (ajaxPayload.error) {
      throw new ApiError(400, ajaxPayload.error.message, ajaxPayload.error.details ?? "");
    }

    return ajaxPayload.result;
  }

  return payload as T;
}

async function parseJsonResponse<T>(response: Response): Promise<T> {
  const text = await response.text();
  if (!text) {
    return undefined as T;
  }

  return JSON.parse(text) as T;
}

async function toApiError(response: Response) {
  const fallbackMessage = `API request failed with status ${response.status}.`;
  const text = await response.text();

  if (!text) {
    return new ApiError(response.status, fallbackMessage);
  }

  try {
    const payload = JSON.parse(text) as { message?: string; detail?: string; error?: { message?: string; details?: string } };
    return new ApiError(
      response.status,
      payload.error?.message ?? payload.message ?? fallbackMessage,
      payload.error?.details ?? payload.detail ?? text,
    );
  } catch {
    return new ApiError(response.status, fallbackMessage, text);
  }
}

export function getBrowserTransportLabel() {
  return shouldUseDirectBrowserApi(process.env)
    ? "Direct browser -> API"
    : "Same-origin frontend proxy";
}

export function getResolvedBrowserApiBaseUrl() {
  return getBrowserApiBaseUrl(process.env);
}

export async function fetchAntiForgeryToken(forceRefresh = false) {
  if (!forceRefresh && antiForgeryTokenCache) {
    return antiForgeryTokenCache;
  }

  const response = await fetch(resolveBrowserUrl("/AntiForgery/GetToken"), {
    method: "GET",
    cache: "no-store",
    credentials: "include",
  });

  if (!response.ok) {
    throw await toApiError(response);
  }

  const antiForgeryToken = response.headers.get(API_HEADERS.antiForgery);
  if (!antiForgeryToken) {
    throw new ApiError(500, "The backend did not expose an anti-forgery token.");
  }

  antiForgeryTokenCache = antiForgeryToken;
  return antiForgeryToken;
}

export async function apiRequest<T>({
  path,
  method = "GET",
  json,
  body,
  headers,
  accessToken,
  tenantId,
  antiForgeryToken,
}: ApiRequestOptions): Promise<T> {
  const resolvedMethod = method.toUpperCase();
  const requestHeaders = new Headers(headers);

  if (json !== undefined) {
    requestHeaders.set(API_HEADERS.contentType, "application/json");
  }

  if (accessToken) {
    requestHeaders.set(API_HEADERS.authorization, `Bearer ${accessToken}`);
  }

  if (tenantId !== undefined && tenantId !== null && `${tenantId}`.trim() !== "") {
    requestHeaders.set(API_HEADERS.tenantId, String(tenantId));
  }

  if (isUnsafeMethod(resolvedMethod)) {
    const token = antiForgeryToken ?? (await fetchAntiForgeryToken());
    requestHeaders.set(API_HEADERS.antiForgery, token);
  }

  const response = await fetch(resolveBrowserUrl(path), {
    method: resolvedMethod,
    headers: requestHeaders,
    body: json !== undefined ? JSON.stringify(json) : body,
    cache: "no-store",
    credentials: "include",
  });

  if (!response.ok) {
    throw await toApiError(response);
  }

  const contentType = response.headers.get("content-type") ?? "";
  if (!contentType.includes("application/json")) {
    return undefined as T;
  }

  const payload = await parseJsonResponse<AjaxResponse<T> | T>(response);
  return unwrapAjaxResponse(payload);
}

export async function isTenantAvailable(tenancyName: string) {
  return apiRequest<TenantAvailabilityResult>({
    path: "/api/services/app/Account/IsTenantAvailable",
    method: "POST",
    json: { tenancyName },
  });
}

export async function authenticate({
  userNameOrEmailAddress,
  password,
  tenantId,
}: AuthenticateRequest) {
  const payload = await apiRequest<AuthenticateResult>({
    path: "/api/TokenAuth/Authenticate",
    method: "POST",
    json: {
      userNameOrEmailAddress,
      password,
      rememberClient: true,
    },
    tenantId,
  });

  const session: StoredAuthSession = {
    accessToken: payload.accessToken,
    userId: payload.userId,
    expiresAt: new Date(Date.now() + payload.expireInSeconds * 1000).toISOString(),
    tenantId: tenantId ? String(tenantId) : null,
  };

  saveAuthSession(session);
  return session;
}

export async function getCurrentLoginInformations(session: StoredAuthSession) {
  return apiRequest<CurrentLoginInformations>({
    path: "/api/services/app/Session/GetCurrentLoginInformations",
    accessToken: session.accessToken,
    tenantId: session.tenantId ?? null,
  });
}

export function getStoredAuthSession() {
  return loadAuthSession();
}

export function signOut() {
  clearAuthSession();
}
