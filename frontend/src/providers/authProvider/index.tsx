"use client";

import axios from "axios";
import { useContext, useEffect, useReducer, useRef } from "react";
import { useRouter } from "next/navigation";
import type { PropsWithChildren } from "react";
import {
  DASHBOARD_ROUTE,
  DEFAULT_TENANCY_NAME,
  LOGIN_ROUTE,
} from "@/constants/auth";
import type { AbpApiResponse } from "@/types/api";
import type {
  AuthenticateResult,
  AuthSession,
  LoginFormValues,
  LoginRequest,
  RegisterFormValues,
  RegisterRequest,
  TenantAvailabilityState,
} from "@/types/auth";
import {
  clearAuthSession,
  getAuthSession,
  setAuthSession,
} from "@/utils/authCookies";
import { setUnauthorizedHandler } from "@/utils/axiosInstance";
import {
  authenticate,
  getCurrentLoginInformations,
  isTenantAvailable,
  registerAccount,
} from "@/services/authService";
import {
  clearFeedback as clearFeedbackAction,
  initializeAuthError,
  initializeAuthPending,
  initializeAuthSuccess,
  loginError,
  loginPending,
  loginSuccess,
  registerError,
  registerPending,
  registerSuccess,
  logoutSuccess,
} from "./actions";
import { AuthReducer } from "./reducer";
import {
  AuthActionContext,
  AuthStateContext,
  INITIAL_STATE,
} from "./context";

function buildAuthSession(
  payload: AuthenticateResult,
  tenancyName: string,
): AuthSession {
  const expiresAt = new Date(Date.now() + payload.expireInSeconds * 1000);

  return {
    accessToken: payload.accessToken,
    encryptedAccessToken: payload.encryptedAccessToken,
    expireInSeconds: payload.expireInSeconds,
    userId: payload.userId,
    tenancyName,
    expiresAt: expiresAt.toISOString(),
  };
}

function getErrorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    const response = error.response?.data as
      | AbpApiResponse<unknown>
      | { error?: { message?: string } }
      | undefined;

    if (
      response &&
      "error" in response &&
      response.error &&
      typeof response.error.message === "string"
    ) {
      return response.error.message;
    }

    if (typeof error.message === "string" && error.message.length > 0) {
      return error.message;
    }
  }

  if (error instanceof Error && error.message.length > 0) {
    return error.message;
  }

  return "Unable to complete authentication right now.";
}

function getTenantAvailabilityMessage(
  state: TenantAvailabilityState,
  tenancyName: string,
) {
  if (state === 2) {
    return `Tenant "${tenancyName}" is not active.`;
  }

  if (state === 3) {
    return `There is no tenant defined with the name "${tenancyName}".`;
  }

  return "Unable to validate that tenant right now.";
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [state, dispatch] = useReducer(AuthReducer, INITIAL_STATE);
  const router = useRouter();
  const hasInitialized = useRef(false);

  const logout = (options?: { redirectTo?: string }) => {
    clearAuthSession();
    dispatch(logoutSuccess());
    router.replace(options?.redirectTo ?? LOGIN_ROUTE);
  };

  const initializeAuth = async () => {
    dispatch(initializeAuthPending());

    const session = getAuthSession();

    if (!session) {
      dispatch(initializeAuthSuccess({}));
      return;
    }

    try {
      const profile = await getCurrentLoginInformations();

      dispatch(
        initializeAuthSuccess({
          session,
          profile,
        }),
      );
    } catch (error) {
      clearAuthSession();
      dispatch(initializeAuthError(getErrorMessage(error)));
    }
  };

  const clearFeedback = () => {
    dispatch(clearFeedbackAction());
  };

  const performLogin = async (
    values: LoginFormValues,
    options?: { redirectOnSuccess?: boolean },
  ) => {
    dispatch(loginPending());

    const tenancyName =
      values.tenancyName?.trim() || DEFAULT_TENANCY_NAME;
    const requestPayload: LoginRequest = {
      userNameOrEmailAddress: values.userNameOrEmailAddress.trim(),
      password: values.password,
      tenancyName,
      rememberClient: true,
    };

    try {
      const result = await authenticate(requestPayload);
      const session = buildAuthSession(result, tenancyName);
      setAuthSession(session);

      const profile = await getCurrentLoginInformations();

      dispatch(
        loginSuccess({
          session,
          profile,
        }),
      );

      if (options?.redirectOnSuccess ?? true) {
        router.replace(DASHBOARD_ROUTE);
      }
    } catch (error) {
      clearAuthSession();
      dispatch(loginError(getErrorMessage(error)));
      throw error;
    }
  };

  const login = async (values: LoginFormValues) => {
    await performLogin(values);
  };

  const register = async (values: RegisterFormValues) => {
    dispatch(registerPending());

    const requestPayload: RegisterRequest = {
      name: values.name.trim(),
      surname: values.surname.trim(),
      userName: values.userName.trim(),
      emailAddress: values.emailAddress.trim(),
      tenancyName: values.tenancyName.trim(),
      password: values.password,
    };

    try {
      const availability = await isTenantAvailable({
        tenancyName: requestPayload.tenancyName,
      });

      if (availability.state !== 1) {
        throw new Error(
          getTenantAvailabilityMessage(
            availability.state,
            requestPayload.tenancyName,
          ),
        );
      }

      const result = await registerAccount(requestPayload);
      dispatch(registerSuccess());

      if (result.canLogin) {
        await performLogin({
          userNameOrEmailAddress: requestPayload.userName,
          password: requestPayload.password,
          tenancyName: requestPayload.tenancyName,
        });
        return;
      }

      router.replace(`${LOGIN_ROUTE}?registered=1`);
    } catch (error) {
      clearAuthSession();
      dispatch(registerError(getErrorMessage(error)));
      throw error;
    }
  };

  useEffect(() => {
    setUnauthorizedHandler(() => {
      clearAuthSession();
      dispatch(logoutSuccess());
      router.replace(LOGIN_ROUTE);
    });

    return () => {
      setUnauthorizedHandler(null);
    };
  }, [router]);

  useEffect(() => {
    if (hasInitialized.current) {
      return;
    }

    hasInitialized.current = true;
    void initializeAuth();
  }, []);

  return (
    <AuthStateContext.Provider value={state}>
      <AuthActionContext.Provider
        value={{
          initializeAuth,
          login,
          register,
          clearFeedback,
          logout,
        }}
      >
        {children}
      </AuthActionContext.Provider>
    </AuthStateContext.Provider>
  );
}

export function useAuthState() {
  const context = useContext(AuthStateContext);

  if (!context) {
    throw new Error("useAuthState must be used within an AuthProvider");
  }

  return context;
}

export function useAuthActions() {
  const context = useContext(AuthActionContext);

  if (!context) {
    throw new Error("useAuthActions must be used within an AuthProvider");
  }

  return context;
}
