# Phase 3: Teacher Dashboard Backend

## Summary

Implemented the critical missing `GET /api/teacher/dashboard` backend endpoint that the
frontend already calls at `TeacherDashboardPage.tsx`. Build passes with 0 errors.

## What Was Implemented

### New Files (3)

| File | Purpose |
|------|---------|
| `Application/Features/Portal/Queries/GetTeacherDashboard/TeacherDashboardDto.cs` | DTOs: `TeacherDashboardDto`, `TeacherDashboardScheduleSlot`, `TeacherDashboardClass`, `TeacherDashboardAnnouncement` |
| `Application/Features/Portal/Queries/GetTeacherDashboard/GetTeacherDashboardQuery.cs` | MediatR query record |
| `Application/Features/Portal/Queries/GetTeacherDashboard/GetTeacherDashboardQueryHandler.cs` | Handler — resolves `ICurrentUserService.UserId` and delegates to `IPortalReadService` |

### Modified Files (4)

| File | Change |
|------|--------|
| `Application/Common/Interfaces/Services/IPortalReadService.cs` | Added `GetTeacherDashboardAsync(Guid schoolId, Guid userId, CancellationToken ct)` |
| `Infrastructure/Persistence/Services/PortalReadService.cs` | Added ~120-line implementation of `GetTeacherDashboardAsync` with real EF Core queries |
| `Api/Controllers/PortalController.cs` | Added `GET /api/teacher/dashboard?schoolId={schoolId}` endpoint with `Schedule.Read` policy |
| `docs/phase3-teacher-dashboard-report.md` | This report |

## API Contract

```
GET /api/teacher/dashboard?schoolId={schoolId}
Authorization: Bearer <token> (requires Schedule.Read policy)
```

### Response Shape

```json
{
  "totalClasses": 3,
  "totalStudents": 105,
  "todayLessons": 2,
  "pendingAttendance": 1,
  "pendingAssignments": 3,
  "todaySchedule": [
    {
      "teachingAssignmentId": "...",
      "className": "Grade 10-A",
      "subject": "Mathematics",
      "teacherName": "John Smith",
      "room": "Room 101",
      "startTime": "08:00:00",
      "endTime": "09:00:00",
      "status": "completed"
    }
  ],
  "myClasses": [
    {
      "classRoomId": "...",
      "name": "Grade 10-A",
      "subject": "Mathematics",
      "teacherName": "John Smith",
      "studentCount": 35,
      "gradeLevel": "Grade 10"
    }
  ],
  "announcements": [
    {
      "id": "...",
      "title": "Staff Meeting",
      "content": "Mandatory staff meeting...",
      "author": "System",
      "createdAtUtc": "2026-07-20T10:00:00Z"
    }
  ]
}
```

## How the Dashboard Data Is Computed

| Field | Source | Logic |
|-------|--------|-------|
| `totalClasses` | `TeachingAssignments` → `ClassRooms` | Distinct classrooms per teacher |
| `totalStudents` | `StudentEnrollments` | Count of active enrollments per classroom |
| `todayLessons` | `ClassSchedules` | Count of slots where `DayOfWeek == today` |
| `pendingAttendance` | `AttendanceSessions` | Sessions for today that are not locked |
| `pendingAssignments` | `Assignments` + `AssignmentSubmissions` | Assignments with at least one ungraded submission |
| `todaySchedule` | `ClassSchedules` + `Subjects` + `Rooms` | Today's timetable with status (upcoming/in_progress/completed) |
| `myClasses` | `TeachingAssignments` + `ClassRooms` + `GradeLevels` | Class list with student counts |
| `announcements` | `NotificationBatches` | Last 5 school-wide notifications as announcements |

## Security

- Teacher is resolved from `ICurrentUserService.UserId` (JWT `sub` claim) → `Teacher.ApplicationUserId`
- Only returns data for the teacher's own classes within the specified school
- All queries use `AsNoTracking()` for read-only performance
- Tenant isolation enforced via `SchoolId` parameter

## Teacher Dashboard Status vs Phase 3 Plan

| Phase 3 Task | Backend Status |
|-------------|:---:|
| Teacher Dashboard endpoint | **DONE** |
| Teacher Classes page | Schedule + Roster endpoints exist (Phase 2) |
| Teacher Attendance page | `AttendanceReadService` + `AssignmentSubmissionCommand` exist |
| Teacher Grades page | `ExamReadService` + `ExamResultCommand` exist |
| Teacher Schedule page | `GET /api/portal/teacher/{teacherId}/schedule` exists (Phase 2) |

## Build Status

```
Build succeeded.
0 Error(s)
1 Warning(s)  (existing NU1903 — Microsoft.OpenApi vulnerability, pre-existing)
```
