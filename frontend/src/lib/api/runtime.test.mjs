import assert from "node:assert/strict";

import {
  API_HEADERS,
  API_PROXY_PREFIX,
  buildProxyTargetUrl,
  copyProxyRequestHeaders,
  copyProxyResponseHeaders,
  getBrowserApiBaseUrl,
  getServerApiBaseUrl,
  normalizeApiBaseUrl,
  shouldUseDirectBrowserApi,
} from "./runtime.js";

assert.equal(normalizeApiBaseUrl(" https://api.example.com/// "), "https://api.example.com");
assert.equal(normalizeApiBaseUrl(""), null);
assert.equal(normalizeApiBaseUrl(undefined), null);

assert.equal(
  shouldUseDirectBrowserApi({
    NODE_ENV: "development",
    NEXT_PUBLIC_API_BASE_URL: "https://localhost:44311/",
  }),
  true,
);

assert.equal(
  getBrowserApiBaseUrl({
    NODE_ENV: "development",
    NEXT_PUBLIC_API_BASE_URL: "https://localhost:44311/",
  }),
  "https://localhost:44311",
);

assert.equal(
  getBrowserApiBaseUrl({
    NODE_ENV: "production",
    NEXT_PUBLIC_API_BASE_URL: "https://api.example.com/",
  }),
  API_PROXY_PREFIX,
);

assert.equal(
  getServerApiBaseUrl({
    BACKEND_INTERNAL_URL: "https://internal-api.example.com/",
    NEXT_PUBLIC_API_BASE_URL: "https://public-api.example.com/",
  }),
  "https://internal-api.example.com",
);

assert.equal(
  getServerApiBaseUrl({
    NEXT_PUBLIC_API_BASE_URL: "https://public-api.example.com/",
  }),
  "https://public-api.example.com",
);

const targetUrl = buildProxyTargetUrl(
  ["api", "services", "app", "Session", "GetCurrentLoginInformations"],
  "?include=features",
  { BACKEND_INTERNAL_URL: "https://internal-api.example.com/" },
);

assert.equal(
  targetUrl,
  "https://internal-api.example.com/api/services/app/Session/GetCurrentLoginInformations?include=features",
);

const requestHeaders = new Headers({
  authorization: "Bearer token",
  cookie: "XSRF-TOKEN=value",
  "content-type": "application/json",
  "x-xsrf-token": "anti-forgery-token",
  "abp.tenantid": "1",
  connection: "keep-alive",
  host: "frontend.example.com",
  "content-length": "128",
});

const forwardedRequestHeaders = copyProxyRequestHeaders(requestHeaders);

assert.equal(forwardedRequestHeaders.get(API_HEADERS.authorization), "Bearer token");
assert.equal(forwardedRequestHeaders.get("cookie"), "XSRF-TOKEN=value");
assert.equal(forwardedRequestHeaders.get(API_HEADERS.contentType), "application/json");
assert.equal(forwardedRequestHeaders.get(API_HEADERS.antiForgery), "anti-forgery-token");
assert.equal(forwardedRequestHeaders.get(API_HEADERS.tenantId), "1");
assert.equal(forwardedRequestHeaders.has("connection"), false);
assert.equal(forwardedRequestHeaders.has("host"), false);
assert.equal(forwardedRequestHeaders.has("content-length"), false);

const responseHeaders = new Headers({
  "content-type": "application/json",
  "set-cookie": "XSRF-TOKEN=value; Secure; SameSite=None",
  "x-xsrf-token": "anti-forgery-token",
  "transfer-encoding": "chunked",
});

const forwardedResponseHeaders = copyProxyResponseHeaders(responseHeaders);

assert.equal(forwardedResponseHeaders.get("content-type"), "application/json");
assert.equal(forwardedResponseHeaders.get("set-cookie"), "XSRF-TOKEN=value; Secure; SameSite=None");
assert.equal(forwardedResponseHeaders.get(API_HEADERS.antiForgery), "anti-forgery-token");
assert.equal(forwardedResponseHeaders.has("transfer-encoding"), false);
