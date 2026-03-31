import {
  AggregateTransformationConfig,
  AggregateTransformationDefinition,
  AggregationFunction,
  ConvertDataTypeTransformationConfig,
  DatasetColumn,
  DatasetTransformation,
  DatasetTransformationBuilderConfiguration,
  DatasetTransformationBuilderStep,
  DatasetTransformationDataType,
  DatasetTransformationStepRequest,
  DatasetTransformationType,
  FilterMatchMode,
  FilterOperator,
  FilterRowsTransformationCondition,
  FilterRowsTransformationConfig,
  HandleMissingValuesTransformationConfig,
  MissingValueStrategy,
  RemoveDuplicatesTransformationConfig,
} from "@/types/datasets";

const getBuilderStepIdentifier = () =>
  `${Date.now()}-${Math.random().toString(36).slice(2, 10)}`;

export const getDatasetTransformationTypeLabel = (
  transformationType: DatasetTransformationType,
): string => {
  switch (transformationType) {
    case DatasetTransformationType.RemoveDuplicates:
      return "Remove Duplicates";
    case DatasetTransformationType.HandleMissingValues:
      return "Handle Missing Values";
    case DatasetTransformationType.ConvertDataType:
      return "Convert Data Type";
    case DatasetTransformationType.FilterRows:
      return "Filter Rows";
    case DatasetTransformationType.Aggregate:
      return "Aggregate";
    default:
      return "Unknown";
  }
};

export const getDatasetTransformationTypeColor = (
  transformationType: DatasetTransformationType,
) => {
  switch (transformationType) {
    case DatasetTransformationType.RemoveDuplicates:
      return "gold";
    case DatasetTransformationType.HandleMissingValues:
      return "orange";
    case DatasetTransformationType.ConvertDataType:
      return "processing";
    case DatasetTransformationType.FilterRows:
      return "purple";
    case DatasetTransformationType.Aggregate:
      return "success";
    default:
      return "default";
  }
};

export const getMissingValueStrategyLabel = (
  strategy: MissingValueStrategy,
): string => {
  switch (strategy) {
    case MissingValueStrategy.DropRows:
      return "Drop Rows";
    case MissingValueStrategy.FillConstant:
      return "Fill Constant";
    case MissingValueStrategy.FillMean:
      return "Fill Mean";
    case MissingValueStrategy.FillMode:
      return "Fill Mode";
    default:
      return "Unknown";
  }
};

export const getFilterOperatorLabel = (operator: FilterOperator): string => {
  switch (operator) {
    case FilterOperator.Equals:
      return "Equals";
    case FilterOperator.NotEquals:
      return "Not Equals";
    case FilterOperator.GreaterThan:
      return "Greater Than";
    case FilterOperator.GreaterThanOrEqual:
      return "Greater Than Or Equal";
    case FilterOperator.LessThan:
      return "Less Than";
    case FilterOperator.LessThanOrEqual:
      return "Less Than Or Equal";
    case FilterOperator.Contains:
      return "Contains";
    case FilterOperator.StartsWith:
      return "Starts With";
    case FilterOperator.EndsWith:
      return "Ends With";
    case FilterOperator.IsNull:
      return "Is Null";
    case FilterOperator.IsNotNull:
      return "Is Not Null";
    default:
      return "Unknown";
  }
};

export const getAggregationFunctionLabel = (
  aggregationFunction: AggregationFunction,
): string => {
  switch (aggregationFunction) {
    case AggregationFunction.Count:
      return "Count";
    case AggregationFunction.Sum:
      return "Sum";
    case AggregationFunction.Average:
      return "Average";
    case AggregationFunction.Min:
      return "Minimum";
    case AggregationFunction.Max:
      return "Maximum";
    default:
      return "Unknown";
  }
};

export const getDatasetTransformationDataTypeLabel = (
  dataType: DatasetTransformationDataType,
): string => {
  switch (dataType) {
    case DatasetTransformationDataType.Integer:
      return "Integer";
    case DatasetTransformationDataType.Decimal:
      return "Decimal";
    case DatasetTransformationDataType.Boolean:
      return "Boolean";
    case DatasetTransformationDataType.DateTime:
      return "Datetime";
    case DatasetTransformationDataType.String:
      return "String";
    default:
      return "Unknown";
  }
};

export const createDefaultRemoveDuplicatesConfiguration =
  (): RemoveDuplicatesTransformationConfig => ({
    columns: [],
  });

export const createDefaultHandleMissingValuesConfiguration =
  (): HandleMissingValuesTransformationConfig => ({
    columns: [],
    strategy: MissingValueStrategy.DropRows,
  });

export const createDefaultConvertDataTypeConfiguration =
  (): ConvertDataTypeTransformationConfig => ({
    column: undefined,
    targetType: DatasetTransformationDataType.String,
  });

export const createDefaultFilterCondition =
  (): FilterRowsTransformationCondition => ({
    column: undefined,
    operator: FilterOperator.Equals,
    value: "",
  });

export const createDefaultFilterRowsConfiguration =
  (): FilterRowsTransformationConfig => ({
    match: FilterMatchMode.All,
    conditions: [createDefaultFilterCondition()],
  });

export const createDefaultAggregationDefinition =
  (): AggregateTransformationDefinition => ({
    function: AggregationFunction.Count,
    column: undefined,
    outputColumn: "",
  });

export const createDefaultAggregateConfiguration =
  (): AggregateTransformationConfig => ({
    groupByColumns: [],
    aggregations: [createDefaultAggregationDefinition()],
  });

export const createDefaultTransformationConfiguration = (
  transformationType: DatasetTransformationType,
): DatasetTransformationBuilderConfiguration => {
  switch (transformationType) {
    case DatasetTransformationType.RemoveDuplicates:
      return createDefaultRemoveDuplicatesConfiguration();
    case DatasetTransformationType.HandleMissingValues:
      return createDefaultHandleMissingValuesConfiguration();
    case DatasetTransformationType.ConvertDataType:
      return createDefaultConvertDataTypeConfiguration();
    case DatasetTransformationType.FilterRows:
      return createDefaultFilterRowsConfiguration();
    case DatasetTransformationType.Aggregate:
      return createDefaultAggregateConfiguration();
    default:
      return createDefaultRemoveDuplicatesConfiguration();
  }
};

export const createDatasetTransformationBuilderStep = (
  transformationType: DatasetTransformationType,
): DatasetTransformationBuilderStep => ({
  id: getBuilderStepIdentifier(),
  transformationType,
  configuration: createDefaultTransformationConfiguration(transformationType),
});

export const serializeDatasetTransformationStep = (
  step: DatasetTransformationBuilderStep,
): DatasetTransformationStepRequest => {
  switch (step.transformationType) {
    case DatasetTransformationType.RemoveDuplicates:
      return {
        transformationType: step.transformationType,
        configurationJson: JSON.stringify(
          normalizeRemoveDuplicatesConfiguration(
            step.configuration as RemoveDuplicatesTransformationConfig,
          ),
        ),
      };
    case DatasetTransformationType.HandleMissingValues:
      return {
        transformationType: step.transformationType,
        configurationJson: JSON.stringify(
          normalizeHandleMissingValuesConfiguration(
            step.configuration as HandleMissingValuesTransformationConfig,
          ),
        ),
      };
    case DatasetTransformationType.ConvertDataType:
      return {
        transformationType: step.transformationType,
        configurationJson: JSON.stringify(
          normalizeConvertDataTypeConfiguration(
            step.configuration as ConvertDataTypeTransformationConfig,
          ),
        ),
      };
    case DatasetTransformationType.FilterRows:
      return {
        transformationType: step.transformationType,
        configurationJson: JSON.stringify(
          normalizeFilterRowsConfiguration(
            step.configuration as FilterRowsTransformationConfig,
          ),
        ),
      };
    case DatasetTransformationType.Aggregate:
      return {
        transformationType: step.transformationType,
        configurationJson: JSON.stringify(
          normalizeAggregateConfiguration(
            step.configuration as AggregateTransformationConfig,
          ),
        ),
      };
    default:
      return {
        transformationType: step.transformationType,
        configurationJson: JSON.stringify(step.configuration),
      };
  }
};

export const serializeDatasetTransformationSteps = (
  steps: DatasetTransformationBuilderStep[],
): DatasetTransformationStepRequest[] =>
  steps.map(serializeDatasetTransformationStep);

export const getDatasetTransformationSummaryLines = (
  transformation?: DatasetTransformation | null,
): string[] => {
  if (!transformation?.summaryJson) {
    return [];
  }

  const summary = parseJsonRecord(transformation.summaryJson);
  if (!summary) {
    return [];
  }

  switch (transformation.transformationType) {
    case DatasetTransformationType.RemoveDuplicates:
      return [
        `${getNumberValue(summary.removedRowCount)} rows removed`,
        `${getNumberValue(summary.remainingRowCount)} rows remaining`,
      ];
    case DatasetTransformationType.HandleMissingValues:
      return [
        summary.strategy ? `Strategy: ${String(summary.strategy)}` : "",
        summary.filledCellCount !== undefined
          ? `${getNumberValue(summary.filledCellCount)} cells filled`
          : "",
        summary.droppedRowCount !== undefined
          ? `${getNumberValue(summary.droppedRowCount)} rows dropped`
          : "",
        summary.remainingRowCount !== undefined
          ? `${getNumberValue(summary.remainingRowCount)} rows remaining`
          : "",
      ].filter(Boolean);
    case DatasetTransformationType.ConvertDataType:
      return [
        summary.column ? `Column: ${String(summary.column)}` : "",
        summary.targetType ? `Target Type: ${String(summary.targetType)}` : "",
        summary.convertedValueCount !== undefined
          ? `${getNumberValue(summary.convertedValueCount)} values converted`
          : "",
      ].filter(Boolean);
    case DatasetTransformationType.FilterRows:
      return [
        `${getNumberValue(summary.keptRowCount)} rows kept`,
        `${getNumberValue(summary.removedRowCount)} rows removed`,
      ];
    case DatasetTransformationType.Aggregate:
      return [
        `${getNumberValue(summary.groupCount)} groups created`,
        `${getNumberValue(summary.resultingRowCount)} rows produced`,
      ];
    default:
      return [];
  }
};

export const getAllowedFilterOperators = (
  dataType?: string | null,
): FilterOperator[] => {
  const normalizedDataType = normalizeDatasetColumnDataType(dataType);

  if (normalizedDataType === DatasetTransformationDataType.Boolean) {
    return [
      FilterOperator.Equals,
      FilterOperator.NotEquals,
      FilterOperator.IsNull,
      FilterOperator.IsNotNull,
    ];
  }

  if (
    normalizedDataType === DatasetTransformationDataType.Integer ||
    normalizedDataType === DatasetTransformationDataType.Decimal ||
    normalizedDataType === DatasetTransformationDataType.DateTime
  ) {
    return [
      FilterOperator.Equals,
      FilterOperator.NotEquals,
      FilterOperator.GreaterThan,
      FilterOperator.GreaterThanOrEqual,
      FilterOperator.LessThan,
      FilterOperator.LessThanOrEqual,
      FilterOperator.IsNull,
      FilterOperator.IsNotNull,
    ];
  }

  return [
    FilterOperator.Equals,
    FilterOperator.NotEquals,
    FilterOperator.Contains,
    FilterOperator.StartsWith,
    FilterOperator.EndsWith,
    FilterOperator.IsNull,
    FilterOperator.IsNotNull,
  ];
};

export const normalizeDatasetColumnDataType = (
  dataType?: string | null,
): DatasetTransformationDataType | undefined => {
  switch ((dataType || "").trim().toLowerCase()) {
    case DatasetTransformationDataType.Integer:
      return DatasetTransformationDataType.Integer;
    case DatasetTransformationDataType.Decimal:
      return DatasetTransformationDataType.Decimal;
    case DatasetTransformationDataType.Boolean:
      return DatasetTransformationDataType.Boolean;
    case DatasetTransformationDataType.DateTime:
      return DatasetTransformationDataType.DateTime;
    case DatasetTransformationDataType.String:
      return DatasetTransformationDataType.String;
    default:
      return undefined;
  }
};

export const getNumericDatasetColumns = (columns: DatasetColumn[]): DatasetColumn[] =>
  columns.filter((column) => {
    const normalizedDataType = normalizeDatasetColumnDataType(column.dataType);

    return (
      normalizedDataType === DatasetTransformationDataType.Integer ||
      normalizedDataType === DatasetTransformationDataType.Decimal
    );
  });

export const canAggregationUseColumn = (
  aggregationFunction: AggregationFunction,
): boolean => aggregationFunction !== AggregationFunction.Count;

const normalizeRemoveDuplicatesConfiguration = (
  configuration: RemoveDuplicatesTransformationConfig,
): RemoveDuplicatesTransformationConfig => ({
  columns: configuration.columns || [],
});

const normalizeHandleMissingValuesConfiguration = (
  configuration: HandleMissingValuesTransformationConfig,
): HandleMissingValuesTransformationConfig => ({
  columns: configuration.columns || [],
  strategy: configuration.strategy,
  constantValue:
    configuration.strategy === MissingValueStrategy.FillConstant
      ? configuration.constantValue || ""
      : undefined,
});

const normalizeConvertDataTypeConfiguration = (
  configuration: ConvertDataTypeTransformationConfig,
): ConvertDataTypeTransformationConfig => ({
  column: configuration.column,
  targetType: configuration.targetType,
});

const normalizeFilterRowsConfiguration = (
  configuration: FilterRowsTransformationConfig,
): FilterRowsTransformationConfig => ({
  match: configuration.match,
  conditions: (configuration.conditions || []).map((condition) => ({
    column: condition.column,
    operator: condition.operator,
    value:
      condition.operator === FilterOperator.IsNull ||
      condition.operator === FilterOperator.IsNotNull
        ? undefined
        : condition.value || "",
  })),
});

const normalizeAggregateConfiguration = (
  configuration: AggregateTransformationConfig,
): AggregateTransformationConfig => ({
  groupByColumns: configuration.groupByColumns || [],
  aggregations: (configuration.aggregations || []).map((aggregation) => ({
    function: aggregation.function,
    column: canAggregationUseColumn(aggregation.function)
      ? aggregation.column
      : undefined,
    outputColumn: aggregation.outputColumn || "",
  })),
});

const parseJsonRecord = (
  input: string,
): Record<string, string | number | boolean | null | undefined> | null => {
  try {
    return JSON.parse(input) as Record<
      string,
      string | number | boolean | null | undefined
    >;
  } catch {
    return null;
  }
};

const getNumberValue = (value: unknown): string =>
  typeof value === "number" ? value.toString() : "0";
