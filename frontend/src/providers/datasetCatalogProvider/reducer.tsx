import { INITIAL_STATE } from "./context";
import type { IDatasetCatalogStateContext } from "./context";
import type { DatasetCatalogAction } from "./actions";

export const DatasetCatalogReducer = (
  state: IDatasetCatalogStateContext = INITIAL_STATE,
  action: DatasetCatalogAction,
) => ({
  ...state,
  ...action.payload,
});
