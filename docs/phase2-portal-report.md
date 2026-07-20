# Phase 2: Student & Parent Portal Backend — Report

> **Date:** 2026-07-20  
> **Status:** Complete — all portal endpoints implemented, backend build passing

---

## Summary

| Area | Status |
|------|--------|
| PortalReadService | All stubs replaced with real EF Core queries (17 methods) |
| Student Dashboard | `GET /portal/student/{enrollmentId}/dashboard` — fully implemented |
| Parent Dashboard | `GET /portal/parent/dashboard` — fully implemented |
| Student APIs | 8 endpoints (profile, schedule, classes, assignments, attendance, exams, report cards, notifications) |
| Parent APIs | 9 endpoints (dashboard, child profile, child attendance, child grades, child assignments, child report cards, billing, notifications) |
| Ownership Validation | All parent endpoints validate `StudentGuardian` relationship |
| Tenant Isolation | All queries filter by `SchoolId` |
| Existing Read Services | NotificationReadService, AttendanceReadService, ExamReadService, AssignmentReadService — all stubs replaced |

---

## APIs Implemented

### Student Portal (9 endpoints)

| Method | Route | Policy | Description |
|--------|-------|--------|-------------|
| GET | `/portal/student/{enrollmentId}/dashboard` | `Profile.Read` | Full dashboard: profile, schedule, assignments, attendance, grades, notifications, GPA |
| GET | `/portal/student/profile` | `Profile.Read` | Student profile details (delegates to existing `GetMyProfileQuery`) |
| GET | `/portal/student/{enrollmentId}/schedule` | `Attendance.ReadOwn` | Weekly timetable for a term |
| GET | `/portal/student/{enrollmentId}/classes` | `MyClasses.Read` | Current classes with subjects, teachers, and schedule slots |
| GET | `/portal/student/{enrollmentId}/assignments` | `Assignment.ReadOwn` | All assignments with submission status |
| GET | `/portal/student/{enrollmentId}/attendance` | `Attendance.ReadOwn` | Attendance history (paginated) |
| GET | `/portal/student/{enrollmentId}/exams` | `Exam.Read` | Exam results (optionally filtered by term) |
| GET | `/portal/student/{enrollmentId}/report-cards` | `Exam.Read` | Report cards with subject breakdowns |
| GET | `/portal/student/notifications` | `Notification.Read` | Notifications list |

### Parent Portal (9 endpoints)

| Method | Route | Policy | Description |
|--------|-------|--------|-------------|
| GET | `/portal/parent/dashboard` | `Children.Read` | Dashboard: children overview, attendance, grades, invoices, notifications |
| GET | `/portal/parent/children/{studentId}/profile` | `Children.Read` | Child profile with academic summary |
| GET | `/portal/parent/children/{studentId}/attendance` | `Attendance.ReadChild` | Child attendance history (paginated) |
| GET | `/portal/parent/children/{studentId}/grades` | `Children.Read` | Child exam grades (optionally filtered by term) |
| GET | `/portal/parent/children/{studentId}/assignments` | `Children.Read` | Child assignments with submission status |
| GET | `/portal/parent/children/{studentId}/report-cards` | `Children.Read` | Child report cards |
| GET | `/portal/parent/billing` | `Children.Read` | School invoices |
| GET | `/portal/parent/notifications` | `Notification.Read` | Notifications list |

### Teacher Portal (2 endpoints — existing, now with real data)

| Method | Route | Policy | Description |
|--------|-------|--------|-------------|
| GET | `/portal/teacher/{teacherId}/schedule` | `Schedule.Read` | Weekly timetable (was stub, now real) |
| GET | `/portal/classrooms/{classRoomId}/roster` | `Schedule.Read` | Classroom roster (was stub, now real) |

---

## DTOs Added

### Shared DTOs (`Features/Portal/Queries/Shared/`)

| DTO | Properties |
|-----|------------|
| `PortalAssignmentDto` | AssignmentId, Title, SubjectName, TeacherName, DueDate, MaxScore, SubmissionStatus, Grade, TeacherFeedback, SubmittedAt |
| `PortalExamResultDto` | SubjectName, ExamName, Score, MaxScore, Percentage, Grade, Remarks |
| `PortalReportCardDto` | ReportCardId, TermName, OverallPercentage, OverallGrade, IsLocked, GeneratedAtUtc, Subjects |
| `PortalReportCardSubjectDto` | SubjectName, Score, MaxScore, Grade |
| `PortalAttendanceSummaryDto` | TotalDays, PresentDays, AbsentDays, LateDays, AttendancePercentage |
| `PortalAttendanceRecordDto` | Date, SubjectName, Status, Note |
| `PortalInvoiceDto` | Id, Amount, PaidAmount, BalanceDue, DueDate, Status, CreatedAtUtc |
| `PortalNotificationDto` | Id, Subject, Body, Status, CreatedAtUtc, ReadAtUtc |

### Student Dashboard DTOs (`Features/Portal/Queries/GetStudentDashboard/`)

| DTO | Properties |
|-----|------------|
| `StudentDashboardDto` | StudentId, StudentCode, FirstName, LastName, AcademicYearName, ClassRoomName, GradeLevelName, CurrentEnrollmentId, TodaySchedule, UpcomingAssignments, TotalAttendanceDays, PresentDays, AttendancePercentage, RecentGrades, UnreadNotificationCount, OverallPercentage, OverallGrade |
| `StudentDashboardScheduleSlot` | SubjectName, TeacherName, RoomName, StartTime, EndTime |
| `StudentDashboardAssignment` | AssignmentId, Title, SubjectName, DueDate, MaxScore, SubmissionStatus, Grade |
| `StudentDashboardExamResult` | SubjectName, ExamName, Score, MaxScore, Percentage, Grade |

### Student Classes DTOs (`Features/Portal/Queries/GetStudentClasses/`)

| DTO | Properties |
|-----|------------|
| `StudentClassDto` | TeachingAssignmentId, SubjectName, SubjectCode, TeacherFirstName, TeacherLastName, ClassRoomName, ScheduleSlots |
| `StudentClassScheduleSlot` | DayOfWeek, StartTime, EndTime, RoomName |

### Parent Dashboard DTOs (`Features/Portal/Queries/GetParentDashboard/`)

| DTO | Properties |
|-----|------------|
| `ParentDashboardDto` | Children, UnreadNotificationCount, OutstandingInvoiceCount, TotalBalanceDue |
| `ParentChildDashboardDto` | StudentId, EnrollmentId, FirstName, LastName, CurrentClass, AttendancePercentage, RecentGrades, PendingAssignments, UpcomingExamCount |
| `ParentChildRecentGrade` | SubjectName, ExamName, Score, MaxScore, Percentage, Grade |

### Parent Child Profile DTO (`Features/Portal/Queries/GetParentChildProfile/`)

| DTO | Properties |
|-----|------------|
| `ParentChildProfileDto` | StudentId, StudentCode, FirstName, LastName, DateOfBirth, CurrentClass, GradeLevelName, AcademicYearName, AttendancePercentage, TotalAssignments, GradedAssignments, LatestOverallPercentage, LatestOverallGrade |

---

## Queries Added

| Query | Folder | Response Type |
|-------|--------|---------------|
| `GetStudentDashboardQuery` | `GetStudentDashboard/` | `StudentDashboardDto` |
| `GetStudentClassesQuery` | `GetStudentClasses/` | `List<StudentClassDto>` |
| `GetStudentNotificationsQuery` | `GetStudentNotifications/` | `List<PortalNotificationDto>` |
| `GetParentDashboardQuery` | `GetParentDashboard/` | `ParentDashboardDto` |
| `GetParentChildProfileQuery` | `GetParentChildProfile/` | `ParentChildProfileDto` |
| `GetParentChildAttendanceQuery` | `GetParentChildAttendance/` | `PagedResult<PortalAttendanceRecordDto>` |
| `GetParentChildGradesQuery` | `GetParentChildGrades/` | `PagedResult<PortalExamResultDto>` |
| `GetParentChildAssignmentsQuery` | `GetParentChildAssignments/` | `PagedResult<PortalAssignmentDto>` |
| `GetParentChildReportCardsQuery` | `GetParentChildReportCards/` | `PagedResult<PortalReportCardDto>` |
| `GetParentBillingQuery` | `GetParentBilling/` | `PagedResult<PortalInvoiceDto>` |
| `GetParentNotificationsQuery` | `GetParentNotifications/` | `List<PortalNotificationDto>` |

---

## Handlers Added

Each query has a corresponding handler that:
1. Injects `IPortalReadService` and `ICurrentUserService`
2. Validates authentication (checks `UserId`)
3. Delegates to `IPortalReadService` for the actual data query
4. Returns `Result<T>` with appropriate error messages

---

## Read Service Implementations

### PortalReadService (17 methods — all real EF Core queries)

| Method | Description |
|--------|-------------|
| `GetTeacherScheduleAsync` | Joins TeachingAssignment → ClassSchedule → Subject, ClassRoom, GradeLevel, Room |
| `GetClassRoomRosterAsync` | Active enrollments with student info, paginated |
| `GetStudentScheduleAsync` | Student's class schedule via enrollment → TeachingAssignment → ClassSchedule |
| `GetStudentSummaryAsync` | Aggregated summary: class, grade level, attendance %, pending assignments |
| `GetStudentDashboardAsync` | Full dashboard: validates student ownership via `ApplicationUserId`, builds today's schedule, upcoming assignments, attendance stats, recent grades, notification count, GPA from latest report card |
| `GetStudentClassesAsync` | All teaching assignments for student's classroom with schedule slots |
| `GetStudentNotificationsAsync` | Notifications joined with batches |
| `GetParentDashboardAsync` | Validates parent via `StudentGuardian`, aggregates per-child: attendance, grades, assignments, exams; plus invoice totals from `PlatformDbContext` |
| `GetParentChildProfileAsync` | Validates ownership, returns full child profile with academic stats |
| `GetParentChildAttendanceAsync` | Validates ownership, paginated attendance with subject names |
| `GetParentChildGradesAsync` | Validates ownership, paginated exam results (filterable by term) |
| `GetParentChildAssignmentsAsync` | Validates ownership, paginated assignments with submission status |
| `GetParentChildReportCardsAsync` | Validates ownership, paginated report cards with subject breakdowns |
| `GetParentBillingAsync` | Invoices from `PlatformDbContext`, paginated |
| `GetParentNotificationsAsync` | Notifications for parent user |
| `CalculateGradeHelper` | Static helper: percentage → letter grade |

### NotificationReadService (was stub, now real)

- `GetUnreadNotificationCountAsync`: Counts `Delivered` notifications for a user
- `GetMyNotificationsAsync`: Joins Notification → NotificationBatch, projects to `NotificationDto`

### AttendanceReadService (was stub, now real)

- `GetClassAttendanceAsync`: Session + records for a class schedule + date
- `GetStudentAttendanceAsync`: Paginated attendance history for an enrollment

### ExamReadService (was stub, now real)

- `GetExamsAsync`: Exams for a term with subject names and schedule counts
- `GetReportCardsAsync`: Report cards for an enrollment with subject breakdowns
- `GetStudentExamResultsAsync`: Exam results for an enrollment (filterable by term)
- `GetClassExamResultsAsync`: All results for an exam schedule

### AssignmentReadService (was stub, now real)

- `GetClassAssignmentsAsync`: Assignments for a teaching assignment with submission counts
- `GetStudentAssignmentsAsync`: All assignments for a student's classroom with individual submission status

---

## Security & Ownership Validation

| Concern | Implementation |
|---------|----------------|
| **Tenant isolation** | All queries filter by `SchoolId` via global query filters + explicit `SchoolId` checks |
| **Student ownership** | Dashboard/Classes verify `Student.ApplicationUserId == currentUserId` |
| **Parent ownership** | All parent-child endpoints verify `StudentGuardian.ParentId == parentId && StudentGuardian.StudentId == studentId` |
| **Authorization** | Each endpoint has `[Authorize(Policy = "...")]` attribute |
| **No data leakage** | Ownership validated at service level, not just controller level |

---

## Performance Considerations

- **`AsNoTracking()`** used on all read queries
- **Projection queries** avoid loading full entity graphs
- **Lazy loading avoided** — explicit joins and separate queries
- **N+1 partially addressed** — dashboard aggregates use batch queries where possible; some per-child loops remain (acceptable for dashboard count of 1-5 children)
- **`PlatformDbContext`** used separately for billing queries (Invoice, Payment are in platform DB)

---

## Files Changed

### New Files (Application Layer — 26 files)

| File | Purpose |
|------|---------|
| `Features/Portal/Queries/Shared/PortalAssignmentDto.cs` | Shared assignment DTO |
| `Features/Portal/Queries/Shared/PortalExamResultDto.cs` | Shared exam result DTO |
| `Features/Portal/Queries/Shared/PortalReportCardDto.cs` | Shared report card DTO + subject DTO |
| `Features/Portal/Queries/Shared/PortalAttendanceSummaryDto.cs` | Attendance summary DTO |
| `Features/Portal/Queries/Shared/PortalInvoiceDto.cs` | Invoice DTO |
| `Features/Portal/Queries/Shared/PortalNotificationDto.cs` | Notification DTO |
| `Features/Portal/Queries/GetStudentDashboard/GetStudentDashboardQuery.cs` | Student dashboard query |
| `Features/Portal/Queries/GetStudentDashboard/StudentDashboardDto.cs` | Student dashboard DTOs |
| `Features/Portal/Queries/GetStudentDashboard/GetStudentDashboardQueryHandler.cs` | Student dashboard handler |
| `Features/Portal/Queries/GetStudentClasses/GetStudentClassesQuery.cs` | Student classes query |
| `Features/Portal/Queries/GetStudentClasses/StudentClassDto.cs` | Student classes DTO |
| `Features/Portal/Queries/GetStudentClasses/GetStudentClassesQueryHandler.cs` | Student classes handler |
| `Features/Portal/Queries/GetStudentNotifications/GetStudentNotificationsQuery.cs` | Student notifications query |
| `Features/Portal/Queries/GetStudentNotifications/GetStudentNotificationsQueryHandler.cs` | Student notifications handler |
| `Features/Portal/Queries/GetParentDashboard/GetParentDashboardQuery.cs` | Parent dashboard query |
| `Features/Portal/Queries/GetParentDashboard/ParentDashboardDto.cs` | Parent dashboard DTOs |
| `Features/Portal/Queries/GetParentDashboard/GetParentDashboardQueryHandler.cs` | Parent dashboard handler |
| `Features/Portal/Queries/GetParentChildProfile/GetParentChildProfileQuery.cs` | Parent child profile query |
| `Features/Portal/Queries/GetParentChildProfile/ParentChildProfileDto.cs` | Parent child profile DTO |
| `Features/Portal/Queries/GetParentChildProfile/GetParentChildProfileQueryHandler.cs` | Parent child profile handler |
| `Features/Portal/Queries/GetParentChildAttendance/GetParentChildAttendanceQuery.cs` | Parent child attendance query + DTO |
| `Features/Portal/Queries/GetParentChildAttendance/GetParentChildAttendanceQueryHandler.cs` | Parent child attendance handler |
| `Features/Portal/Queries/GetParentChildGrades/GetParentChildGradesQuery.cs` | Parent child grades query |
| `Features/Portal/Queries/GetParentChildGrades/GetParentChildGradesQueryHandler.cs` | Parent child grades handler |
| `Features/Portal/Queries/GetParentChildAssignments/GetParentChildAssignmentsQuery.cs` | Parent child assignments query |
| `Features/Portal/Queries/GetParentChildAssignments/GetParentChildAssignmentsQueryHandler.cs` | Parent child assignments handler |
| `Features/Portal/Queries/GetParentChildReportCards/GetParentChildReportCardsQuery.cs` | Parent child report cards query |
| `Features/Portal/Queries/GetParentChildReportCards/GetParentChildReportCardsQueryHandler.cs` | Parent child report cards handler |
| `Features/Portal/Queries/GetParentBilling/GetParentBillingQuery.cs` | Parent billing query |
| `Features/Portal/Queries/GetParentBilling/GetParentBillingQueryHandler.cs` | Parent billing handler |
| `Features/Portal/Queries/GetParentNotifications/GetParentNotificationsQuery.cs` | Parent notifications query |
| `Features/Portal/Queries/GetParentNotifications/GetParentNotificationsQueryHandler.cs` | Parent notifications handler |

### Modified Files (5 files)

| File | Change |
|------|--------|
| `Application/Common/Interfaces/Services/IPortalReadService.cs` | Expanded from 4 to 17 methods |
| `Infrastructure/Persistence/Services/PortalReadService.cs` | Full rewrite — all 17 methods with real EF Core queries |
| `Infrastructure/Persistence/Services/NotificationReadService.cs` | Stub → real queries |
| `Infrastructure/Persistence/Services/AttendanceReadService.cs` | Stub → real queries |
| `Infrastructure/Persistence/Services/ExamReadService.cs` | Stub → real queries |
| `Infrastructure/Persistence/Services/AssignmentReadService.cs` | Stub → real queries |
| `Api/Controllers/PortalController.cs` | Expanded from 4 to 20 endpoints |

---

## Build Verification

| Build | Result |
|-------|--------|
| Backend (`dotnet build`) | ✅ Build succeeded. 0 Error(s), 2 Warning(s) |

Warnings are pre-existing (NU1903 NuGet vulnerability).

---

## Remaining Portal Work

| Item | Phase |
|------|-------|
| Frontend student portal pages | Phase 2 frontend |
| Frontend parent portal pages | Phase 2 frontend |
| Teacher dashboard real data (endpoint) | Phase 3 |
| SignalR real-time notifications | Phase 11 |
| Student self-service (password reset, profile edit) | Phase 6 |
| Parent billing payment flow | Phase 10 |
| Email/push notification delivery | Phase 6 |
| Advanced filtering on attendance/grades | Future enhancement |
