import { INITIAL_STATE } from "./context";
import type { IDatasetTransformationHistoryStateContext } from "./context";
import type { DatasetTransformationHistoryAction } from "./actions";

export const DatasetTransformationHistoryReducer = (
  state: IDatasetTransformationHistoryStateContext = INITIAL_STATE,
  action: DatasetTransformationHistoryAction,
) => ({
  ...state,
  ...action.payload,
});
