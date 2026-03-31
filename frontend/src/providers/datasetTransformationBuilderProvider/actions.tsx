import type {
  DatasetTransformationBuilderStep,
  TransformDatasetVersionResult,
} from "@/types/datasets";
import type { IDatasetTransformationBuilderStateContext } from "./context";

type DatasetTransformationBuilderStatePatch =
  Partial<IDatasetTransformationBuilderStateContext>;

export enum DatasetTransformationBuilderActionEnums {
  setSourceVersionId = "DATASET_TRANSFORMATION_BUILDER_SET_SOURCE_VERSION_ID",
  addStep = "DATASET_TRANSFORMATION_BUILDER_ADD_STEP",
  updateStep = "DATASET_TRANSFORMATION_BUILDER_UPDATE_STEP",
  removeStep = "DATASET_TRANSFORMATION_BUILDER_REMOVE_STEP",
  moveStep = "DATASET_TRANSFORMATION_BUILDER_MOVE_STEP",
  transformPending = "DATASET_TRANSFORMATION_BUILDER_PENDING",
  transformSuccess = "DATASET_TRANSFORMATION_BUILDER_SUCCESS",
  transformError = "DATASET_TRANSFORMATION_BUILDER_ERROR",
  reset = "DATASET_TRANSFORMATION_BUILDER_RESET",
}

interface SetSourceVersionIdAction {
  type: DatasetTransformationBuilderActionEnums.setSourceVersionId;
  payload: DatasetTransformationBuilderStatePatch;
}

interface AddStepAction {
  type: DatasetTransformationBuilderActionEnums.addStep;
  payload: {
    step: DatasetTransformationBuilderStep;
  };
}

interface UpdateStepAction {
  type: DatasetTransformationBuilderActionEnums.updateStep;
  payload: {
    step: DatasetTransformationBuilderStep;
  };
}

interface RemoveStepAction {
  type: DatasetTransformationBuilderActionEnums.removeStep;
  payload: {
    stepId: string;
  };
}

interface MoveStepAction {
  type: DatasetTransformationBuilderActionEnums.moveStep;
  payload: {
    stepId: string;
    direction: "up" | "down";
  };
}

interface TransformPendingAction {
  type: DatasetTransformationBuilderActionEnums.transformPending;
  payload: DatasetTransformationBuilderStatePatch;
}

interface TransformSuccessAction {
  type: DatasetTransformationBuilderActionEnums.transformSuccess;
  payload: DatasetTransformationBuilderStatePatch;
}

interface TransformErrorAction {
  type: DatasetTransformationBuilderActionEnums.transformError;
  payload: DatasetTransformationBuilderStatePatch;
}

interface ResetTransformationAction {
  type: DatasetTransformationBuilderActionEnums.reset;
  payload: DatasetTransformationBuilderStatePatch;
}

export type DatasetTransformationBuilderAction =
  | SetSourceVersionIdAction
  | AddStepAction
  | UpdateStepAction
  | RemoveStepAction
  | MoveStepAction
  | TransformPendingAction
  | TransformSuccessAction
  | TransformErrorAction
  | ResetTransformationAction;

export const setSourceVersionId = (
  sourceVersionId?: number,
): SetSourceVersionIdAction => ({
  type: DatasetTransformationBuilderActionEnums.setSourceVersionId,
  payload: {
    selectedSourceVersionId: sourceVersionId,
  },
});

export const addStep = (
  step: DatasetTransformationBuilderStep,
): AddStepAction => ({
  type: DatasetTransformationBuilderActionEnums.addStep,
  payload: {
    step,
  },
});

export const updateStep = (
  step: DatasetTransformationBuilderStep,
): UpdateStepAction => ({
  type: DatasetTransformationBuilderActionEnums.updateStep,
  payload: {
    step,
  },
});

export const removeStep = (stepId: string): RemoveStepAction => ({
  type: DatasetTransformationBuilderActionEnums.removeStep,
  payload: {
    stepId,
  },
});

export const moveStep = (
  stepId: string,
  direction: "up" | "down",
): MoveStepAction => ({
  type: DatasetTransformationBuilderActionEnums.moveStep,
  payload: {
    stepId,
    direction,
  },
});

export const transformPending = (): TransformPendingAction => ({
  type: DatasetTransformationBuilderActionEnums.transformPending,
  payload: {
    isSubmittingTransformation: true,
    isPending: true,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
    transformResult: undefined,
  },
});

export const transformSuccess = (
  transformResult: TransformDatasetVersionResult,
): TransformSuccessAction => ({
  type: DatasetTransformationBuilderActionEnums.transformSuccess,
  payload: {
    isSubmittingTransformation: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    transformResult,
  },
});

export const transformError = (
  errorMessage: string,
): TransformErrorAction => ({
  type: DatasetTransformationBuilderActionEnums.transformError,
  payload: {
    isSubmittingTransformation: false,
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
  },
});

export const resetTransformation = (
  sourceVersionId?: number,
): ResetTransformationAction => ({
  type: DatasetTransformationBuilderActionEnums.reset,
  payload: {
    isSubmittingTransformation: false,
    isPending: false,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
    selectedSourceVersionId: sourceVersionId,
    steps: [],
    transformResult: undefined,
  },
});
