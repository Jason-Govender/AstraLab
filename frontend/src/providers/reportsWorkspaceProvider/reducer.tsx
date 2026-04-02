import { INITIAL_STATE } from "./context";
import type { ReportsWorkspaceAction } from "./actions";
import type { IReportsWorkspaceStateContext } from "./context";

export const ReportsWorkspaceReducer = (
  state: IReportsWorkspaceStateContext = INITIAL_STATE,
  action: ReportsWorkspaceAction,
) => ({
  ...state,
  ...action.payload,
});
