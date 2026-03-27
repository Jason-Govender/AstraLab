import axios, { AxiosHeaders } from "axios";
import { clearAuthSession, getAuthToken } from "@/utils/authCookies";
import { LOGIN_ROUTE } from "@/constants/auth";

declare module "axios" {
  export interface AxiosRequestConfig {
    skipAuthRedirect?: boolean;
  }
}

let unauthorizedHandler: (() => void) | null = null;

export const axiosInstance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL,
});

export function setUnauthorizedHandler(handler: (() => void) | null) {
  unauthorizedHandler = handler;
}

axiosInstance.interceptors.request.use((config) => {
  const token = getAuthToken();

  if (token) {
    config.headers = config.headers ?? new AxiosHeaders();
    config.headers.set("Authorization", `Bearer ${token}`);
  }

  return config;
});

axiosInstance.interceptors.response.use(
  (response) => response,
  async (error) => {
    const skipAuthRedirect = Boolean(error.config?.skipAuthRedirect);

    if (error.response?.status === 401 && !skipAuthRedirect) {
      clearAuthSession();

      if (unauthorizedHandler) {
        unauthorizedHandler();
      } else if (typeof window !== "undefined") {
        window.location.assign(LOGIN_ROUTE);
      }
    }

    return Promise.reject(error);
  },
);
