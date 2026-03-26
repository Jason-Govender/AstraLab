export const API_PROXY_PREFIX = "/api/backend";
export const DEFAULT_LOCAL_API_BASE_URL = "https://localhost:44311";

export const API_HEADERS = Object.freeze({
  authorization: "Authorization",
  antiForgery: "X-XSRF-TOKEN",
  contentType: "Content-Type",
  tenantId: "Abp.TenantId",
});

const HOP_BY_HOP_REQUEST_HEADERS = new Set([
  "connection",
  "content-length",
  "host",
  "keep-alive",
  "proxy-authenticate",
  "proxy-authorization",
  "te",
  "trailer",
  "transfer-encoding",
  "upgrade",
]);

const HOP_BY_HOP_RESPONSE_HEADERS = new Set([
  "connection",
  "content-length",
  "keep-alive",
  "proxy-authenticate",
  "proxy-authorization",
  "te",
  "trailer",
  "transfer-encoding",
  "upgrade",
]);

export function normalizeApiBaseUrl(value) {
  const trimmedValue = value?.trim();
  if (!trimmedValue) {
    return null;
  }

  return trimmedValue.replace(/\/+$/, "");
}

export function shouldUseDirectBrowserApi(env = process.env) {
  return env.NODE_ENV === "development" && Boolean(normalizeApiBaseUrl(env.NEXT_PUBLIC_API_BASE_URL));
}

export function getBrowserApiBaseUrl(env = process.env) {
  return shouldUseDirectBrowserApi(env)
    ? normalizeApiBaseUrl(env.NEXT_PUBLIC_API_BASE_URL)
    : API_PROXY_PREFIX;
}

export function getServerApiBaseUrl(env = process.env) {
  return (
    normalizeApiBaseUrl(env.BACKEND_INTERNAL_URL) ??
    normalizeApiBaseUrl(env.NEXT_PUBLIC_API_BASE_URL) ??
    DEFAULT_LOCAL_API_BASE_URL
  );
}

export function buildApiUrl(baseUrl, path) {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  return `${baseUrl}${normalizedPath}`;
}

export function shouldForwardRequestBody(method) {
  const normalizedMethod = method.toUpperCase();
  return !["GET", "HEAD", "OPTIONS"].includes(normalizedMethod);
}

export function buildProxyTargetUrl(pathSegments, search = "", env = process.env) {
  const normalizedPath = pathSegments.length > 0 ? `/${pathSegments.join("/")}` : "";
  return `${getServerApiBaseUrl(env)}${normalizedPath}${search}`;
}

export function copyProxyRequestHeaders(source) {
  const headers = new Headers();

  source.forEach((value, key) => {
    if (HOP_BY_HOP_REQUEST_HEADERS.has(key.toLowerCase())) {
      return;
    }

    headers.set(key, value);
  });

  return headers;
}

export function copyProxyResponseHeaders(source) {
  const headers = new Headers();

  source.forEach((value, key) => {
    if (HOP_BY_HOP_RESPONSE_HEADERS.has(key.toLowerCase())) {
      return;
    }

    headers.append(key, value);
  });

  return headers;
}

export async function proxyToBackend(request, pathSegments, env = process.env) {
  const requestUrl = new URL(request.url);
  const targetUrl = buildProxyTargetUrl(pathSegments, requestUrl.search, env);
  const headers = copyProxyRequestHeaders(request.headers);

  let body;
  if (shouldForwardRequestBody(request.method)) {
    const arrayBuffer = await request.arrayBuffer();
    body = arrayBuffer.byteLength > 0 ? arrayBuffer : undefined;
  }

  try {
    const upstreamResponse = await fetch(targetUrl, {
      method: request.method,
      headers,
      body,
      cache: "no-store",
      redirect: "manual",
    });

    return new Response(upstreamResponse.body, {
      status: upstreamResponse.status,
      statusText: upstreamResponse.statusText,
      headers: copyProxyResponseHeaders(upstreamResponse.headers),
    });
  } catch (error) {
    return Response.json(
      {
        message: "Failed to reach the backend API.",
        detail: error instanceof Error ? error.message : "Unknown proxy error.",
        targetUrl,
      },
      { status: 502 },
    );
  }
}
