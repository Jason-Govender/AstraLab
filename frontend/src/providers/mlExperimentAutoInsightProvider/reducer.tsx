import { INITIAL_STATE } from "./context";
import type { IMlExperimentAutoInsightStateContext } from "./context";
import type { MlExperimentAutoInsightAction } from "./actions";

export const MlExperimentAutoInsightReducer = (
  state: IMlExperimentAutoInsightStateContext = INITIAL_STATE,
  action: MlExperimentAutoInsightAction,
) => ({
  ...state,
  ...action.payload,
});
