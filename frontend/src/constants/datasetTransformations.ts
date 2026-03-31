import {
  AggregationFunction,
  DatasetTransformationDataType,
  DatasetTransformationType,
  FilterMatchMode,
  FilterOperator,
  MissingValueStrategy,
} from "@/types/datasets";

export const DEFAULT_DATASET_TRANSFORMATION_STEP_TYPE =
  DatasetTransformationType.RemoveDuplicates;

export const DATASET_TRANSFORMATION_TYPE_OPTIONS = [
  { label: "Remove Duplicates", value: DatasetTransformationType.RemoveDuplicates },
  { label: "Handle Missing Values", value: DatasetTransformationType.HandleMissingValues },
  { label: "Convert Data Type", value: DatasetTransformationType.ConvertDataType },
  { label: "Filter Rows", value: DatasetTransformationType.FilterRows },
  { label: "Aggregate", value: DatasetTransformationType.Aggregate },
] as const;

export const MISSING_VALUE_STRATEGY_OPTIONS = [
  { label: "Drop Rows", value: MissingValueStrategy.DropRows },
  { label: "Fill Constant", value: MissingValueStrategy.FillConstant },
  { label: "Fill Mean", value: MissingValueStrategy.FillMean },
  { label: "Fill Mode", value: MissingValueStrategy.FillMode },
] as const;

export const FILTER_MATCH_MODE_OPTIONS = [
  { label: "All Conditions", value: FilterMatchMode.All },
  { label: "Any Condition", value: FilterMatchMode.Any },
] as const;

export const FILTER_OPERATOR_OPTIONS = [
  { label: "Equals", value: FilterOperator.Equals },
  { label: "Not Equals", value: FilterOperator.NotEquals },
  { label: "Greater Than", value: FilterOperator.GreaterThan },
  { label: "Greater Than Or Equal", value: FilterOperator.GreaterThanOrEqual },
  { label: "Less Than", value: FilterOperator.LessThan },
  { label: "Less Than Or Equal", value: FilterOperator.LessThanOrEqual },
  { label: "Contains", value: FilterOperator.Contains },
  { label: "Starts With", value: FilterOperator.StartsWith },
  { label: "Ends With", value: FilterOperator.EndsWith },
  { label: "Is Null", value: FilterOperator.IsNull },
  { label: "Is Not Null", value: FilterOperator.IsNotNull },
] as const;

export const AGGREGATION_FUNCTION_OPTIONS = [
  { label: "Count", value: AggregationFunction.Count },
  { label: "Sum", value: AggregationFunction.Sum },
  { label: "Average", value: AggregationFunction.Average },
  { label: "Minimum", value: AggregationFunction.Min },
  { label: "Maximum", value: AggregationFunction.Max },
] as const;

export const DATASET_TRANSFORMATION_DATA_TYPE_OPTIONS = [
  { label: "Integer", value: DatasetTransformationDataType.Integer },
  { label: "Decimal", value: DatasetTransformationDataType.Decimal },
  { label: "Boolean", value: DatasetTransformationDataType.Boolean },
  { label: "Datetime", value: DatasetTransformationDataType.DateTime },
  { label: "String", value: DatasetTransformationDataType.String },
] as const;

export const DEFAULT_DATASET_TRANSFORMATION_HISTORY_PAGE_SIZE = 20;
