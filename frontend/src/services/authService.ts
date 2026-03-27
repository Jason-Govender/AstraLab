import type { AbpApiResponse } from "@/types/api";
import type {
  AuthProfile,
  AuthenticateResult,
  LoginRequest,
  RegisterRequest,
  RegisterResult,
  TenantAvailabilityRequest,
  TenantAvailabilityResult,
} from "@/types/auth";
import { axiosInstance } from "@/utils/axiosInstance";

const SESSION_INFO_ENDPOINT = "/api/services/app/Session/GetCurrentLoginInformations";
const TOKEN_AUTH_ENDPOINT = "/api/TokenAuth/Authenticate";
const REGISTER_ENDPOINT = "/api/services/app/Account/Register";
const TENANT_AVAILABILITY_ENDPOINT = "/api/services/app/Account/IsTenantAvailable";

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

export async function registerAccount(requestPayload: RegisterRequest) {
  const response = await axiosInstance.post<AbpApiResponse<RegisterResult>>(
    REGISTER_ENDPOINT,
    requestPayload,
    {
      skipAuthRedirect: true,
    },
  );

  return response.data.result;
}

export async function isTenantAvailable(
  requestPayload: TenantAvailabilityRequest,
) {
  const response = await axiosInstance.post<
    AbpApiResponse<TenantAvailabilityResult>
  >(TENANT_AVAILABILITY_ENDPOINT, requestPayload, {
    skipAuthRedirect: true,
  });

  return response.data.result;
}

export async function getCurrentLoginInformations() {
  const response = await axiosInstance.get<AbpApiResponse<AuthProfile>>(
    SESSION_INFO_ENDPOINT,
  );

  return response.data.result;
}
