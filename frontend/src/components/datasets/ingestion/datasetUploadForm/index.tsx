"use client";

import { useState } from "react";
import { Alert, Button, Card, Form, Input, Typography, Upload } from "antd";
import type { UploadFile } from "antd";
import type { UploadProps } from "antd";
import type { UploadRawDatasetFormValues } from "@/types/datasets";
import { useStyles } from "./style";

const { Dragger } = Upload;
const { Paragraph } = Typography;
const { TextArea } = Input;

interface DatasetUploadFormProps {
  isSubmitting?: boolean;
  errorMessage?: string;
  onSubmit: (values: UploadRawDatasetFormValues) => Promise<void>;
}

interface DatasetUploadFields {
  name: string;
  description?: string;
}

export const DatasetUploadForm = ({
  isSubmitting = false,
  errorMessage,
  onSubmit,
}: DatasetUploadFormProps) => {
  const { styles } = useStyles();
  const [form] = Form.useForm<DatasetUploadFields>();
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [fileError, setFileError] = useState<string>();

  const handleUploadChange: UploadProps["onChange"] = ({ fileList: nextFileList }) => {
    setFileError(undefined);
    setFileList(nextFileList.slice(-1));
  };

  const handleSubmit = async (values: DatasetUploadFields) => {
    const currentFile = fileList[0]?.originFileObj;

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
      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        className={styles.form}
      >
        {errorMessage ? (
          <Alert
            type="error"
            showIcon
            message="Upload failed"
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
          extra="Upload a CSV or JSON file. The backend validates and extracts schema metadata."
        >
          <Dragger
            accept=".csv,.json"
            fileList={fileList}
            maxCount={1}
            beforeUpload={() => false}
            onChange={handleUploadChange}
            onRemove={() => {
              setFileError(undefined);
              setFileList([]);
            }}
          >
            <Paragraph>Click or drag a dataset file to this area to upload</Paragraph>
            <Paragraph className={styles.helperText}>
              Supported formats: CSV and JSON
            </Paragraph>
          </Dragger>
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
          >
            Upload dataset
          </Button>
          <Paragraph className={styles.helperText}>
            You’ll be redirected to the dataset details page after a successful upload.
          </Paragraph>
        </div>
      </Form>
    </Card>
  );
};
