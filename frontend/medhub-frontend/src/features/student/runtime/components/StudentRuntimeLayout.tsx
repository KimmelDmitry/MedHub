import type { ReactNode } from 'react';

type StudentRuntimeLayoutProps = {
  children: ReactNode;
};

export function StudentRuntimeLayout({ children }: StudentRuntimeLayoutProps) {
  return <div className="mx-auto max-w-7xl space-y-6">{children}</div>;
}
