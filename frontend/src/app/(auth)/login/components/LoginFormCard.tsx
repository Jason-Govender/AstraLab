import { Alert, Button, Card, Divider, Form, Input, Typography } from "antd";
import type { LoginFormValues } from "@/types/auth";
import { useStyles } from "../style";

const { Paragraph, Text, Title } = Typography;

interface LoginFormCardProps {
  errorMessage?: string;
  successMessage?: string;
  isSubmitting: boolean;
  onSubmit: (values: LoginFormValues) => Promise<void>;
  onForgotPassword: () => void;
  onRegister: () => void;
}

export function LoginFormCard({
  errorMessage,
  successMessage,
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
        {successMessage ? (
          <Alert
            type="success"
            message={successMessage}
            showIcon
            className={styles.successAlert}
          />
        ) : null}

        {errorMessage ? (
          <Alert
            type="error"
            message={errorMessage}
            showIcon
            className={styles.alert}
          />
        ) : null}

        <Form.Item<LoginFormValues>
          label="Email or Username"
          name="userNameOrEmailAddress"
          className={styles.fieldRow}
          rules={[
            {
              required: true,
              message: "Please enter your email address or username",
            },
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

          <Form.Item<LoginFormValues>
            label="Tenant"
            name="tenancyName"
            className={styles.fieldRow}
            extra={
              <span className={styles.helperText}>
                Leave blank to use the Default tenant
              </span>
            }
          >
            <Input
              size="large"
              placeholder="Default"
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
