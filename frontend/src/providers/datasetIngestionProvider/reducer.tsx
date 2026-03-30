import { INITIAL_STATE } from "./context";
import type { IDatasetIngestionStateContext } from "./context";
import type { DatasetIngestionAction } from "./actions";

export const DatasetIngestionReducer = (
  state: IDatasetIngestionStateContext = INITIAL_STATE,
  action: DatasetIngestionAction,
) => ({
  ...state,
  ...action.payload,
});
