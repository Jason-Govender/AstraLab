import { INITIAL_STATE } from "./context";
import type { IDatasetAutoInsightStateContext } from "./context";
import type { DatasetAutoInsightAction } from "./actions";

export const DatasetAutoInsightReducer = (
  state: IDatasetAutoInsightStateContext = INITIAL_STATE,
  action: DatasetAutoInsightAction,
) => ({
  ...state,
  ...action.payload,
});
