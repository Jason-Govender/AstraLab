"use client";

import { useState } from "react";
import { Alert, Button, Card, Empty, Select, Typography } from "antd";
import {
  DEFAULT_DATASET_TRANSFORMATION_STEP_TYPE,
  DATASET_TRANSFORMATION_TYPE_OPTIONS,
} from "@/constants/datasetTransformations";
import type {
  DatasetColumn,
  DatasetTransformationBuilderStep,
  DatasetTransformationType,
} from "@/types/datasets";
import { DatasetTransformationStepCard } from "../datasetTransformationStepCard";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetTransformationPipelineBuilderProps {
  columns: DatasetColumn[];
  steps: DatasetTransformationBuilderStep[];
  isSubmitting?: boolean;
  errorMessage?: string;
  onAddStep: (transformationType: DatasetTransformationType) => void;
  onUpdateStep: (step: DatasetTransformationBuilderStep) => void;
  onRemoveStep: (stepId: string) => void;
  onMoveStep: (stepId: string, direction: "up" | "down") => void;
  onSubmit: () => Promise<void>;
}

export const DatasetTransformationPipelineBuilder = ({
  columns,
  steps,
  isSubmitting = false,
  errorMessage,
  onAddStep,
  onUpdateStep,
  onRemoveStep,
  onMoveStep,
  onSubmit,
}: DatasetTransformationPipelineBuilderProps) => {
  const { styles } = useStyles();
  const [nextStepType, setNextStepType] = useState<DatasetTransformationType>(
    DEFAULT_DATASET_TRANSFORMATION_STEP_TYPE,
  );

  return (
    <Card className={styles.card}>
      <div className={styles.header}>
        <div>
          <Title level={3} className={styles.title}>
            Transformation Pipeline
          </Title>
          <Paragraph className={styles.helperText}>
            Build an ordered pipeline and let the backend execute each step, create processed versions, and profile the final result.
          </Paragraph>
        </div>

        <div className={styles.headerActions}>
          <Select
            size="large"
            value={nextStepType}
            className={styles.typeSelect}
            options={DATASET_TRANSFORMATION_TYPE_OPTIONS.map((option) => ({
              label: option.label,
              value: option.value,
            }))}
            onChange={setNextStepType}
          />
          <Button onClick={() => onAddStep(nextStepType)}>Add Step</Button>
          <Button
            type="primary"
            size="large"
            loading={isSubmitting}
            onClick={() => void onSubmit()}
          >
            Run Transformation
          </Button>
        </div>
      </div>

      {errorMessage ? (
        <Alert
          type="error"
          showIcon
          message="Transformation failed"
          description={errorMessage}
          className={styles.alert}
        />
      ) : null}

      {steps.length === 0 ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Add one or more transformation steps to build the pipeline."
        />
      ) : (
        <div className={styles.steps}>
          {steps.map((step, index) => (
            <DatasetTransformationStepCard
              key={step.id}
              step={step}
              stepNumber={index + 1}
              totalSteps={steps.length}
              columns={columns}
              onChange={onUpdateStep}
              onMove={onMoveStep}
              onRemove={onRemoveStep}
            />
          ))}
        </div>
      )}
    </Card>
  );
};
