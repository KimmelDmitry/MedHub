import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { LoginPage } from '../features/auth/pages/LoginPage';
import { RegisterPage } from '../features/auth/pages/RegisterPage';
import { DashboardPage } from '../features/auth/pages/DashboardPage';
import { AuthProvider } from '../features/auth/model/AuthProvider';
import {
  TeacherAttemptsPage,
  TeacherDashboardPage,
  TeacherLessonsPage,
  TeacherMediaPage,
} from '../features/teacher/pages/TeacherPages';
import { TeacherCoursesPage } from '../features/teacher/courses/pages/TeacherCoursesPage';
import { TeacherCourseNewPage } from '../features/teacher/courses/pages/TeacherCourseNewPage';
import { TeacherCourseDetailPage } from '../features/teacher/courses/pages/TeacherCourseDetailPage';
import { TeacherLessonNewPage } from '../features/teacher/lessons/pages/TeacherLessonNewPage';
import { TeacherLessonDetailPage } from '../features/teacher/lessons/pages/TeacherLessonDetailPage';
import { TeacherCheckpointDetailPage } from '../features/teacher/checkpoints/pages/TeacherCheckpointDetailPage';
import { StudentCatalogPage } from '../features/student/catalog/pages/StudentCatalogPage';
import { StudentCatalogCoursePage } from '../features/student/catalog/pages/StudentCatalogCoursePage';
import { StudentLessonRuntimePage } from '../features/student/runtime/pages/StudentLessonRuntimePage';
import { ProtectedRoute } from './ProtectedRoute';
import { Layout } from './Layout';
import { HomePage } from '../pages/HomePage';

export function Router() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Layout>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route
              path="/dashboard"
              element={
                <ProtectedRoute>
                  <DashboardPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teacher"
              element={
                <ProtectedRoute teacherOnly>
                  <TeacherDashboardPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teacher/courses"
              element={
                <ProtectedRoute teacherOnly>
                  <TeacherCoursesPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teacher/courses/new"
              element={
                <ProtectedRoute teacherOnly>
                  <TeacherCourseNewPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teacher/courses/:courseId/lessons/new"
              element={
                <ProtectedRoute teacherOnly>
                  <TeacherLessonNewPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teacher/courses/:courseId"
              element={
                <ProtectedRoute teacherOnly>
                  <TeacherCourseDetailPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teacher/lessons"
              element={
                <ProtectedRoute teacherOnly>
                  <TeacherLessonsPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teacher/lessons/:lessonId"
              element={
                <ProtectedRoute teacherOnly>
                  <TeacherLessonDetailPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teacher/checkpoints/:checkpointId"
              element={
                <ProtectedRoute teacherOnly>
                  <TeacherCheckpointDetailPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/catalog"
              element={
                <ProtectedRoute>
                  <StudentCatalogPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/catalog/courses/:courseId"
              element={
                <ProtectedRoute>
                  <StudentCatalogCoursePage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/student/lessons/:lessonId"
              element={
                <ProtectedRoute>
                  <StudentLessonRuntimePage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teacher/media"
              element={
                <ProtectedRoute teacherOnly>
                  <TeacherMediaPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teacher/attempts"
              element={
                <ProtectedRoute teacherOnly>
                  <TeacherAttemptsPage />
                </ProtectedRoute>
              }
            />
            <Route path="*" element={<Navigate replace to="/" />} />
          </Routes>
        </Layout>
      </AuthProvider>
    </BrowserRouter>
  );
}
