import { INITIAL_STATE } from "./context";
import type { IDatasetExplorationStateContext } from "./context";
import type { DatasetExplorationAction } from "./actions";

export const DatasetExplorationReducer = (
  state: IDatasetExplorationStateContext = INITIAL_STATE,
  action: DatasetExplorationAction,
) => ({
  ...state,
  ...action.payload,
});
