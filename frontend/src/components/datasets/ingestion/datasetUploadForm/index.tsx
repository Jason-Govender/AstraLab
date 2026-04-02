"use client";

import { useState } from "react";
import { Button, Card, Form, Input, Tag, Typography, Upload } from "antd";
import { FileTextOutlined } from "@ant-design/icons";
import type { UploadFile } from "antd";
import type { UploadProps } from "antd";
import { WorkspaceFeedbackAlert } from "@/components/workspaceShell/WorkspaceFeedbackAlert";
import type { UploadRawDatasetFormValues } from "@/types/datasets";
import { formatBytes } from "@/utils/datasets";
import { useStyles } from "./style";

const { Dragger } = Upload;
const { Paragraph, Title } = Typography;
const { TextArea } = Input;

interface DatasetUploadFormProps {
  isSubmitting?: boolean;
  errorMessage?: string;
  onInteraction?: () => void;
  onSubmit: (values: UploadRawDatasetFormValues) => Promise<void>;
}

interface DatasetUploadFields {
  name: string;
  description?: string;
}

export const DatasetUploadForm = ({
  isSubmitting = false,
  errorMessage,
  onInteraction,
  onSubmit,
}: DatasetUploadFormProps) => {
  const { styles } = useStyles();
  const [form] = Form.useForm<DatasetUploadFields>();
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [fileError, setFileError] = useState<string>();
  const datasetName = Form.useWatch("name", form);
  const selectedFile = fileList[0];
  const currentFile = selectedFile?.originFileObj;
  const canSubmit = Boolean(datasetName?.trim()) && Boolean(currentFile) && !isSubmitting;

  const clearFormFeedback = () => {
    onInteraction?.();
  };

  const handleUploadChange: UploadProps["onChange"] = ({ fileList: nextFileList }) => {
    clearFormFeedback();
    setFileError(undefined);
    setFileList(nextFileList.slice(-1));
  };

  const handleRemoveFile = () => {
    clearFormFeedback();
    setFileError(undefined);
    setFileList([]);
  };

  const handleSubmit = async (values: DatasetUploadFields) => {
    if (!currentFile) {
      setFileError("A dataset file is required.");
      return;
    }

    await onSubmit({
      name: values.name.trim(),
      description: values.description?.trim(),
      file: currentFile,
    });
  };

  return (
    <Card className={styles.card}>
      <div className={styles.introBlock}>
        <Title level={4} className={styles.introTitle}>
          Start a new dataset intake
        </Title>
        <Paragraph className={styles.helperText}>
          Upload one raw CSV or JSON file, confirm the dataset metadata, and let AstraLab create the first profiled version automatically.
        </Paragraph>
      </div>

      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        onValuesChange={clearFormFeedback}
        className={styles.form}
      >
        {isSubmitting ? (
          <WorkspaceFeedbackAlert
            type="info"
            title="Currently running"
            description="AstraLab is uploading the file, creating the first dataset version, and profiling it in the background."
          />
        ) : null}

        {errorMessage ? (
          <WorkspaceFeedbackAlert
            type="error"
            title="Action failed"
            description={errorMessage}
          />
        ) : null}

        <Form.Item
          name="name"
          label="Dataset name"
          rules={[{ required: true, message: "Dataset name is required." }]}
        >
          <Input
            size="large"
            placeholder="Customer churn master dataset"
            maxLength={300}
          />
        </Form.Item>

        <Form.Item name="description" label="Description">
          <TextArea
            rows={4}
            placeholder="Briefly describe what this dataset contains and where it came from."
            maxLength={1000}
          />
        </Form.Item>

        <Form.Item
          label="Raw dataset file"
          required
          extra="Select a CSV or JSON file. The actual upload happens only when you press Upload dataset."
        >
          <Dragger
            className={styles.dragger}
            accept=".csv,.json"
            fileList={fileList}
            maxCount={1}
            showUploadList={false}
            beforeUpload={() => false}
            onChange={handleUploadChange}
          >
            <Paragraph>Click or drag a dataset file to this area to upload</Paragraph>
            <Paragraph className={styles.helperText}>
              Supported formats: CSV and JSON. Files stay local until you submit this form.
            </Paragraph>
          </Dragger>

          {selectedFile ? (
            <div className={styles.selectedFileCard}>
              <div className={styles.selectedFileBody}>
                <div className={styles.selectedFileName}>
                  <FileTextOutlined /> {selectedFile.name}
                </div>
                <div className={styles.selectedFileMeta}>
                  <Tag color="blue">
                    {(selectedFile.name.split(".").pop() || "file").toUpperCase()}
                  </Tag>
                  {typeof selectedFile.size === "number" ? (
                    <Tag color="default">{formatBytes(selectedFile.size)}</Tag>
                  ) : null}
                  <Tag color="cyan">Ready to upload</Tag>
                </div>
              </div>

              <Button onClick={handleRemoveFile}>Remove file</Button>
            </div>
          ) : null}

          {fileError ? (
            <Paragraph className={styles.fileError}>{fileError}</Paragraph>
          ) : null}
        </Form.Item>

        <div className={styles.submitRow}>
          <Button
            type="primary"
            htmlType="submit"
            size="large"
            loading={isSubmitting}
            disabled={!canSubmit}
          >
            Upload dataset
          </Button>
          <Paragraph className={styles.submitHint}>
            After a successful upload, AstraLab will show the immediate profiling snapshot here and then redirect you to dataset details automatically.
          </Paragraph>
        </div>
      </Form>
    </Card>
  );
};
