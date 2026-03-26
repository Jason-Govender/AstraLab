"use client";

import { useEffect, useState } from "react";
import styles from "./page.module.css";
import {
  fetchAntiForgeryToken,
  authenticate,
  getBrowserTransportLabel,
  getCurrentLoginInformations,
  getResolvedBrowserApiBaseUrl,
  getStoredAuthSession,
  isTenantAvailable,
  signOut,
  type CurrentLoginInformations,
} from "@/lib/api/client";
import type { StoredAuthSession } from "@/lib/api/storage";

function formatJson(value: unknown) {
  return JSON.stringify(value, null, 2);
}

export default function Home() {
  const [pendingAction, setPendingAction] = useState<string | null>(null);
  const [status, setStatus] = useState("Ready to verify frontend-to-API communication.");
  const [tenantName, setTenantName] = useState("");
  const [tenantId, setTenantId] = useState("");
  const [userNameOrEmailAddress, setUserNameOrEmailAddress] = useState("admin");
  const [password, setPassword] = useState("123qwe");
  const [session, setSession] = useState<StoredAuthSession | null>(null);
  const [sessionInfo, setSessionInfo] = useState<CurrentLoginInformations | null>(null);
  const [antiForgeryToken, setAntiForgeryToken] = useState<string | null>(null);
  const [payloadPreview, setPayloadPreview] = useState("{}");

  useEffect(() => {
    setSession(getStoredAuthSession());
  }, []);

  async function runAction(label: string, action: () => Promise<unknown>) {
    setPendingAction(label);
    setStatus(`${label} in progress...`);

    try {
      const result = await action();
      if (typeof result !== "undefined") {
        setPayloadPreview(formatJson(result));
      }

      setStatus(`${label} completed successfully.`);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unexpected error.";
      setStatus(`${label} failed: ${message}`);
    } finally {
      setPendingAction(null);
    }
  }

  return (
    <main className={styles.page}>
      <section className={styles.hero}>
        <p className={styles.eyebrow}>AstraLab HTTP contract</p>
        <h1>Frontend and ASP.NET Core are now wired for local and deployed traffic.</h1>
        <p className={styles.summary}>
          Local development can call the API directly, while deployed browser traffic stays on the
          frontend origin and proxies through <code>/api/backend</code>.
        </p>
      </section>

      <section className={styles.grid}>
        <article className={styles.card}>
          <h2>Transport</h2>
          <dl className={styles.definitionList}>
            <div>
              <dt>Browser mode</dt>
              <dd>{getBrowserTransportLabel()}</dd>
            </div>
            <div>
              <dt>Resolved base URL</dt>
              <dd>
                <code>{getResolvedBrowserApiBaseUrl()}</code>
              </dd>
            </div>
            <div>
              <dt>Required request headers</dt>
              <dd>
                <code>Authorization</code>, <code>Content-Type</code>,{" "}
                <code>X-XSRF-TOKEN</code>, <code>Abp.TenantId</code>
              </dd>
            </div>
          </dl>
          <button
            className={styles.primaryButton}
            disabled={Boolean(pendingAction)}
            onClick={() =>
              void runAction("Anti-forgery bootstrap", async () => {
                const token = await fetchAntiForgeryToken(true);
                setAntiForgeryToken(token);
                return { antiForgeryToken: token };
              })
            }
          >
            {pendingAction === "Anti-forgery bootstrap" ? "Bootstrapping..." : "Fetch anti-forgery token"}
          </button>
          <p className={styles.tokenHint}>
            Cached token: <code>{antiForgeryToken ? `${antiForgeryToken.slice(0, 18)}...` : "not fetched yet"}</code>
          </p>
        </article>

        <article className={styles.card}>
          <h2>Tenant lookup</h2>
          <label className={styles.fieldLabel} htmlFor="tenantName">
            Tenancy name
          </label>
          <input
            id="tenantName"
            className={styles.input}
            placeholder="Default"
            value={tenantName}
            onChange={(event) => setTenantName(event.target.value)}
          />
          <button
            className={styles.secondaryButton}
            disabled={Boolean(pendingAction) || tenantName.trim().length === 0}
            onClick={() =>
              void runAction("Tenant lookup", async () => {
                const availability = await isTenantAvailable(tenantName.trim());
                setTenantId(availability.tenantId ? String(availability.tenantId) : "");
                return availability;
              })
            }
          >
            {pendingAction === "Tenant lookup" ? "Resolving..." : "Resolve tenant"}
          </button>
          <p className={styles.tokenHint}>
            Resolved tenant id: <code>{tenantId || "host context"}</code>
          </p>
        </article>

        <article className={styles.card}>
          <h2>Authentication</h2>
          <label className={styles.fieldLabel} htmlFor="username">
            Username or email
          </label>
          <input
            id="username"
            className={styles.input}
            value={userNameOrEmailAddress}
            onChange={(event) => setUserNameOrEmailAddress(event.target.value)}
          />
          <label className={styles.fieldLabel} htmlFor="password">
            Password
          </label>
          <input
            id="password"
            className={styles.input}
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
          <div className={styles.buttonRow}>
            <button
              className={styles.primaryButton}
              disabled={Boolean(pendingAction)}
              onClick={() =>
                void runAction("Login", async () => {
                  const nextSession = await authenticate({
                    userNameOrEmailAddress,
                    password,
                    tenantId: tenantId || null,
                  });

                  setSession(nextSession);
                  return nextSession;
                })
              }
            >
              {pendingAction === "Login" ? "Signing in..." : "Sign in"}
            </button>
            <button
              className={styles.secondaryButton}
              disabled={Boolean(pendingAction) || !session}
              onClick={() =>
                void runAction("Session fetch", async () => {
                  if (!session) {
                    throw new Error("Authenticate first.");
                  }

                  const currentInfo = await getCurrentLoginInformations(session);
                  setSessionInfo(currentInfo);
                  return currentInfo;
                })
              }
            >
              {pendingAction === "Session fetch" ? "Loading..." : "Load current session"}
            </button>
          </div>
          <button
            className={styles.ghostButton}
            disabled={Boolean(pendingAction) || !session}
            onClick={() => {
              signOut();
              setSession(null);
              setSessionInfo(null);
              setStatus("Stored bearer token cleared from the frontend.");
              setPayloadPreview("{}");
            }}
          >
            Clear stored token
          </button>
        </article>

        <article className={styles.card}>
          <h2>Live state</h2>
          <dl className={styles.definitionList}>
            <div>
              <dt>Session</dt>
              <dd>{session ? "Authenticated" : "Anonymous"}</dd>
            </div>
            <div>
              <dt>User id</dt>
              <dd>{session?.userId ?? "n/a"}</dd>
            </div>
            <div>
              <dt>Expires</dt>
              <dd>{session?.expiresAt ?? "n/a"}</dd>
            </div>
            <div>
              <dt>Tenant</dt>
              <dd>{session?.tenantId ?? "host"}</dd>
            </div>
            <div>
              <dt>Resolved user</dt>
              <dd>{sessionInfo?.user?.userName ?? "not loaded yet"}</dd>
            </div>
          </dl>
        </article>
      </section>

      <section className={styles.console}>
        <p className={styles.status}>{status}</p>
        <pre className={styles.output}>{payloadPreview}</pre>
      </section>
    </main>
  );
}
