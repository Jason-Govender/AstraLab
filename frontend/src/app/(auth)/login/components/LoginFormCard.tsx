import { Button, Card, Divider, Form, Input, Typography } from "antd";
import type { Store } from "antd/es/form/interface";
import { useStyles } from "../style";

const { Paragraph, Text, Title } = Typography;

interface LoginFormValues extends Store {
  email: string;
  password: string;
}

interface LoginFormCardProps {
  isSubmitting: boolean;
  onSubmit: (values: LoginFormValues) => Promise<void>;
  onForgotPassword: () => void;
  onRegister: () => void;
}

export function LoginFormCard({
  isSubmitting,
  onSubmit,
  onForgotPassword,
  onRegister,
}: LoginFormCardProps) {
  const { styles } = useStyles();

  return (
    <Card bordered className={styles.formCard}>
      <div className={styles.formHeader}>
        <Title level={2} className={styles.formTitle}>
          Welcome back
        </Title>
        <Paragraph className={styles.formSubtitle}>
          Sign in to continue to your workspace
        </Paragraph>
      </div>

      <Form<LoginFormValues>
        layout="vertical"
        requiredMark={false}
        onFinish={onSubmit}
        className={styles.form}
      >
        <Form.Item<LoginFormValues>
          label="Email"
          name="email"
          className={styles.fieldRow}
          rules={[
            { required: true, message: "Please enter your email" },
            { type: "email", message: "Enter a valid email address" },
          ]}
        >
          <Input
            size="large"
            placeholder="you@example.com"
            className={styles.input}
          />
        </Form.Item>

        <Form.Item<LoginFormValues>
          label="Password"
          name="password"
          className={styles.fieldRow}
          rules={[{ required: true, message: "Please enter your password" }]}
        >
          <Input.Password
            size="large"
            placeholder="••••••••••••"
            className={styles.input}
          />
        </Form.Item>

        <div className={styles.actionsRow}>
          <Button
            type="link"
            className={styles.linkButton}
            onClick={onForgotPassword}
          >
            Forgot password?
          </Button>
        </div>

        <Button
          htmlType="submit"
          type="primary"
          size="large"
          block
          loading={isSubmitting}
          className={styles.submitButton}
        >
          Sign In
        </Button>

        <Divider plain className={styles.divider}>
          OR
        </Divider>

        <div className={styles.registerRow}>
          <Text className={styles.registerText}>Don&apos;t have an account?</Text>
          <Button
            type="link"
            className={styles.registerButton}
            onClick={onRegister}
          >
            Register
          </Button>
        </div>
      </Form>
    </Card>
  );
}
