import { useContext } from 'react';
import { AuthContext, type UserProfile } from '../model/auth-context';

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider');
  }

  return context;
}

export type { UserProfile };
