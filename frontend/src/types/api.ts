export interface AbpApiError {
  message?: string;
  details?: string;
}

export interface AbpApiResponse<T> {
  result: T;
  success: boolean;
  error?: AbpApiError | null;
  targetUrl?: string | null;
  unAuthorizedRequest?: boolean;
  __abp?: boolean;
}
