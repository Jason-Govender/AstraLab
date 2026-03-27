"use client";

import { useState } from "react";
import { App, Col, Row, Typography } from "antd";
import type { Store } from "antd/es/form/interface";
import { LoginFormCard } from "./components/LoginFormCard";
import { LoginHero } from "./components/LoginHero";
import { useStyles } from "./style";

const { Text } = Typography;

interface LoginFormValues extends Store {
  email: string;
  password: string;
}

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
  const { styles } = useStyles();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (values: LoginFormValues) => {
    setIsSubmitting(true);

    try {
      await new Promise((resolve) => {
        setTimeout(resolve, 650);
      });

      void message.success(`Welcome back, ${values.email}`);
    } finally {
      setIsSubmitting(false);
    }
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
                isSubmitting={isSubmitting}
                onSubmit={handleSubmit}
                onForgotPassword={() => handleAuxClick("Forgot password")}
                onRegister={() => handleAuxClick("Register")}
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
