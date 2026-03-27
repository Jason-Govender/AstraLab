export interface LoginFormValues {
  userNameOrEmailAddress: string;
  password: string;
  tenancyName?: string;
}

export interface RegisterFormValues {
  name: string;
  surname: string;
  userName: string;
  emailAddress: string;
  tenancyName: string;
  password: string;
  confirmPassword: string;
  acceptTerms: boolean;
}

export interface LoginRequest {
  userNameOrEmailAddress: string;
  password: string;
  tenancyName: string;
  rememberClient: boolean;
}

export interface RegisterRequest {
  name: string;
  surname: string;
  userName: string;
  emailAddress: string;
  tenancyName: string;
  password: string;
}

export interface TenantAvailabilityRequest {
  tenancyName: string;
}

export interface AuthenticateResult {
  accessToken: string;
  encryptedAccessToken: string;
  expireInSeconds: number;
  userId: number;
}

export interface RegisterResult {
  canLogin: boolean;
}

export enum TenantAvailabilityState {
  Available = 1,
  InActive = 2,
  NotFound = 3,
}

export interface TenantAvailabilityResult {
  state: TenantAvailabilityState;
  tenantId?: number | null;
}

export interface AuthSession {
  accessToken: string;
  encryptedAccessToken: string;
  expireInSeconds: number;
  userId: number;
  expiresAt: string;
  tenancyName: string;
}

export interface ApplicationInfo {
  version: string;
  releaseDate: string;
  features: Record<string, boolean>;
}

export interface AuthUser {
  id: number;
  name: string;
  surname: string;
  userName: string;
  emailAddress: string;
}

export interface AuthTenant {
  id: number;
  tenancyName: string;
  name: string;
}

export interface AuthProfile {
  application?: ApplicationInfo;
  user?: AuthUser;
  tenant?: AuthTenant;
}
