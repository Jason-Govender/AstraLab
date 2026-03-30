import { INITIAL_STATE } from "./context";
import type { IDatasetDetailsStateContext } from "./context";
import type { DatasetDetailsAction } from "./actions";

export const DatasetDetailsReducer = (
  state: IDatasetDetailsStateContext = INITIAL_STATE,
  action: DatasetDetailsAction,
) => ({
  ...state,
  ...action.payload,
});
