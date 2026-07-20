# Phase 4: Exam & Grading Module — Term Management

## Summary

Added full Term CRUD operations to the backend: standalone listing, update, and delete.
This was the primary gap identified in Phase 4's backend checklist. Build passes with 0 errors.

## What Was Implemented

### Domain Layer (1 file modified)

| File | Change |
|------|--------|
| `Domain/Entities/Academics/Term.cs` | Added `Update(name, sequence, startDate, endDate)` method with validation |

### Application Layer (8 files created)

| File | Purpose |
|------|---------|
| `Features/Academics/Queries/GetTerms/GetTermsQuery.cs` | Query to list terms for an academic year |
| `Features/Academics/Queries/GetTerms/GetTermsQueryHandler.cs` | Handler — validates school ownership |
| `Features/Academics/Queries/GetTerms/TermDto.cs` | DTO: `Id`, `Name`, `Sequence`, `StartDate`, `EndDate` |
| `Features/Academics/Commands/UpdateTerm/UpdateTermCommand.cs` | Command to update term details |
| `Features/Academics/Commands/UpdateTerm/UpdateTermCommandHandler.cs` | Handler — validates school ownership, delegates to domain |
| `Features/Academics/Commands/UpdateTerm/UpdateTermCommandValidator.cs` | FluentValidation: required fields, date ordering |
| `Features/Academics/Commands/DeleteTerm/DeleteTermCommand.cs` | Command to delete a term |
| `Features/Academics/Commands/DeleteTerm/DeleteTermCommandHandler.cs` | Handler — validates school ownership, removes via repository |

### Repository Layer (2 files modified)

| File | Change |
|------|--------|
| `Application/Common/Interfaces/Repositories/IAcademicYearRepository.cs` | Added `UpdateTermAsync`, `DeleteTermAsync`, `SaveChangesAsync` |
| `Infrastructure/Persistence/Repositories/AcademicYearRepository.cs` | Implemented `UpdateTermAsync`, `DeleteTermAsync`, `SaveChangesAsync` |

### API Layer (1 file modified)

| File | Change |
|------|--------|
| `Api/Controllers/AcademicsController.cs` | Added 3 endpoints: `GET`, `PUT`, `DELETE` for terms |

## New API Endpoints

### GET /api/academics/academic-years/{academicYearId}/terms

Returns all terms for a specific academic year, ordered by sequence.

```
Query: schoolId (required)
Policy: AcademicYear.Read
Response: List<TermDto>
```

### PUT /api/academics/academic-years/{academicYearId}/terms/{termId}

Updates a term's name, sequence, start date, and end date.

```
Body: { schoolId, name, sequence, startDate, endDate }
Policy: AcademicYear.Update
Response: Result (success/failure)
```

### DELETE /api/academics/academic-years/{academicYearId}/terms/{termId}

Deletes a term from an academic year.

```
Query: schoolId (required)
Policy: AcademicYear.Update
Response: Result (success/failure)
```

## Existing Exam Endpoints (Already Implemented)

The ExamsController already had comprehensive CRUD operations:

| Endpoint | Status |
|----------|:---:|
| `GET /exams` (with termId, pagination) | Exists |
| `POST /exams` | Exists |
| `POST /exams/{examId}/schedules` | Exists |
| `POST /exams/{examId}/results` | Exists |
| `PATCH /exams/{examId}/lock` | Exists |
| `GET /exams/schedules/{id}/results` | Exists |
| `GET /exams/students/{id}/results` (with termId, pagination) | Exists |
| `GET /exams/students/{id}/report-cards` | Exists |
| `POST /exams/students/{id}/report-cards` | Exists |

## Build Status

```
Build succeeded.
0 Error(s)
1 Warning(s)  (existing NU1903 — Microsoft.OpenApi vulnerability, pre-existing)
```
