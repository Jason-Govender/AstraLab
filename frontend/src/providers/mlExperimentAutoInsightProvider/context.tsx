import { createContext } from "react";
import type { AIResponse } from "@/types/datasets";

export interface IMlExperimentAutoInsightStateContext {
  currentExperimentId?: number;
  latestInsight?: AIResponse | null;
  isLoadingInsight: boolean;
  isError: boolean;
  errorMessage?: string;
}

export interface IMlExperimentAutoInsightActionContext {
  getLatestAutomaticInsight: (experimentId?: number) => Promise<void>;
  clearLatestAutomaticInsight: () => void;
}

export const INITIAL_STATE: IMlExperimentAutoInsightStateContext = {
  latestInsight: null,
  isLoadingInsight: false,
  isError: false,
};

export const MlExperimentAutoInsightStateContext =
  createContext<IMlExperimentAutoInsightStateContext | undefined>(undefined);

export const MlExperimentAutoInsightActionContext =
  createContext<IMlExperimentAutoInsightActionContext | undefined>(undefined);
