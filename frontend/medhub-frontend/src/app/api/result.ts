export interface ApiResult<T> {
  isSuccess: boolean;
  value?: T;
  errors?: string[];
  statusCode?: number;
}

export class ApiError extends Error {
  public errors: string[];
  public statusCode?: number;

  constructor(errors: string[], statusCode?: number) {
    super(Array.isArray(errors) ? errors.join('; ') : String(errors));
    this.name = 'ApiError';
    this.errors = errors;
    this.statusCode = statusCode;
  }
}

export function unwrapResult<T>(response: { data?: ApiResult<T> }): T {
  const body = response?.data;
  if (!body) {
    throw new ApiError(['Empty response body']);
  }
  if (!body.isSuccess) {
    throw new ApiError(body.errors || ['Unknown error'], body.statusCode);
  }
  if (body.value === undefined) {
    throw new ApiError(['Response has no value despite isSuccess=true'], body.statusCode);
  }
  return body.value as T;
}
