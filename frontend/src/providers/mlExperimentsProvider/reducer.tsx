import { INITIAL_STATE } from "./context";
import type { IMlExperimentsStateContext } from "./context";
import type { MlExperimentsAction } from "./actions";

export const MlExperimentsReducer = (
  state: IMlExperimentsStateContext = INITIAL_STATE,
  action: MlExperimentsAction,
) => ({
  ...state,
  ...action.payload,
});
