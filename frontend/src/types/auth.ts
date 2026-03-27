export interface LoginFormValues {
  userNameOrEmailAddress: string;
  password: string;
  tenancyName?: string;
}

export interface LoginRequest {
  userNameOrEmailAddress: string;
  password: string;
  tenancyName: string;
  rememberClient: boolean;
}

export interface AuthenticateResult {
  accessToken: string;
  encryptedAccessToken: string;
  expireInSeconds: number;
  userId: number;
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
