import { createContext } from "react";
import type { AuthProfile, AuthSession, LoginFormValues } from "@/types/auth";

export interface IAuthStateContext {
  isInitialized: boolean;
  isInitializing: boolean;
  isLoggingIn: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  isAuthenticated: boolean;
  errorMessage?: string;
  session?: AuthSession;
  profile?: AuthProfile;
}

export interface IAuthActionContext {
  initializeAuth: () => Promise<void>;
  login: (values: LoginFormValues) => Promise<void>;
  logout: (options?: { redirectTo?: string }) => void;
}

export const INITIAL_STATE: IAuthStateContext = {
  isInitialized: false,
  isInitializing: false,
  isLoggingIn: false,
  isPending: false,
  isSuccess: false,
  isError: false,
  isAuthenticated: false,
};

export const AuthStateContext = createContext<IAuthStateContext | undefined>(
  undefined,
);
export const AuthActionContext = createContext<IAuthActionContext | undefined>(
  undefined,
);
