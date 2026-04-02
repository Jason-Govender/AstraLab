import { INITIAL_STATE } from "./context";
import type { AnalyticsDashboardReducerAction } from "./actions";
import type { IAnalyticsDashboardStateContext } from "./context";

export const AnalyticsDashboardReducer = (
  state: IAnalyticsDashboardStateContext = INITIAL_STATE,
  action: AnalyticsDashboardReducerAction,
) => ({
  ...state,
  ...action.payload,
});
