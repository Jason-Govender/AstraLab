import type { AuthProfile, AuthSession } from "@/types/auth";
import type { IAuthStateContext } from "./context";

type AuthStatePatch = Partial<IAuthStateContext>;

export enum AuthActionEnums {
  initializeAuthPending = "INITIALIZE_AUTH_PENDING",
  initializeAuthSuccess = "INITIALIZE_AUTH_SUCCESS",
  initializeAuthError = "INITIALIZE_AUTH_ERROR",
  loginPending = "LOGIN_PENDING",
  loginSuccess = "LOGIN_SUCCESS",
  loginError = "LOGIN_ERROR",
  logout = "LOGOUT",
}

interface AuthSuccessPayload {
  session?: AuthSession;
  profile?: AuthProfile;
}

interface InitializeAuthPendingAction {
  type: AuthActionEnums.initializeAuthPending;
  payload: AuthStatePatch;
}

interface InitializeAuthSuccessAction {
  type: AuthActionEnums.initializeAuthSuccess;
  payload: AuthStatePatch;
}

interface InitializeAuthErrorAction {
  type: AuthActionEnums.initializeAuthError;
  payload: AuthStatePatch;
}

interface LoginPendingAction {
  type: AuthActionEnums.loginPending;
  payload: AuthStatePatch;
}

interface LoginSuccessAction {
  type: AuthActionEnums.loginSuccess;
  payload: AuthStatePatch;
}

interface LoginErrorAction {
  type: AuthActionEnums.loginError;
  payload: AuthStatePatch;
}

interface LogoutAction {
  type: AuthActionEnums.logout;
  payload: AuthStatePatch;
}

export type AuthAction =
  | InitializeAuthPendingAction
  | InitializeAuthSuccessAction
  | InitializeAuthErrorAction
  | LoginPendingAction
  | LoginSuccessAction
  | LoginErrorAction
  | LogoutAction;

export function initializeAuthPending(): InitializeAuthPendingAction {
  return {
    type: AuthActionEnums.initializeAuthPending,
    payload: {
      isInitializing: true,
      isPending: true,
      isError: false,
      errorMessage: undefined,
    },
  };
}

export function initializeAuthSuccess({
  session,
  profile,
}: AuthSuccessPayload): InitializeAuthSuccessAction {
  return {
    type: AuthActionEnums.initializeAuthSuccess,
    payload: {
      isInitialized: true,
      isInitializing: false,
      isPending: false,
      isSuccess: Boolean(session),
      isError: false,
      isAuthenticated: Boolean(session),
      errorMessage: undefined,
      session,
      profile,
    },
  };
}

export function initializeAuthError(
  errorMessage: string,
): InitializeAuthErrorAction {
  return {
    type: AuthActionEnums.initializeAuthError,
    payload: {
      isInitialized: true,
      isInitializing: false,
      isPending: false,
      isSuccess: false,
      isError: true,
      isAuthenticated: false,
      errorMessage,
      session: undefined,
      profile: undefined,
    },
  };
}

export function loginPending(): LoginPendingAction {
  return {
    type: AuthActionEnums.loginPending,
    payload: {
      isLoggingIn: true,
      isPending: true,
      isError: false,
      errorMessage: undefined,
    },
  };
}

export function loginSuccess({
  session,
  profile,
}: AuthSuccessPayload): LoginSuccessAction {
  return {
    type: AuthActionEnums.loginSuccess,
    payload: {
      isInitialized: true,
      isLoggingIn: false,
      isPending: false,
      isSuccess: true,
      isError: false,
      isAuthenticated: true,
      errorMessage: undefined,
      session,
      profile,
    },
  };
}

export function loginError(errorMessage: string): LoginErrorAction {
  return {
    type: AuthActionEnums.loginError,
    payload: {
      isInitialized: true,
      isLoggingIn: false,
      isPending: false,
      isSuccess: false,
      isError: true,
      isAuthenticated: false,
      errorMessage,
      session: undefined,
      profile: undefined,
    },
  };
}

export function logoutSuccess(): LogoutAction {
  return {
    type: AuthActionEnums.logout,
    payload: {
      isInitialized: true,
      isInitializing: false,
      isLoggingIn: false,
      isPending: false,
      isSuccess: false,
      isError: false,
      isAuthenticated: false,
      errorMessage: undefined,
      session: undefined,
      profile: undefined,
    },
  };
}
