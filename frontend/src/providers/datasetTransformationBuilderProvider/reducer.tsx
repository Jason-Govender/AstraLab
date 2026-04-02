import { INITIAL_STATE } from "./context";
import type { IDatasetTransformationBuilderStateContext } from "./context";
import {
  DatasetTransformationBuilderActionEnums,
  type DatasetTransformationBuilderAction,
} from "./actions";

export const DatasetTransformationBuilderReducer = (
  state: IDatasetTransformationBuilderStateContext = INITIAL_STATE,
  action: DatasetTransformationBuilderAction,
) => {
  switch (action.type) {
    case DatasetTransformationBuilderActionEnums.addStep:
      return {
        ...state,
        isPending: false,
        isSuccess: false,
        isError: false,
        errorMessage: undefined,
        steps: [...state.steps, action.payload.step],
      };
    case DatasetTransformationBuilderActionEnums.updateStep:
      return {
        ...state,
        isPending: false,
        isSuccess: false,
        isError: false,
        errorMessage: undefined,
        steps: state.steps.map((step) =>
          step.id === action.payload.step.id ? action.payload.step : step,
        ),
      };
    case DatasetTransformationBuilderActionEnums.removeStep:
      return {
        ...state,
        isPending: false,
        isSuccess: false,
        isError: false,
        errorMessage: undefined,
        steps: state.steps.filter((step) => step.id !== action.payload.stepId),
      };
    case DatasetTransformationBuilderActionEnums.moveStep: {
      const currentIndex = state.steps.findIndex(
        (step) => step.id === action.payload.stepId,
      );

      if (currentIndex < 0) {
        return state;
      }

      const targetIndex =
        action.payload.direction === "up" ? currentIndex - 1 : currentIndex + 1;

      if (targetIndex < 0 || targetIndex >= state.steps.length) {
        return state;
      }

      const nextSteps = [...state.steps];
      const [movedStep] = nextSteps.splice(currentIndex, 1);
      nextSteps.splice(targetIndex, 0, movedStep);

      return {
        ...state,
        isPending: false,
        isSuccess: false,
        isError: false,
        errorMessage: undefined,
        steps: nextSteps,
      };
    }
    default:
      return {
        ...state,
        ...action.payload,
      };
  }
};
