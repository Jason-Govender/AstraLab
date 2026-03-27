"use client";

import { useEffect, useRef } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { App, Col, Row, Typography } from "antd";
import { REGISTER_ROUTE } from "@/constants/auth";
import { useAuthActions, useAuthState } from "@/providers/authProvider";
import type { LoginFormValues } from "@/types/auth";
import { LoginFormCard } from "./components/LoginFormCard";
import { LoginHero } from "./components/LoginHero";
import { useStyles } from "./style";

const { Text } = Typography;

interface Metric {
  label: string;
  value: string;
}

const metrics: Metric[] = [
  { label: "Datasets Managed", value: "128" },
  { label: "ML Experiments", value: "342" },
];

export default function LoginRoute() {
  const { message } = App.useApp();
  const router = useRouter();
  const searchParams = useSearchParams();
  const hasClearedFeedback = useRef(false);
  const { clearFeedback, login } = useAuthActions();
  const { errorMessage, isLoggingIn } = useAuthState();
  const { styles } = useStyles();
  const successMessage =
    searchParams.get("registered") === "1"
      ? "Account created successfully. Sign in to continue."
      : undefined;

  useEffect(() => {
    if (hasClearedFeedback.current) {
      return;
    }

    hasClearedFeedback.current = true;
    clearFeedback();
  }, [clearFeedback]);

  const handleSubmit = async (values: LoginFormValues) => {
    try {
      await login(values);
    } catch {}
  };

  const handleAuxClick = (label: string) => {
    void message.info(`${label} is coming soon.`);
  };

  return (
    <main className={styles.page}>
      <section className={styles.shell}>
        <Row gutter={[56, 40]} align="middle">
          <Col xs={24} lg={13}>
            <LoginHero metrics={metrics} />
          </Col>

          <Col xs={24} lg={11}>
            <div className={styles.formColumn}>
              <LoginFormCard
                errorMessage={errorMessage}
                successMessage={successMessage}
                isSubmitting={isLoggingIn}
                onSubmit={handleSubmit}
                onForgotPassword={() => handleAuxClick("Forgot password")}
                onRegister={() => router.push(REGISTER_ROUTE)}
              />

              <Text className={styles.footnote}>
                Secure login powered by the AstraLab platform
              </Text>
            </div>
          </Col>
        </Row>
      </section>
    </main>
  );
}
