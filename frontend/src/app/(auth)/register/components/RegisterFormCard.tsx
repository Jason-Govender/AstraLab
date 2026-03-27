import {
  Alert,
  Button,
  Card,
  Checkbox,
  Col,
  Form,
  Input,
  Row,
  Typography,
} from "antd";
import { AUTH_PASSWORD_REGEX } from "@/constants/auth";
import type { RegisterFormValues } from "@/types/auth";
import { useStyles } from "../style";

const { Paragraph, Text, Title } = Typography;

interface RegisterFormCardProps {
  errorMessage?: string;
  isSubmitting: boolean;
  onSubmit: (values: RegisterFormValues) => Promise<void>;
  onSignIn: () => void;
}

function isEmailLike(value: string) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

export function RegisterFormCard({
  errorMessage,
  isSubmitting,
  onSubmit,
  onSignIn,
}: RegisterFormCardProps) {
  const { styles } = useStyles();

  return (
    <Card bordered className={styles.formCard}>
      <div className={styles.formHeader}>
        <Title level={2} className={styles.formTitle}>
          Create account
        </Title>
        <Paragraph className={styles.formSubtitle}>
          Start your workspace in a few steps
        </Paragraph>
      </div>

      <Form<RegisterFormValues>
        layout="vertical"
        requiredMark={false}
        onFinish={onSubmit}
        className={styles.form}
      >
        {errorMessage ? (
          <Alert
            type="error"
            message={errorMessage}
            showIcon
            className={styles.alert}
          />
        ) : null}

        <Row gutter={16}>
          <Col xs={24} sm={12}>
            <Form.Item<RegisterFormValues>
              label="First Name"
              name="name"
              className={styles.fieldRow}
              rules={[{ required: true, message: "Please enter your first name" }]}
            >
              <Input
                size="large"
                placeholder="Jason"
                className={styles.input}
              />
            </Form.Item>
          </Col>

          <Col xs={24} sm={12}>
            <Form.Item<RegisterFormValues>
              label="Surname"
              name="surname"
              className={styles.fieldRow}
              rules={[{ required: true, message: "Please enter your surname" }]}
            >
              <Input
                size="large"
                placeholder="Smith"
                className={styles.input}
              />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item<RegisterFormValues>
          label="Username"
          name="userName"
          dependencies={["emailAddress"]}
          className={styles.fieldRow}
          rules={[
            { required: true, message: "Please choose a username" },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value) {
                  return Promise.resolve();
                }

                const emailAddress = getFieldValue("emailAddress");

                if (!isEmailLike(value) || value === emailAddress) {
                  return Promise.resolve();
                }

                return Promise.reject(
                  new Error(
                    "Username cannot be an email unless it matches your email address",
                  ),
                );
              },
            }),
          ]}
        >
          <Input
            size="large"
            placeholder="jason.smith"
            className={styles.input}
          />
        </Form.Item>

        <Form.Item<RegisterFormValues>
          label="Email"
          name="emailAddress"
          className={styles.fieldRow}
          rules={[
            { required: true, message: "Please enter your email address" },
            { type: "email", message: "Please enter a valid email address" },
          ]}
        >
          <Input
            size="large"
            placeholder="you@example.com"
            className={styles.input}
          />
        </Form.Item>

        <Form.Item<RegisterFormValues>
          label="Tenant"
          name="tenancyName"
          className={styles.fieldRow}
          rules={[{ required: true, message: "Please enter your tenant name" }]}
          extra={
            <span className={styles.helperText}>
              This must match an active AstraLab tenant
            </span>
          }
        >
          <Input size="large" placeholder="acme" className={styles.input} />
        </Form.Item>

        <Form.Item<RegisterFormValues>
          label="Password"
          name="password"
          className={styles.fieldRow}
          rules={[
            { required: true, message: "Please enter a password" },
            {
              pattern: AUTH_PASSWORD_REGEX,
              message:
                "Use at least 8 characters with upper, lower, number, and no spaces",
            },
          ]}
        >
          <Input.Password
            size="large"
            placeholder="••••••••••••"
            className={styles.input}
          />
        </Form.Item>

        <Form.Item<RegisterFormValues>
          label="Confirm Password"
          name="confirmPassword"
          className={styles.fieldRow}
          dependencies={["password"]}
          rules={[
            { required: true, message: "Please confirm your password" },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue("password") === value) {
                  return Promise.resolve();
                }

                return Promise.reject(new Error("Passwords do not match"));
              },
            }),
          ]}
        >
          <Input.Password
            size="large"
            placeholder="••••••••••••"
            className={styles.input}
          />
        </Form.Item>

        <Form.Item<RegisterFormValues>
          name="acceptTerms"
          valuePropName="checked"
          className={styles.checkboxRow}
          rules={[
            {
              validator(_, value) {
                if (value) {
                  return Promise.resolve();
                }

                return Promise.reject(
                  new Error("Please agree to the terms and privacy policy"),
                );
              },
            },
          ]}
        >
          <Checkbox className={styles.checkbox}>
            <span className={styles.checkboxLabel}>
              I agree to the terms and privacy policy
            </span>
          </Checkbox>
        </Form.Item>

        <Button
          htmlType="submit"
          type="primary"
          size="large"
          block
          loading={isSubmitting}
          className={styles.submitButton}
        >
          Create Account
        </Button>

        <div className={styles.signInRow}>
          <Text className={styles.signInText}>Already have an account?</Text>
          <Button
            type="link"
            className={styles.signInButton}
            onClick={onSignIn}
          >
            Sign in
          </Button>
        </div>
      </Form>
    </Card>
  );
}
