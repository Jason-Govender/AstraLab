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

interface AuthenticateResultResponse {
  accessToken?: string;
  encryptedAccessToken?: string;
  expireInSeconds?: number;
  userId?: number;
  AccessToken?: string;
  EncryptedAccessToken?: string;
  ExpireInSeconds?: number;
  UserId?: number;
}

function normalizeAuthenticateResult(
  payload: AuthenticateResultResponse,
): AuthenticateResult {
  const accessToken = payload.accessToken ?? payload.AccessToken;
  const encryptedAccessToken =
    payload.encryptedAccessToken ?? payload.EncryptedAccessToken;
  const expireInSeconds = Number(
    payload.expireInSeconds ?? payload.ExpireInSeconds,
  );
  const userId = Number(payload.userId ?? payload.UserId);

  if (
    !accessToken ||
    !encryptedAccessToken ||
    !Number.isFinite(expireInSeconds) ||
    !Number.isFinite(userId)
  ) {
    throw new Error("Unexpected authentication response from the API.");
  }

  return {
    accessToken,
    encryptedAccessToken,
    expireInSeconds,
    userId,
  };
}

export async function authenticate(requestPayload: LoginRequest) {
  const response = await axiosInstance.post<AuthenticateResultResponse>(
    TOKEN_AUTH_ENDPOINT,
    requestPayload,
    {
      skipAuthRedirect: true,
    },
  );

  return normalizeAuthenticateResult(response.data);
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
