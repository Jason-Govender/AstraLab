"use client";

import { useEffect, useRef } from "react";
import { useRouter } from "next/navigation";
import { Col, Row, Typography } from "antd";
import { LOGIN_ROUTE } from "@/constants/auth";
import { useAuthActions, useAuthState } from "@/providers/authProvider";
import type { RegisterFormValues } from "@/types/auth";
import { RegisterFormCard } from "./components/RegisterFormCard";
import { RegisterHero } from "./components/RegisterHero";
import { useStyles } from "./style";

const { Text } = Typography;

const benefits = [
  "Data profiling and health scoring",
  "Guided transformations and versioning",
  "AI analyst chat and insight memory",
  "Machine learning workflows and report export",
];

export default function RegisterRoute() {
  const router = useRouter();
  const hasClearedFeedback = useRef(false);
  const { clearFeedback, register } = useAuthActions();
  const { errorMessage, isRegistering } = useAuthState();
  const { styles } = useStyles();

  useEffect(() => {
    if (hasClearedFeedback.current) {
      return;
    }

    hasClearedFeedback.current = true;
    clearFeedback();
  }, [clearFeedback]);

  const handleSubmit = async (values: RegisterFormValues) => {
    try {
      await register(values);
    } catch {}
  };

  return (
    <main className={styles.page}>
      <section className={styles.shell}>
        <Row gutter={[56, 40]} align="middle">
          <Col xs={24} lg={12}>
            <RegisterHero benefits={benefits} />
          </Col>

          <Col xs={24} lg={12}>
            <div className={styles.formColumn}>
              <RegisterFormCard
                errorMessage={errorMessage}
                isSubmitting={isRegistering}
                onSubmit={handleSubmit}
                onSignIn={() => router.push(LOGIN_ROUTE)}
              />

              <Text className={styles.footnote}>
                Account creation secured by AstraLab
              </Text>
            </div>
          </Col>
        </Row>
      </section>
    </main>
  );
}
