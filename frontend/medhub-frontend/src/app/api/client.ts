import { Api } from '../../generated/myApi';

export type AuthTokens = { accessToken: string; refreshToken?: string } | null;

const getApiBase = (): string => {
  const envBase = import.meta.env.VITE_API_BASE;
  return envBase && envBase.length > 0 ? envBase : '';
};

export const apiBaseUrl = getApiBase();

export const api = new Api({
  baseURL: apiBaseUrl,
  secure: true,
  securityWorker: async (tokens: AuthTokens | null) => ({
    headers: tokens?.accessToken
      ? { Authorization: `Bearer ${tokens.accessToken}` }
      : {},
  }),
});

export const setAuthTokens = (tokens: AuthTokens) => {
  api.setSecurityData(tokens);
  if (tokens) {
    try {
      localStorage.setItem('auth_tokens', JSON.stringify(tokens));
    } catch {
      // ignore storage errors
    }
  } else {
    try {
      localStorage.removeItem('auth_tokens');
    } catch {
      // ignore storage errors
    }
  }
};

export const getStoredTokens = (): AuthTokens => {
  try {
    const raw = localStorage.getItem('auth_tokens');
    return raw ? (JSON.parse(raw) as AuthTokens) : null;
  } catch {
    return null;
  }
};

const initialTokens = getStoredTokens();
if (initialTokens) {
  setAuthTokens(initialTokens);
}

export const clearAuthTokens = () => setAuthTokens(null);

export default api;
