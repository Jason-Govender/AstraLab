import { INITIAL_STATE } from "./context";
import type { IDatasetAiAssistantStateContext } from "./context";
import type { DatasetAiAssistantAction } from "./actions";

export const DatasetAiAssistantReducer = (
  state: IDatasetAiAssistantStateContext = INITIAL_STATE,
  action: DatasetAiAssistantAction,
) => ({
  ...state,
  ...action.payload,
});
