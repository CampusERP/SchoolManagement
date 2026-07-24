import { lazy } from "react";
import { Navigate } from "react-router-dom";
import type { RouteObject } from "react-router-dom";
import ProtectedRoute from "@/router/guards";
import RequirePermission from "@/router/RequirePermission";
import AppLayout from "@/layouts/AppLayout";
import AuthLayout from "@/layouts/AuthLayout";
import PortalLayout from "@/layouts/PortalLayout";
import { ParentChildrenPage, ParentDashboardPage, ParentResourcePage, StudentDashboardPage, StudentResourcePage } from "@/pages/portal/PortalPages";

const LoginPage = lazy(() => import("@/pages/auth/LoginPage"));
const PlatformDashboardPage = lazy(() => import("@/pages/platform/PlatformDashboardPage"));
const SchoolsPage = lazy(() => import("@/pages/platform/SchoolsPage"));
const CreateSchoolPage = lazy(() => import("@/pages/platform/CreateSchoolPage"));
const RegisterAdminPage = lazy(() => import("@/pages/platform/RegisterAdminPage"));
const SchoolDetailPage = lazy(() => import("@/pages/platform/SchoolDetailPage"));
const EditSchoolPage = lazy(() => import("@/pages/platform/EditSchoolPage"));
const SchoolDashboardPage = lazy(() => import("@/pages/school/SchoolDashboardPage"));
const TeacherDashboardPage = lazy(() => import("@/pages/teacher/TeacherDashboardPage"));
const TeacherClassesPage = lazy(() => import("@/pages/teacher/TeacherClassesPage"));
const TeacherAttendancePage = lazy(() => import("@/pages/teacher/TeacherAttendancePage"));
const TeacherGradesPage = lazy(() => import("@/pages/teacher/TeacherGradesPage"));
const TeacherSchedulePage = lazy(() => import("@/pages/teacher/TeacherSchedulePage"));
const TeacherExamsPage = lazy(() => import("@/pages/teacher/TeacherExamsPage"));
const PlaceholderPage = lazy(() => import("@/pages/PlaceholderPage"));

// School admin — Academics
const AcademicYearsPage = lazy(() => import("@/pages/school/academic/AcademicYearsPage"));
const ClassroomsPage = lazy(() => import("@/pages/school/academic/ClassroomsPage"));
const ClassroomDetailPage = lazy(() => import("@/pages/school/academic/ClassroomDetailPage"));
const GradeLevelsPage = lazy(() => import("@/pages/school/academic/GradeLevelsPage"));
const EducationStagesPage = lazy(() => import("@/pages/school/academic/EducationStagesPage"));
const RoomsPage = lazy(() => import("@/pages/school/academic/RoomsPage"));

// School admin — People
const StudentsPage = lazy(() => import("@/pages/school/people/StudentsPage"));
const StudentDetailPage = lazy(() => import("@/pages/school/people/StudentDetailPage"));
const TeachersPage = lazy(() => import("@/pages/school/people/TeachersPage"));
const TeacherDetailPage = lazy(() => import("@/pages/school/people/TeacherDetailPage"));
const ParentsPage = lazy(() => import("@/pages/school/people/ParentsPage"));
const ParentDetailPage = lazy(() => import("@/pages/school/people/ParentDetailPage"));
const StudentGuardiansPage = lazy(() => import("@/pages/school/people/StudentGuardiansPage"));

// School admin — Enrollment
const EnrollmentPage = lazy(() => import("@/pages/school/enrollment/EnrollmentPage"));
const AssignTeacherPage = lazy(() => import("@/pages/school/enrollment/AssignTeacherPage"));

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
          { path: "schools", element: <SchoolsPage /> },
          { path: "schools/new", element: <CreateSchoolPage /> },
          { path: "schools/:id", element: <SchoolDetailPage /> },
          { path: "schools/:id/edit", element: <EditSchoolPage /> },
          { path: "admins/new", element: <RegisterAdminPage /> },
        ],
      },
    ],
  },
  {
    path: "/school",
    element: <RequirePermission permission="school.dashboard" requiredRole="school_admin" />,
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
    element: <RequirePermission permission="academicyear.read" requiredRole="school_admin" />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { path: "years", element: <AcademicYearsPage /> },
          { path: "classrooms", element: <ClassroomsPage /> },
          { path: "classrooms/:id", element: <ClassroomDetailPage /> },
          { path: "grade-levels", element: <GradeLevelsPage /> },
          { path: "education-stages", element: <EducationStagesPage /> },
          { path: "rooms", element: <RoomsPage /> },
        ],
      },
    ],
  },
  {
    path: "/people",
    element: <RequirePermission permission="student.read" requiredRole="school_admin" />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { path: "students", element: <StudentsPage /> },
          { path: "students/:id", element: <StudentDetailPage /> },
          { path: "teachers", element: <TeachersPage /> },
          { path: "teachers/:id", element: <TeacherDetailPage /> },
          { path: "parents", element: <ParentsPage /> },
          { path: "parents/:id", element: <ParentDetailPage /> },
          { path: "student-guardians", element: <StudentGuardiansPage /> },
        ],
      },
    ],
  },
  {
    path: "/enrollment",
    element: <RequirePermission permission="enrollment.create" requiredRole="school_admin" />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <EnrollmentPage /> },
          { path: "assign-teacher", element: <AssignTeacherPage /> },
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
          { path: "classes", element: <TeacherClassesPage /> },
          { path: "attendance", element: <TeacherAttendancePage /> },
          { path: "grades", element: <TeacherGradesPage /> },
          { path: "schedule", element: <TeacherSchedulePage /> },
          { path: "exams", element: <TeacherExamsPage /> },
        ],
      },
    ],
  },
  {
    path: "/student",
    element: <ProtectedRoute requiredRole="student" />,
    children: [
      {
        element: <PortalLayout />,
        children: [
          { index: true, element: <PlaceholderPage title="Student Dashboard" description="Your classes, grades, and schedule — coming soon" /> },
          { path: "classes", element: <PlaceholderPage title="My Classes" description="Enrolled classes — coming soon" /> },
          { path: "grades", element: <PlaceholderPage title="My Grades" description="Exam results and report cards — coming soon" /> },
          { path: "schedule", element: <PlaceholderPage title="My Schedule" description="Weekly timetable — coming soon" /> },
          { path: "attendance", element: <PlaceholderPage title="My Attendance" description="Attendance history — coming soon" /> },
          { path: "profile", element: <PlaceholderPage title="My Profile" description="Personal information — coming soon" /> },
        ],
      },
    ],
  },
  {
    path: "/parent",
    element: <ProtectedRoute requiredRole="parent" />,
    children: [
      {
        element: <PortalLayout />,
        children: [
          { index: true, element: <PlaceholderPage title="Parent Dashboard" description="Your children's academic overview — coming soon" /> },
          { path: "children", element: <PlaceholderPage title="My Children" description="Children list and details — coming soon" /> },
          { path: "grades", element: <PlaceholderPage title="Children's Grades" description="Exam results and report cards — coming soon" /> },
          { path: "attendance", element: <PlaceholderPage title="Children's Attendance" description="Attendance history — coming soon" /> },
          { path: "profile", element: <PlaceholderPage title="My Profile" description="Personal information — coming soon" /> },
        ],
      },
    ],
  },
  { path: "/student/assignments", element: <ProtectedRoute requiredRole="student" />, children: [{ element: <PortalLayout />, children: [{ index: true, element: <StudentResourcePage title="Assignments" resource="assignments" /> }] }] },
  { path: "/student/notifications", element: <ProtectedRoute requiredRole="student" />, children: [{ element: <PortalLayout />, children: [{ index: true, element: <StudentResourcePage title="Notices" resource="notifications" /> }] }] },
  { path: "/student/report-cards", element: <ProtectedRoute requiredRole="student" />, children: [{ element: <PortalLayout />, children: [{ index: true, element: <StudentResourcePage title="Report cards" resource="report-cards" /> }] }] },
  { path: "/parent/children/:studentId", element: <ProtectedRoute requiredRole="parent" />, children: [{ element: <PortalLayout />, children: [{ index: true, element: <ParentResourcePage title="Child overview" resource="profile" /> }, { path: "attendance", element: <ParentResourcePage title="Attendance" resource="attendance" /> }, { path: "grades", element: <ParentResourcePage title="Grades" resource="grades" /> }, { path: "assignments", element: <ParentResourcePage title="Assignments" resource="assignments" /> }, { path: "report-cards", element: <ParentResourcePage title="Report cards" resource="report-cards" /> }] }] },
  { path: "/parent/billing", element: <ProtectedRoute requiredRole="parent" />, children: [{ element: <PortalLayout />, children: [{ index: true, element: <ParentResourcePage title="Payments & fees" resource="billing" /> }] }] },
  { path: "/parent/notifications", element: <ProtectedRoute requiredRole="parent" />, children: [{ element: <PortalLayout />, children: [{ index: true, element: <ParentResourcePage title="Notices" resource="notifications" /> }] }] },
  { path: "*", element: <Navigate to="/auth/login" replace /> },
];
