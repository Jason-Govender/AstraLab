"use client";

import { Button, Card, Select, Tag, Typography } from "antd";
import { DATASET_TRANSFORMATION_TYPE_OPTIONS } from "@/constants/datasetTransformations";
import type {
  AggregateTransformationConfig,
  ConvertDataTypeTransformationConfig,
  DatasetColumn,
  DatasetTransformationBuilderStep,
  DatasetTransformationType,
  FilterRowsTransformationConfig,
  HandleMissingValuesTransformationConfig,
  RemoveDuplicatesTransformationConfig,
} from "@/types/datasets";
import { DatasetTransformationType as DatasetTransformationTypeEnum } from "@/types/datasets";
import {
  createDefaultTransformationConfiguration,
  getDatasetTransformationTypeLabel,
} from "@/utils/datasetTransformations";
import { AggregateTransformationStepForm } from "../aggregateTransformationStepForm";
import { ConvertDataTypeTransformationStepForm } from "../convertDataTypeTransformationStepForm";
import { FilterRowsTransformationStepForm } from "../filterRowsTransformationStepForm";
import { HandleMissingValuesTransformationStepForm } from "../handleMissingValuesTransformationStepForm";
import { RemoveDuplicatesTransformationStepForm } from "../removeDuplicatesTransformationStepForm";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetTransformationStepCardProps {
  step: DatasetTransformationBuilderStep;
  stepNumber: number;
  totalSteps: number;
  columns: DatasetColumn[];
  onChange: (step: DatasetTransformationBuilderStep) => void;
  onMove: (stepId: string, direction: "up" | "down") => void;
  onRemove: (stepId: string) => void;
}

const renderTransformationForm = ({
  columns,
  step,
  onChange,
}: {
  columns: DatasetColumn[];
  step: DatasetTransformationBuilderStep;
  onChange: (step: DatasetTransformationBuilderStep) => void;
}) => {
  switch (step.transformationType) {
    case DatasetTransformationTypeEnum.RemoveDuplicates:
      return (
        <RemoveDuplicatesTransformationStepForm
          columns={columns}
          configuration={step.configuration as RemoveDuplicatesTransformationConfig}
          onChange={(configuration) =>
            onChange({
              ...step,
              configuration,
            })
          }
        />
      );
    case DatasetTransformationTypeEnum.HandleMissingValues:
      return (
        <HandleMissingValuesTransformationStepForm
          columns={columns}
          configuration={
            step.configuration as HandleMissingValuesTransformationConfig
          }
          onChange={(configuration) =>
            onChange({
              ...step,
              configuration,
            })
          }
        />
      );
    case DatasetTransformationTypeEnum.ConvertDataType:
      return (
        <ConvertDataTypeTransformationStepForm
          columns={columns}
          configuration={step.configuration as ConvertDataTypeTransformationConfig}
          onChange={(configuration) =>
            onChange({
              ...step,
              configuration,
            })
          }
        />
      );
    case DatasetTransformationTypeEnum.FilterRows:
      return (
        <FilterRowsTransformationStepForm
          columns={columns}
          configuration={step.configuration as FilterRowsTransformationConfig}
          onChange={(configuration) =>
            onChange({
              ...step,
              configuration,
            })
          }
        />
      );
    case DatasetTransformationTypeEnum.Aggregate:
      return (
        <AggregateTransformationStepForm
          columns={columns}
          configuration={step.configuration as AggregateTransformationConfig}
          onChange={(configuration) =>
            onChange({
              ...step,
              configuration,
            })
          }
        />
      );
    default:
      return null;
  }
};

export const DatasetTransformationStepCard = ({
  step,
  stepNumber,
  totalSteps,
  columns,
  onChange,
  onMove,
  onRemove,
}: DatasetTransformationStepCardProps) => {
  const { styles } = useStyles();

  const handleTypeChange = (transformationType: DatasetTransformationType) => {
    onChange({
      ...step,
      transformationType,
      configuration: createDefaultTransformationConfiguration(transformationType),
    });
  };

  return (
    <Card className={styles.card}>
      <div className={styles.header}>
        <div className={styles.headingBlock}>
          <Tag className={styles.stepTag}>{`Step ${stepNumber}`}</Tag>
          <Title level={4} className={styles.title}>
            {getDatasetTransformationTypeLabel(step.transformationType)}
          </Title>
          <Paragraph className={styles.helperText}>
            Configure this transformation step exactly as the backend expects it.
          </Paragraph>
        </div>

        <div className={styles.controls}>
          <Select
            size="large"
            value={step.transformationType}
            className={styles.typeSelect}
            options={DATASET_TRANSFORMATION_TYPE_OPTIONS.map((option) => ({
              label: option.label,
              value: option.value,
            }))}
            onChange={handleTypeChange}
          />
          <Button
            onClick={() => onMove(step.id, "up")}
            disabled={stepNumber === 1}
          >
            Move Up
          </Button>
          <Button
            onClick={() => onMove(step.id, "down")}
            disabled={stepNumber === totalSteps}
          >
            Move Down
          </Button>
          <Button danger onClick={() => onRemove(step.id)}>
            Remove
          </Button>
        </div>
      </div>

      {renderTransformationForm({
        columns,
        step,
        onChange,
      })}
    </Card>
  );
};
