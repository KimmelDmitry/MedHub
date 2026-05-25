import { createContext } from 'react';
import type { AuthTokens } from '../../../app/api/client';
import type { LogInUserRequest, RegisterUserRequest } from '../../../generated/myApi';

export type RegisterPayload = RegisterUserRequest & {
  teacherRegistrationCode?: string;
};

export interface UserProfile {
  id?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  role?: string;
  permissions?: string[];
}

export interface AuthContextValue {
  tokens: AuthTokens;
  user: UserProfile | null | undefined;
  role: string | undefined;
  permissions: string[];
  hasTeacherAccess: boolean;
  isAuthenticated: boolean;
  isProfileLoading: boolean;
  isProfileError: boolean;
  isPending: boolean;
  error: Error | null;
  login: (credentials: LogInUserRequest) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  logout: () => void;
  refetchProfile: () => Promise<unknown>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
