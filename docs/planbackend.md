# Backend Plan — Multi-Tenant School Management SaaS
> **Stack**: ASP.NET Core 9 · Clean Architecture · CQRS (MediatR) · EF Core · SQL Server · ASP.NET Identity

---

## Architecture Overview

```
Api  ──►  Application (CQRS)  ──►  Domain
                 │
          Infrastructure
      (EF Core · Identity · JWT)
```

| Layer | Project | Role |
|---|---|---|
| Presentation | `Api` | Controllers, Middleware, Swagger |
| Application | `Application` | Commands/Queries (MediatR), Behaviors, Interfaces |
| Domain | `Domain` | Entities, Enums, Aggregates, Domain Events |
| Infrastructure | `Infrastructure` | EF Core, Repositories, Read Services, JWT, Identity |

---

## ✅ What Is Done

### Domain Layer
| Area | Entities |
|---|---|
| **Tenancy** | `School`, `Campus`, `RefreshToken`, `UserSchoolMembership` |
| **Academics** | `AcademicYear`, `Term`, `GradeLevel`, `ClassRoom`, `Room`, `EducationStage` |
| **People** | `Student`, `Teacher`, `Parent`, `StudentGuardian`, `SchoolAdminProfile` |
| **Enrollment** | `StudentEnrollment`, `TeachingAssignment`, `ClassSchedule` |
| **Common** | `AuditableEntity`, `Entity`, `TenantEntity`, `ValueObject`, `BaseDomainEvent`, `IAggregateRoot`, `IDomainEvent` |
| **Enums** | `AcademicYearStatus`, `DayOfWeekEnum`, `EmploymentStatus`, `EnrollmentStatus`, `GuardianRelationshipType` |

### Application Layer — CQRS Features

#### Identity
| Feature | Type | Status |
|---|---|---|
| Login | Command | ✅ |
| Register School Admin | Command | ✅ |
| Refresh Token | Command | ✅ |

#### Schools (Platform-level)
| Feature | Type | Status |
|---|---|---|
| Create School | Command | ✅ |
| Update School | Command | ✅ |
| Get All Schools | Query | ✅ |
| Get School By ID | Query | ✅ |
| Get School Dashboard | Query | ✅ |
| Get Platform Analytics | Query | ✅ |

#### Academics
| Feature | Type | Status |
|---|---|---|
| Create Academic Year | Command | ✅ |
| Update Academic Year | Command | ✅ |
| Create Term | Command | ✅ |
| Create Classroom | Command | ✅ |
| Update Classroom | Command | ✅ |
| Create Grade Level | Command | ✅ |
| Update Grade Level | Command | ✅ |
| Create Room | Command | ✅ |
| Update Room | Command | ✅ |
| Get Academic Years | Query | ✅ |
| Get Classrooms | Query | ✅ |
| Get Grade Levels | Query | ✅ |
| Get Rooms | Query | ✅ |

#### People
| Feature | Type | Status |
|---|---|---|
| Create Student | Command | ✅ |
| Update Student | Command | ✅ |
| Create Teacher | Command | ✅ |
| Update Teacher | Command | ✅ |
| Create Parent | Command | ✅ |
| Update Parent | Command | ✅ |
| Link Student ↔ Guardian | Command | ✅ |
| Get Students (paged) | Query | ✅ |
| Get Student By ID | Query | ✅ |
| Get Teachers (paged) | Query | ✅ |
| Get Teacher By ID | Query | ✅ |
| Get Parents (paged) | Query | ✅ |
| Get Parent By ID | Query | ✅ |
| Get My Profile (Student) | Query | ✅ |
| Get My Children (Parent) | Query | ✅ |
| Get My Classes (Teacher) | Query | ✅ |

#### Enrollment
| Feature | Type | Status |
|---|---|---|
| Enroll Student in Classroom | Command | ✅ |
| Assign Teacher to Classroom | Command | ✅ |

### Pipeline Behaviors
| Behavior | Status |
|---|---|
| `ValidationBehavior` (FluentValidation) | ✅ |
| `LoggingBehavior` | ✅ |
| `AppTransactionBehavior` | ✅ |
| `PlatformTransactionBehavior` | ✅ |
| `TenantAuthorizationBehavior` | ✅ |

### Infrastructure
| Area | Status |
|---|---|
| `ApplicationDbContext` (tenant data) | ✅ |
| `PlatformDbContext` (identity + schools) | ✅ |
| Dual-DB separation (Platform + App) | ✅ |
| EF Audit Interceptor (`AuditSaveChangesInterceptor`) | ✅ |
| 13 Repository implementations | ✅ |
| 5 Read Services (Student, Teacher, Parent, School, Academic) | ✅ |
| `JwtTokenService` (Access + Refresh tokens) | ✅ |
| `IdentityService` (Register/Login/Permissions) | ✅ |
| `TenantContext` + `CurrentUserService` | ✅ |
| `PermissionProvider` | ✅ |
| EF Core Migrations (Platform + AppDb) | ✅ |
| `DataSeeder` (Platform Admin + seed data) | ✅ |

### API Layer
| Area | Status |
|---|---|
| `AuthController` (Login, Refresh, Register Admin) | ✅ |
| `SchoolsController` (CRUD + Dashboard + Analytics) | ✅ |
| `AcademicsController` (AcademicYears, Terms, Classrooms, GradeLevels, Rooms) | ✅ |
| `PeopleController` (Students, Teachers, Parents, Me endpoints) | ✅ |
| `EnrollmentController` (Enroll Student, Assign Teacher) | ✅ |
| Swagger / OpenAPI with JWT Bearer | ✅ |
| CORS (Development + Production policies) | ✅ |
| JWT Authentication + Permission-based Authorization (25 policies) | ✅ |
| `ExceptionHandlingMiddleware` | ✅ |
| Global `Result<T>` + `PagedResult<T>` pattern | ✅ |

---

## ❌ What Is Missing

### 1. Attendance Management
> **Priority: HIGH** — Core school operation

| Missing | Description |
|---|---|
| `Attendance` domain entity | Date, StudentEnrollmentId, Status (Present/Absent/Late/Excused), Notes |
| `AttendanceSession` entity | Group attendance records per class/date |
| `CreateAttendanceSession` command | Bulk record attendance for a classroom/term/date |
| `UpdateAttendance` command | Edit individual attendance record |
| `GetAttendanceSummary` query | Per student attendance stats (%) |
| `GetAttendanceByClassroom` query | Daily class roster with status |
| `AttendanceController` | REST endpoints |
| EF Configuration + Migration | Persistence setup |

---

### 2. Grading & Assessment
> **Priority: HIGH** — Core academic feature

| Missing | Description |
|---|---|
| `Subject` entity | Subject per grade level (Math, Science…) |
| `AssignmentType` entity | Homework, Quiz, Exam, Project… |
| `Grade` entity | Student grade per assignment |
| `GradeBook` aggregate | Consolidated grades per student/term |
| `CreateSubject` command | |
| `RecordGrade` command | Teacher records a grade |
| `GetGradeBook` query | Full grade view per student/class |
| `GetStudentGrades` query | All grades for one student |
| `GradesController` | REST endpoints |

---

### 3. Announcements / Notifications
> **Priority: MEDIUM**

| Missing | Description |
|---|---|
| `Announcement` entity | Title, Body, SchoolId, TargetRole, CreatedAt, ExpiresAt |
| `CreateAnnouncement` command | |
| `GetAnnouncements` query | Role-filtered list |
| `AnnouncementsController` | REST endpoints |
| Email/Push notification service | Integration with SendGrid / Firebase |

---

### 4. Fees & Finance
> **Priority: MEDIUM**

| Missing | Description |
|---|---|
| `FeeStructure` entity | Fee type per academic year / grade level |
| `StudentFeeRecord` entity | Invoice per student |
| `Payment` entity | Payment receipt |
| `CreateFeeStructure` command | |
| `RecordPayment` command | |
| `GetFeeStatement` query | Student balance / payment history |
| `FinanceController` | REST endpoints |

---

### 5. Timetable / Scheduling
> **Priority: MEDIUM**

> `ClassSchedule` entity exists but has no CRUD operations or queries.

| Missing | Description |
|---|---|
| `GetClassSchedule` query | Per classroom or teacher timetable |
| `UpdateClassSchedule` command | Modify schedule slot |
| `DeleteClassSchedule` command | Remove a schedule entry |
| Conflict-detection logic | Prevent double-booking of rooms/teachers |
| `ScheduleController` (or extend Enrollment) | REST endpoints |

---

### 6. Document Management
> **Priority: LOW**

| Missing | Description |
|---|---|
| `Document` entity | File name, path, type, uploaded by, linked entity |
| File upload service | Azure Blob Storage / local disk |
| `UploadDocument` command | |
| `GetDocuments` query | |
| `DocumentsController` | REST endpoints |

---

### 7. Reports
> **Priority: LOW**

| Missing | Description |
|---|---|
| Attendance Report (PDF/Excel) | Per student or per class |
| Grade Report / Transcript | Per student per term |
| School Performance Report | Aggregate analytics |
| Report generation service | Use FastReport / RDLC / QuestPDF |

---

### 8. Backend Gaps & Technical Debt

| Item | Status | Priority |
|---|---|---|
| `DeleteAcademicYear` command | ❌ Missing | HIGH |
| `DeleteClassroom` command | ❌ Missing | HIGH |
| `DeleteGradeLevel` command | ❌ Missing | HIGH |
| `DeleteStudent/Teacher/Parent` commands | ❌ Missing | HIGH |
| `DeleteSchool` command | ❌ Missing | MEDIUM |
| `GetTerms` query (list terms of an AY) | ❌ Missing | HIGH |
| `UpdateTerm` command | ❌ Missing | HIGH |
| `GetStudentEnrollments` query | ❌ Missing | MEDIUM |
| `GetTeachingAssignments` query | ❌ Missing | MEDIUM |
| `Campus` CRUD (Create/Update/Get) | ❌ Missing | MEDIUM |
| `EducationStage` CRUD | ❌ Missing | MEDIUM |
| Unit Tests (only placeholder file) | ❌ Empty | HIGH |
| Integration Tests (only placeholder file) | ❌ Empty | HIGH |
| API versioning (`/api/v1/...`) | ❌ Missing | MEDIUM |
| Rate limiting | ❌ Missing | MEDIUM |
| Health check endpoint (`/health`) | ❌ Missing | LOW |
| OpenTelemetry / structured logging | ❌ Missing | LOW |
| `Acadmics` folder typo (duplicate of `Academics`) | ⚠️ Cleanup | LOW |

---

## Recommended Implementation Order

```
Phase 1 — Fill core gaps (Sprint 1-2)
  ├── Delete commands for all entities
  ├── GetTerms + UpdateTerm queries/commands
  ├── GetStudentEnrollments + GetTeachingAssignments
  ├── Campus CRUD + EducationStage CRUD
  └── API route prefix standardisation (/api/v1)

Phase 2 — Attendance (Sprint 3)
  ├── Attendance entity + EF config + migration
  ├── AttendanceSession aggregate
  ├── Commands: CreateAttendanceSession, UpdateAttendance
  ├── Queries: GetAttendanceSummary, GetAttendanceByClassroom
  └── AttendanceController

Phase 3 — Grading (Sprint 4-5)
  ├── Subject entity + CRUD
  ├── AssignmentType entity
  ├── Grade entity + GradeBook aggregate
  ├── Commands: RecordGrade, UpdateGrade
  ├── Queries: GetGradeBook, GetStudentGrades
  └── GradesController

Phase 4 — Scheduling (Sprint 6)
  ├── GetClassSchedule, UpdateClassSchedule queries/commands
  ├── Conflict detection service
  └── ScheduleController

Phase 5 — Announcements & Finance (Sprint 7-8)
  ├── Announcement entity + CRUD
  ├── FeeStructure + StudentFeeRecord + Payment entities
  └── FinanceController + AnnouncementsController

Phase 6 — Tests & Infrastructure (Ongoing)
  ├── Unit tests for all command handlers
  ├── Integration tests with WebApplicationFactory
  ├── Health checks
  └── Rate limiting + OpenTelemetry
```

---

## Current API Endpoint Summary

| Method | Route | Permission |
|---|---|---|
| POST | `/api/auth/login` | Anonymous |
| POST | `/api/auth/refresh` | Anonymous |
| POST | `/api/auth/register-school-admin` | School.Create |
| GET | `/api/schools` | School.Read |
| POST | `/api/schools` | School.Create |
| GET | `/api/schools/{id}` | School.Read |
| PUT | `/api/schools/{id}` | School.Update |
| GET | `/api/schools/{id}/dashboard` | School.Dashboard |
| GET | `/api/schools/analytics` | Platform.Analytics |
| GET | `/api/academics/academic-years` | AcademicYear.Read |
| POST | `/api/academics/academic-years` | AcademicYear.Create |
| PUT | `/api/academics/academic-years/{id}` | AcademicYear.Update |
| POST | `/api/academics/academic-years/{id}/terms` | AcademicYear.Create |
| GET | `/api/academics/classrooms` | ClassRoom.Read |
| POST | `/api/academics/classrooms` | ClassRoom.Create |
| PUT | `/api/academics/classrooms/{id}` | ClassRoom.Update |
| GET | `/api/academics/grade-levels` | GradeLevel.Read |
| POST | `/api/academics/grade-levels` | AcademicYear.Create |
| PUT | `/api/academics/grade-levels/{id}` | GradeLevel.Update |
| GET | `/api/academics/rooms` | Room.Read |
| POST | `/api/academics/rooms` | ClassRoom.Create |
| PUT | `/api/academics/rooms/{id}` | Room.Update |
| GET | `/api/people/students` | Student.Read |
| GET | `/api/people/students/{id}` | Student.Read |
| POST | `/api/people/students` | Student.Create |
| PUT | `/api/people/students/{id}` | Student.Update |
| POST | `/api/people/students/{id}/guardians` | Student.Create |
| GET | `/api/people/teachers` | Teacher.Read |
| GET | `/api/people/teachers/{id}` | Teacher.Read |
| POST | `/api/people/teachers` | Teacher.Create |
| PUT | `/api/people/teachers/{id}` | Teacher.Update |
| GET | `/api/people/parents` | Parent.Read |
| GET | `/api/people/parents/{id}` | Parent.Read |
| POST | `/api/people/parents` | Parent.Create |
| PUT | `/api/people/parents/{id}` | Parent.Update |
| GET | `/api/people/me/student-profile` | Profile.Read |
| GET | `/api/people/me/children` | Children.Read |
| GET | `/api/people/me/classes` | MyClasses.Read |
| POST | `/api/enrollment/students` | Enrollment.Create |
| POST | `/api/enrollment/teachers` | Schedule.Create |
