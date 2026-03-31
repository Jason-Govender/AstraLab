import { INITIAL_STATE } from "./context";
import type { IProcessedDatasetVersionStateContext } from "./context";
import type { ProcessedDatasetVersionAction } from "./actions";

export const ProcessedDatasetVersionReducer = (
  state: IProcessedDatasetVersionStateContext = INITIAL_STATE,
  action: ProcessedDatasetVersionAction,
) => ({
  ...state,
  ...action.payload,
});
