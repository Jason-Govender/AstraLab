import { INITIAL_STATE } from "./context";
import type { IAuthStateContext } from "./context";
import type { AuthAction } from "./actions";

export function AuthReducer(
  state: IAuthStateContext = INITIAL_STATE,
  action: AuthAction,
) {
  return {
    ...state,
    ...action.payload,
  };
}
