import type { AbpApiResponse } from "@/types/api";
import type { AuthProfile, AuthenticateResult, LoginRequest } from "@/types/auth";
import { axiosInstance } from "@/utils/axiosInstance";

const SESSION_INFO_ENDPOINT = "/api/services/app/Session/GetCurrentLoginInformations";
const TOKEN_AUTH_ENDPOINT = "/api/TokenAuth/Authenticate";

export async function authenticate(requestPayload: LoginRequest) {
  const response = await axiosInstance.post<AuthenticateResult>(
    TOKEN_AUTH_ENDPOINT,
    requestPayload,
    {
      skipAuthRedirect: true,
    },
  );

  return response.data;
}

export async function getCurrentLoginInformations() {
  const response = await axiosInstance.get<AbpApiResponse<AuthProfile>>(
    SESSION_INFO_ENDPOINT,
  );

  return response.data.result;
}
