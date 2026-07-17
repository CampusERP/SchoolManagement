import { lazy } from "react";
import { Navigate } from "react-router-dom";
import type { RouteObject } from "react-router-dom";
import ProtectedRoute from "@/router/guards";
import AppLayout from "@/layouts/AppLayout";
import AuthLayout from "@/layouts/AuthLayout";

const LoginPage = lazy(() => import("@/pages/auth/LoginPage"));
const PlatformDashboardPage = lazy(() => import("@/pages/platform/PlatformDashboardPage"));
const SchoolDashboardPage = lazy(() => import("@/pages/school/SchoolDashboardPage"));
const TeacherDashboardPage = lazy(() => import("@/pages/teacher/TeacherDashboardPage"));
const PlaceholderPage = lazy(() => import("@/pages/PlaceholderPage"));

const placeholder = (title: string) => (
  <PlaceholderPage title={title} description={`${title} page is under development.`} />
);

export const routes: RouteObject[] = [
  {
    path: "/auth",
    element: <AuthLayout />,
    children: [
      { path: "login", element: <LoginPage /> },
    ],
  },
  {
    path: "/platform",
    element: <ProtectedRoute requiredRole="platform_admin" />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <PlatformDashboardPage /> },
          { path: "schools", element: <PlaceholderPage title="Schools" description="Manage all schools in the platform." /> },
          { path: "schools/new", element: <PlaceholderPage title="Create School" description="Create a new school." /> },
          { path: "schools/:id", element: <PlaceholderPage title="School Details" description="View school details." /> },
          { path: "schools/:id/edit", element: <PlaceholderPage title="Edit School" description="Edit school information." /> },
          { path: "admins/new", element: <PlaceholderPage title="Register Admin" description="Register a new school admin." /> },
        ],
      },
    ],
  },
  {
    path: "/school",
    element: <ProtectedRoute requiredRole="school_admin" />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <SchoolDashboardPage /> },
        ],
      },
    ],
  },
  {
    path: "/academics",
    element: <ProtectedRoute requiredRole="school_admin" />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { path: "years", element: <PlaceholderPage title="Academic Years" description="Manage academic years and terms." /> },
          { path: "classrooms", element: <PlaceholderPage title="Classrooms" description="Manage classrooms and assignments." /> },
          { path: "grade-levels", element: <PlaceholderPage title="Grade Levels" description="Configure grade levels." /> },
          { path: "rooms", element: <PlaceholderPage title="Rooms" description="Manage physical rooms and facilities." /> },
        ],
      },
    ],
  },
  {
    path: "/people",
    element: <ProtectedRoute requiredRole="school_admin" />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { path: "students", element: <PlaceholderPage title="Students" description="Manage student records." /> },
          { path: "students/:id", element: <PlaceholderPage title="Student Profile" description="View student profile and details." /> },
          { path: "teachers", element: <PlaceholderPage title="Teachers" description="Manage teacher records." /> },
          { path: "teachers/:id", element: <PlaceholderPage title="Teacher Profile" description="View teacher profile and details." /> },
          { path: "parents", element: <PlaceholderPage title="Parents" description="Manage parent records." /> },
          { path: "parents/:id", element: <PlaceholderPage title="Parent Profile" description="View parent profile and children." /> },
        ],
      },
    ],
  },
  {
    path: "/enrollment",
    element: <ProtectedRoute requiredRole="school_admin" />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <PlaceholderPage title="Enrollment" description="Enroll students and assign teachers." /> },
          { path: "assign-teacher", element: <PlaceholderPage title="Assign Teacher" description="Assign a teacher to a class." /> },
        ],
      },
    ],
  },
  {
    path: "/teacher",
    element: <ProtectedRoute requiredRole="teacher" />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <TeacherDashboardPage /> },
          { path: "classes", element: <TeacherDashboardPage /> },
        ],
      },
    ],
  },
  { path: "*", element: <Navigate to="/auth/login" replace /> },
];
