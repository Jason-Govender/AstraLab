import axios from "axios";
import type { AbpApiResponse } from "@/types/api";

export const getApiErrorMessage = (
  error: unknown,
  fallbackMessage: string,
): string => {
  if (axios.isAxiosError(error)) {
    const response = error.response?.data as
      | AbpApiResponse<unknown>
      | { message?: string; error?: { message?: string } }
      | undefined;

    if (
      response &&
      "error" in response &&
      response.error &&
      typeof response.error.message === "string"
    ) {
      return response.error.message;
    }

    if (
      response &&
      "message" in response &&
      typeof response.message === "string" &&
      response.message.length > 0
    ) {
      return response.message;
    }

    if (typeof error.message === "string" && error.message.length > 0) {
      return error.message;
    }
  }

  if (error instanceof Error && error.message.length > 0) {
    return error.message;
  }

  return fallbackMessage;
};
