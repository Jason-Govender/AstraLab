import { INITIAL_STATE } from "./context";
import type { IDatasetExplorationAnalysisStateContext } from "./context";
import type { DatasetExplorationAnalysisAction } from "./actions";

export const DatasetExplorationAnalysisReducer = (
  state: IDatasetExplorationAnalysisStateContext = INITIAL_STATE,
  action: DatasetExplorationAnalysisAction,
) => ({
  ...state,
  ...action.payload,
});
