import { useCallback, useMemo, useState, type ReactNode } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import type { AxiosResponse } from 'axios';
import { api, clearAuthTokens, getStoredTokens, setAuthTokens, type AuthTokens } from '../../../app/api/client';
import type { LogInUserRequest, RegisterUserRequest } from '../../../generated/myApi';
import { AuthContext, type UserProfile } from './auth-context';

type TokenPayload = {
  accessToken?: string | null;
  refreshToken?: string | null;
};

type TokenResponse = TokenPayload & {
  isSuccess?: boolean;
  value?: TokenPayload | null;
};

const teacherPermissions = new Set([
  'courses:create',
  'courses:update',
  'lessons:create',
  'lessons:update',
  'media:upload',
]);

const meQueryKey = ['me'] as const;

function extractTokens(response: AxiosResponse<unknown>): AuthTokens {
  const body = (response.data ?? {}) as TokenResponse;
  let tokens: TokenPayload | null = null;

  if (body.isSuccess && body.value) {
    tokens = body.value;
  } else if (body.accessToken) {
    tokens = body;
  }

  if (!tokens?.accessToken) {
    const authHeader = response.headers?.authorization ?? response.headers?.Authorization;

    if (typeof authHeader === 'string' && authHeader.startsWith('Bearer ')) {
      tokens = { accessToken: authHeader.replace('Bearer ', '') };
    }
  }

  if (!tokens?.accessToken) {
    return null;
  }

  return {
    accessToken: tokens.accessToken,
    refreshToken: tokens.refreshToken ?? undefined,
  };
}

async function fetchUserProfile(): Promise<UserProfile | null> {
  const response = (await api.api.v1UsersMeList()) as unknown as AxiosResponse<UserProfile>;
  return response.data ?? null;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [tokens, setTokens] = useState<AuthTokens>(() => getStoredTokens());
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const profileQuery = useQuery<UserProfile | null, Error>({
    queryKey: meQueryKey,
    queryFn: fetchUserProfile,
    enabled: Boolean(tokens?.accessToken),
    retry: false,
    staleTime: 5 * 60_000,
  });

  const authenticate = useCallback(
    async (authTokens: AuthTokens) => {
      if (!authTokens?.accessToken) {
        throw new Error('No access token received from auth endpoint');
      }

      setAuthTokens(authTokens);
      setTokens(authTokens);
      queryClient.removeQueries({ queryKey: meQueryKey });

      await queryClient.fetchQuery({
        queryKey: meQueryKey,
        queryFn: fetchUserProfile,
        staleTime: 5 * 60_000,
      });
    },
    [queryClient],
  );

  const loginWithCredentials = useCallback(
    async (credentials: LogInUserRequest) => {
      const response = (await api.api.v1UsersLoginCreate(credentials)) as unknown as AxiosResponse<unknown>;
      await authenticate(extractTokens(response));
    },
    [authenticate],
  );

  const loginMutation = useMutation<void, Error, LogInUserRequest>({
    mutationFn: loginWithCredentials,
  });

  const registerMutation = useMutation<void, Error, RegisterUserRequest>({
    mutationFn: async (payload) => {
      const response = (await api.api.v1UsersRegisterCreate(payload)) as unknown as AxiosResponse<unknown>;
      const registerTokens = extractTokens(response);

      if (registerTokens?.accessToken) {
        await authenticate(registerTokens);
        return;
      }

      if (!payload.email || !payload.password) {
        throw new Error('Registration succeeded, but auto-login credentials are missing.');
      }

      await loginWithCredentials({
        email: payload.email,
        password: payload.password,
      });
    },
  });

  const logout = useCallback(() => {
    clearAuthTokens();
    setTokens(null);
    queryClient.setQueryData(meQueryKey, null);
    queryClient.removeQueries({ queryKey: meQueryKey });
    navigate('/login', { replace: true });
  }, [navigate, queryClient]);

  const user = profileQuery.data;
  const permissions = useMemo(
    () => (Array.isArray(user?.permissions) ? user.permissions : []),
    [user],
  );
  const role = user?.role;
  const normalizedRole = role?.trim().toLowerCase();
  const hasTeacherAccess = useMemo(
    () =>
      normalizedRole === 'teacher' ||
      normalizedRole === 'admin' ||
      permissions.some((permission) => teacherPermissions.has(permission.trim().toLowerCase())),
    [normalizedRole, permissions],
  );

  const value = useMemo(
    () => ({
      tokens,
      user,
      role,
      permissions,
      hasTeacherAccess,
      isAuthenticated: Boolean(tokens?.accessToken),
      isProfileLoading: profileQuery.isLoading || profileQuery.isFetching,
      isProfileError: profileQuery.isError,
      isPending: loginMutation.isPending || registerMutation.isPending,
      error: loginMutation.error ?? registerMutation.error ?? null,
      login: loginMutation.mutateAsync,
      register: registerMutation.mutateAsync,
      logout,
      refetchProfile: profileQuery.refetch,
    }),
    [
      tokens,
      user,
      role,
      permissions,
      hasTeacherAccess,
      profileQuery.isLoading,
      profileQuery.isFetching,
      profileQuery.isError,
      profileQuery.refetch,
      loginMutation.isPending,
      loginMutation.error,
      loginMutation.mutateAsync,
      registerMutation.isPending,
      registerMutation.error,
      registerMutation.mutateAsync,
      logout,
    ],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
