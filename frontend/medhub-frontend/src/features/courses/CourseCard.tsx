import { BookOpen } from 'lucide-react';
import { Card, CardHeader, CardTitle, CardDescription, CardFooter } from '../../../@/components/ui/card';
import { cn } from '../../../@/lib/utils';
import type { ReactNode } from 'react';

const colorSchemes = {
  blue: 'text-sky-700 bg-sky-100 ring-sky-100',
  purple: 'text-violet-700 bg-violet-100 ring-violet-100',
  green: 'text-emerald-700 bg-emerald-100 ring-emerald-100',
  orange: 'text-orange-700 bg-orange-100 ring-orange-100',
  pink: 'text-pink-700 bg-pink-100 ring-pink-100',
  red: 'text-rose-700 bg-rose-100 ring-rose-100',
} as const;

type CourseStatus = 'Published' | 'Draft' | 'Archived';

interface CourseCardProps {
  icon: ReactNode;
  title: string;
  description: string;
  lessonsCount: number;
  category: string;
  status: CourseStatus;
  colorScheme?: keyof typeof colorSchemes;
}

export function CourseCard({
  icon,
  title,
  description,
  lessonsCount,
  category,
  status,
  colorScheme = 'blue',
}: CourseCardProps) {
  return (
    <Card className="group relative overflow-hidden border border-slate-200 bg-white shadow-[0_20px_80px_rgba(15,23,42,0.06)] transition duration-300 hover:-translate-y-1 hover:shadow-2xl">
      <div
        className={cn(
          'pointer-events-none absolute inset-x-0 top-0 h-32 rounded-b-[2rem] opacity-70',
          colorSchemes[colorScheme],
        )}
      />
      <div className="relative flex h-full flex-col gap-6 p-6">
        <div className="flex items-start justify-between gap-4">
          <div className={cn('inline-flex h-14 w-14 items-center justify-center rounded-3xl border border-current/20', colorSchemes[colorScheme])}>
            {icon}
          </div>
          <span className="rounded-full bg-slate-950/90 px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] text-white shadow-sm">
            {status}
          </span>
        </div>

        <CardHeader className="p-0 gap-3 border-0">
          <CardTitle className="text-2xl font-semibold text-slate-950">{title}</CardTitle>
          <CardDescription className="text-slate-600">{description}</CardDescription>
        </CardHeader>

        <div className="flex flex-wrap gap-2 py-2 text-sm text-slate-500">
          <span className="rounded-full border border-slate-200 bg-slate-100 px-3 py-1">{category}</span>
          <span className="rounded-full border border-slate-200 bg-slate-100 px-3 py-1">{lessonsCount} уроков</span>
        </div>

        <CardFooter className="mt-auto items-center border-t-0 bg-transparent p-0">
          <div className={cn('inline-flex items-center gap-2 rounded-full px-3 py-2 text-sm font-semibold', colorSchemes[colorScheme], 'bg-white/90 ring-1 ring-slate-200')}>
            <BookOpen className="h-4 w-4" />
            <span>Начать обучение</span>
          </div>
        </CardFooter>
      </div>
    </Card>
  );
}
