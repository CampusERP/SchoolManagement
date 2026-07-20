# Phase 1: Critical Bug Fixes — Report

> **Date:** 2026-07-20  
> **Status:** Complete — all 7 fixes implemented, both builds passing

---

## Summary

| # | Fix | Severity | Status |
|---|-----|----------|--------|
| 1 | Tenant isolation for ExamResult, AttendanceRecord, AssignmentSubmission | CRITICAL | ✅ |
| 2 | Missing GradeLevel.Create + Room.Create authorization policies | CRITICAL | ✅ |
| 3 | Dashboard mock data fallbacks removed | HIGH | ✅ |
| 4 | Student/Parent portal routing (infinite redirect loop) | HIGH | ✅ |
| 5 | Teacher portal dead links | MEDIUM | ✅ |
| 6 | TeachersPage missing AssignedClassesCount | LOW | ✅ |
| 7 | React Error Boundary | MEDIUM | ✅ |

---

## Fix 1: Tenant Isolation — ExamResult, AttendanceRecord, AssignmentSubmission

**Problem:**  
`ExamResult`, `AttendanceRecord`, and `AssignmentSubmission` extended `AuditableEntity` instead of `TenantEntity`. They had no `SchoolId` property, no EF global query filter, and no tenant scoping — meaning a school admin could potentially access another school's data through these entities.

**Root cause:**  
These entities were created inside aggregate root methods (`Exam.RecordResult()`, `AttendanceSession.RecordStudent()`, `Assignment.Submit()`) where the parent aggregate already had `SchoolId`, but it was never passed down.

**Fix:**  
- Changed all 3 entities from `AuditableEntity` → `TenantEntity`, added `SchoolId` property
- Updated constructors and `Create()` factory methods to accept `schoolId`
- Updated aggregate root methods to pass `SchoolId` when creating child entities
- Added `SchoolId` indexes to EF configurations
- Updated `ApplicationDbContext` to use `ApplyTenantFilter<>` instead of `ApplySoftDeleteFilter<>` for these entities

**Files changed:**

| File | Change |
|------|--------|
| `Domain/Entities/Exams/ExamResult.cs` | `TenantEntity`, added `SchoolId` param |
| `Domain/Entities/Attendance/AttendanceRecord.cs` | `TenantEntity`, added `SchoolId` param |
| `Domain/Entities/Assignments/AssignmentSubmission.cs` | `TenantEntity`, added `SchoolId` param |
| `Domain/Entities/Exams/Exam.cs` | `RecordResult()` passes `SchoolId` |
| `Domain/Entities/Attendance/AttendanceSession.cs` | `RecordStudent()` passes `SchoolId` |
| `Domain/Entities/Assignments/Assignment.cs` | `Submit()` passes `SchoolId` |
| `Infrastructure/Persistence/ApplicationDbContext.cs` | `ApplyTenantFilter<>` for all 3 entities |
| `Infrastructure/Persistence/.../ExamResultConfiguration.cs` | `SchoolId` index |
| `Infrastructure/Persistence/.../AttendanceConfiguration.cs` | `SchoolId` index on `AttendanceRecord` |
| `Infrastructure/Persistence/.../AssignmentConfiguration.cs` | `SchoolId` index on `AssignmentSubmission` |

**Migration required:**  
A new EF migration must be generated to add the `SchoolId` column to these tables:

```bash
dotnet ef migrations add AddSchoolIdToChildEntities --context ApplicationDbContext --project Infrastructure
```

⚠️ Run this after deploying — existing rows will need `SchoolId` backfilled from their parent aggregate.

---

## Fix 2: Missing Authorization Policies

**Problem:**  
`GradeLevel.Create` and `Room.Create` policies were referenced in `AcademicsController` (`[Authorize(Policy = "GradeLevel.Create")]`) but never registered in `Program.cs` — causing ALL requests to these endpoints to be rejected, including platform admins.

**Fix:**  
- Added both policies to `Program.cs` `permissionPolicies` dictionary
- Added corresponding permissions to both PlatformAdmin and SchoolAdmin in `PermissionProvider.cs`

**Files changed:**

| File | Change |
|------|--------|
| `Api/Program.cs` | Added `"GradeLevel.Create"` and `"Room.Create"` to `permissionPolicies` |
| `Application/Common/Services/PermissionProvider.cs` | Added `gradelevel.create` + `room.create` to both PlatformAdmin and SchoolAdmin |

---

## Fix 3: Dashboard Mock Data Removed

**Problem:**  
`getPlatformDashboard()`, `getSchoolDashboard()`, and `getSchools()` all had try/catch blocks that swallowed API errors and returned hardcoded mock data — hiding real backend failures from users.

**Fix:**  
Removed all try/catch mock data fallbacks. API errors now propagate naturally. The dashboard components already handle `isError` state with proper error UI, so the mock fallbacks were masking real problems.

**Files changed:**

| File | Change |
|------|--------|
| `frontend/src/features/dashboard/api.ts` | Removed mock fallbacks from `getPlatformDashboard`, `getSchoolDashboard`, `getSchools` |

---

## Fix 4: Student/Parent Portal Routing

**Problem:**  
Both `ROLE_HOME` values for `student` and `parent` pointed to `/auth/login`, causing an infinite redirect loop — login → redirect to `/auth/login` → already logged in → redirect again.

**Fix:**  
- Changed `ROLE_HOME` student → `/student`, parent → `/parent`
- Added full route trees for both portals with placeholder pages
- Added sidebar navigation for student and parent roles

**Files changed:**

| File | Change |
|------|--------|
| `frontend/src/lib/constants.ts` | `ROLE_HOME` student → `/student`, parent → `/parent` |
| `frontend/src/router/routes.tsx` | Added student and parent route trees with placeholder pages |

---

## Fix 5: Teacher Portal Dead Links

**Problem:**  
Teacher sidebar had links to `/teacher/attendance` and `/teacher/grades` but no routes were registered for these paths — clicking them showed a blank page.

**Fix:**  
- Added routes for `/teacher/attendance`, `/teacher/grades`, `/teacher/schedule`
- Added placeholder pages for each
- Updated sidebar with Take Attendance, Enter Grades, My Schedule navigation items

**Files changed:**

| File | Change |
|------|--------|
| `frontend/src/router/routes.tsx` | Added teacher attendance/grades/schedule routes |
| `frontend/src/components/organisms/Sidebar.tsx` | Added teacher nav items with `PenLine` and `CalendarDays` icons |

---

## Fix 6: TeachersPage AssignedClassesCount

**Problem:**  
The teachers list API returns `assignedClassesCount` but the table never displayed it.

**Fix:**  
Added the column to the table with monospace formatting and `?? 0` fallback for teachers with no assigned classes.

**Files changed:**

| File | Change |
|------|--------|
| `frontend/src/pages/school/people/TeachersPage.tsx` | Added `assignedClassesCount` column |

---

## Fix 7: React Error Boundary

**Problem:**  
No error boundary existed — any unhandled render error would white-screen the entire app with no recovery path.

**Fix:**  
Created `ErrorBoundary` component with styled error UI (AlertTriangle icon, error message, reset button). Wrapped the entire `<Router />` in `App.tsx`.

**Files changed:**

| File | Change |
|------|--------|
| `frontend/src/components/organisms/ErrorBoundary.tsx` | New file — class component with `componentDidCatch` |
| `frontend/src/App.tsx` | Wrapped `<Router />` with `<ErrorBoundary>` |

---

## Build Verification

| Build | Result |
|-------|--------|
| Backend (`dotnet build --no-incremental`) | ✅ Build succeeded. 0 Error(s), 5 Warning(s) |
| Frontend (`npm run build`) | ✅ Built in 29.80s, no TypeScript errors |

All warnings are pre-existing (CS8618 nullable, CA1816, CA1416, NU1903) and unrelated to Phase 1 changes.

---

## Remaining from Phase 1 Plan

These items from the original plan were **not** addressed in this phase:

| Item | Reason |
|------|--------|
| Implement `PortalReadService` with real EF queries | Requires Phase 2 (Student/Parent portal work) |
| Remove `ChartsPlaceholder` | Requires choosing a charting library — deferred to Phase 8 |
| Fix Student/Teacher/Parent detail pages (sparse) | UX improvement, not a critical bug |

---

## Next Steps (Phase 2)

The next phase should focus on:
1. Generate and apply EF migration for the `SchoolId` column additions
2. Implement real `PortalReadService` queries for student/parent dashboards
3. Build out student and parent portal pages with real data
