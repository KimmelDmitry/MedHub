import type { CatalogCourseListItem } from '../api/studentCatalogApi';
import { CatalogCourseCard } from './CatalogCourseCard';

type CatalogCourseListProps = {
  courses: CatalogCourseListItem[];
};

export function CatalogCourseList({ courses }: CatalogCourseListProps) {
  return (
    <div className="grid gap-4">
      {courses.map((course) => (
        <CatalogCourseCard key={course.id} course={course} />
      ))}
    </div>
  );
}
