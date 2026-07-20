# Phase 5: Attendance Module

## Summary

Enhanced the attendance backend with date range/status filters, a Lock endpoint,
and a statistics summary endpoint that wires up the previously unused `StudentAttendanceSummaryResponse`
DTO. Build passes with 0 errors.

## What Was Implemented

### New Files (4)

| File | Purpose |
|------|---------|
| `Commands/LockAttendanceSession/LockAttendanceSessionCommand.cs` | Command to lock an attendance session |
| `Commands/LockAttendanceSession/LockAttendanceSessionCommandHandler.cs` | Handler — validates ownership, calls `session.Lock()` |
| `Queries/GetClassAttendance/GetStudentAttendanceSummaryQuery.cs` | Query for attendance statistics |
| `Queries/GetClassAttendance/GetStudentAttendanceSummaryQueryHandler.cs` | Handler — delegates to `IAttendanceReadService` |

### Modified Files (7)

| File | Change |
|------|--------|
| `Term.cs` (Domain) | Added `Update()` method (from Phase 4) |
| `GetStudentAttendanceQuery.cs` | Added `DateFrom`, `DateTo`, `Status` filter parameters |
| `GetStudentAttendanceQueryHandler.cs` | Passes new filters to read service |
| `IAttendanceReadService.cs` | Updated signature with filters + added `GetStudentAttendanceSummaryAsync` |
| `AttendanceReadService.cs` | Full rewrite — academic year filter now works, date range/status filters, summary computation |
| `IAttendanceSessionRepository.cs` | Added `SaveChangesAsync` |
| `AttendanceSessionRepository.cs` | Implemented `SaveChangesAsync` |
| `AttendanceController.cs` | Added 2 new endpoints, updated student attendance with filter params |

## New/Updated API Endpoints

### POST /api/attendance/sessions/{attendanceSessionId}/lock

Locks an attendance session to prevent further modifications.

```
Query: schoolId (required)
Policy: Attendance.Record
Response: Result (success/failure)
```

### GET /api/attendance/students/{enrollmentId}/summary

Returns attendance statistics: total/present/absent/late days, percentage, and full record list.

```
Query: schoolId (required)
Policy: Attendance.ReadOwn
Response: StudentAttendanceSummaryResponse
```

### GET /api/attendance/students/{enrollmentId} (updated)

Now supports date range, status, and academic year filters.

```
Query: schoolId, academicYearId?, dateFrom?, dateTo?, status?, page, pageSize
Policy: Attendance.ReadOwn
Response: PagedResult<StudentAttendanceSummaryDto>
```

## Filter Implementation

| Filter | Parameter | How it works |
|--------|-----------|-------------|
| Academic Year | `academicYearId` | Looks up all Terms in the year, filters attendance by those term IDs |
| Date From | `dateFrom` | `Session.Date >= dateFrom` |
| Date To | `dateTo` | `Session.Date <= dateTo` |
| Status | `status` | `Record.Status == status` (Present/Absent/Late/Excused) |

## Build Status

```
Build succeeded.
0 Error(s)
1 Warning(s)  (existing NU1903 — Microsoft.OpenApi vulnerability, pre-existing)
```
