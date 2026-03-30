"use client";

import { useRouter } from "next/navigation";
import { Button, Card, Col, Row, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import { useStyles } from "./style";

const { Paragraph, Text, Title } = Typography;

interface DashboardMetric {
  label: string;
  value: string;
}

interface RecentDatasetRow {
  key: string;
  dataset: string;
  updated: string;
  status: string;
  statusColor: string;
  health: string;
  healthTone: "good" | "warning" | "error";
  lastAnalysis: string;
}

const DASHBOARD_METRICS: DashboardMetric[] = [
  {
    label: "Total Datasets",
    value: "18",
  },
  {
    label: "Processed Versions",
    value: "42",
  },
  {
    label: "ML Experiments",
    value: "11",
  },
  {
    label: "Reports Generated",
    value: "9",
  },
];

const RECENT_DATASETS: RecentDatasetRow[] = [
  {
    key: "customer-churn",
    dataset: "Customer Churn Dataset",
    updated: "2 hours ago",
    status: "Profiled",
    statusColor: "blue",
    health: "86 / 100",
    healthTone: "good",
    lastAnalysis: "Regression Ready",
  },
  {
    key: "retail-sales",
    dataset: "Retail Sales Q4",
    updated: "Yesterday",
    status: "Transformed",
    statusColor: "purple",
    health: "72 / 100",
    healthTone: "warning",
    lastAnalysis: "Clustering Run",
  },
  {
    key: "fraud-signals",
    dataset: "Fraud Signals v2",
    updated: "3 days ago",
    status: "Issues",
    statusColor: "red",
    health: "54 / 100",
    healthTone: "error",
    lastAnalysis: "Anomalies Found",
  },
];

const DashboardPage = () => {
  const { styles, cx } = useStyles();
  const router = useRouter();

  const columns: ColumnsType<RecentDatasetRow> = [
    {
      title: "Dataset",
      dataIndex: "dataset",
      key: "dataset",
    },
    {
      title: "Updated",
      dataIndex: "updated",
      key: "updated",
    },
    {
      title: "Status",
      dataIndex: "status",
      key: "status",
      render: (_, record) => <Tag color={record.statusColor}>{record.status}</Tag>,
    },
    {
      title: "Health",
      dataIndex: "health",
      key: "health",
      render: (_, record) => (
        <span
          className={cx(
            record.healthTone === "good" && styles.healthGood,
            record.healthTone === "warning" && styles.healthWarning,
            record.healthTone === "error" && styles.healthError,
          )}
        >
          {record.health}
        </span>
      ),
    },
    {
      title: "Last Analysis",
      dataIndex: "lastAnalysis",
      key: "lastAnalysis",
    },
  ];

  return (
    <>
      <WorkspacePageHeader
        title="Dataset Dashboard"
        description="Upload, profile, analyze, and model your datasets from one workspace."
      />

      <Card className={styles.uploadCard}>
        <div>
          <Title level={3} className={styles.uploadTitle}>
            Upload a New Dataset
          </Title>
          <Paragraph className={styles.uploadDescription}>
            Start a new profiling and analysis workflow by uploading a CSV or JSON
            file.
          </Paragraph>
        </div>

        <Button
          type="primary"
          size="large"
          onClick={() => router.push("/datasets/upload")}
        >
          Upload Dataset
        </Button>
      </Card>

      <Row gutter={[20, 20]}>
        {DASHBOARD_METRICS.map((metric) => (
          <Col key={metric.label} xs={24} sm={12} xl={6}>
            <Card className={styles.metricCard}>
              <Text className={styles.metricLabel}>{metric.label}</Text>
              <Title level={3} className={styles.metricValue}>
                {metric.value}
              </Title>
            </Card>
          </Col>
        ))}
      </Row>

      <Card className={styles.tableCard}>
        <Title level={3} className={styles.tableTitle}>
          Recent Datasets
        </Title>

        <Table<RecentDatasetRow>
          columns={columns}
          dataSource={RECENT_DATASETS}
          pagination={false}
          className={styles.table}
          scroll={{ x: 900 }}
        />
      </Card>
    </>
  );
};

export default DashboardPage;
