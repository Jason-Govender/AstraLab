"use client";

import { Button, Card, Col, Row, Spin, Typography } from "antd";
import { useAuthActions, useAuthState } from "@/providers/authProvider";
import { useStyles } from "./style";

const { Paragraph, Text, Title } = Typography;

export default function DashboardPage() {
  const { logout } = useAuthActions();
  const { isAuthenticated, isInitialized, profile, session } = useAuthState();
  const { styles } = useStyles();

  if (!isInitialized) {
    return (
      <main className={styles.loadingState}>
        <Spin size="large" />
      </main>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <main className={styles.page}>
      <section className={styles.shell}>
        <div className={styles.header}>
          <div>
            <Text className={styles.kicker}>AstraLab Workspace</Text>
            <Title level={1} className={styles.title}>
              Welcome back, {profile?.user?.name || profile?.user?.userName || "User"}
            </Title>
            <Paragraph className={styles.description}>
              Your session is active and authenticated requests now flow through the
              shared auth provider and axios instance.
            </Paragraph>
          </div>

          <Button
            size="large"
            onClick={() => logout()}
            className={styles.logoutButton}
          >
            Sign Out
          </Button>
        </div>

        <Row gutter={[20, 20]}>
          <Col xs={24} md={8}>
            <Card bordered className={styles.infoCard}>
              <Text className={styles.cardLabel}>Authenticated User</Text>
              <Title level={3} className={styles.cardValue}>
                {profile?.user?.emailAddress || "Unavailable"}
              </Title>
            </Card>
          </Col>

          <Col xs={24} md={8}>
            <Card bordered className={styles.infoCard}>
              <Text className={styles.cardLabel}>Tenant</Text>
              <Title level={3} className={styles.cardValue}>
                {profile?.tenant?.tenancyName || session?.tenancyName || "Default"}
              </Title>
            </Card>
          </Col>

          <Col xs={24} md={8}>
            <Card bordered className={styles.infoCard}>
              <Text className={styles.cardLabel}>User ID</Text>
              <Title level={3} className={styles.cardValue}>
                {session?.userId || "Unknown"}
              </Title>
            </Card>
          </Col>
        </Row>

        <Card bordered className={styles.summaryCard}>
          <Title level={4} className={styles.summaryTitle}>
            Auth wiring completed
          </Title>
          <Paragraph className={styles.summaryText}>
            JWT session storage, request header injection, auth bootstrap, and 401
            handling are now centralized. This route is the first protected landing
            page for the frontend.
          </Paragraph>
        </Card>
      </section>
    </main>
  );
}
