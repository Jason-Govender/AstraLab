import { createContext } from "react";
import type {
  DatasetTransformationBuilderStep,
  DatasetTransformationType,
  TransformDatasetVersionResult,
} from "@/types/datasets";

export interface IDatasetTransformationBuilderStateContext {
  isSubmittingTransformation: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage?: string;
  selectedSourceVersionId?: number;
  steps: DatasetTransformationBuilderStep[];
  transformResult?: TransformDatasetVersionResult;
}

export interface IDatasetTransformationBuilderActionContext {
  setSourceVersionId: (sourceVersionId?: number) => void;
  addStep: (transformationType: DatasetTransformationType) => void;
  updateStep: (step: DatasetTransformationBuilderStep) => void;
  removeStep: (stepId: string) => void;
  moveStep: (stepId: string, direction: "up" | "down") => void;
  transformDatasetVersion: () => Promise<TransformDatasetVersionResult | undefined>;
  resetTransformation: () => void;
}

export const INITIAL_STATE: IDatasetTransformationBuilderStateContext = {
  isSubmittingTransformation: false,
  isPending: false,
  isSuccess: false,
  isError: false,
  steps: [],
};

export const DatasetTransformationBuilderStateContext =
  createContext<IDatasetTransformationBuilderStateContext | undefined>(undefined);

export const DatasetTransformationBuilderActionContext =
  createContext<IDatasetTransformationBuilderActionContext | undefined>(undefined);
