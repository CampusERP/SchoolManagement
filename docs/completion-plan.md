# School Management SaaS — Senior Architect Completion Plan

> **Date:** 2026-07-20
> **Role:** Senior Architect / CTO
> **Mode:** Read-only analysis + actionable completion roadmap
> **Status:** No code was modified

---

## 1. Business Understanding

### 1.1 What This System Is

A **multi-tenant School Management ERP (SaaS)** — one platform serving many independent schools. Each school gets isolated academic, people, attendance, exam, billing, and notification data. A central Platform Admin operates the business: onboarding schools, managing subscriptions, cross-tenant analytics.

### 1.2 Target Users

| Role | Persona | Scope | Frontend Status |
|---|---|---|---|
| Platform Admin | SaaS operator | All schools (cross-tenant) | Partial (6 pages, mock data) |
| School Admin | Principal / back-office | One school | Partial (20+ pages, some mock) |
| Teacher | Educator | Their classes in one school | Mock only (100% fake data) |
| Student | Learner | Their own data | **Not implemented** |
| Parent | Guardian | Their children's data | **Not implemented** |

### 1.3 Core Business Workflows

1. **Onboarding:** Platform Admin creates School → registers School Admin → School Admin populates academics/people
2. **Academic Setup:** Education stages → grade levels → academic years + terms → classrooms + rooms → subjects
3. **People Management:** Create students/teachers/parents (optionally with login accounts)
4. **Enrollment:** Enroll students into classrooms; assign teachers with schedule slots
5. **Daily Operations:** Record attendance; create/submit/grade assignments; create exams; record results; generate report cards
6. **Billing:** Subscription plans → school subscriptions → invoices → payments
7. **Notifications:** Device tokens → notification batches via outbox
8. **Self-Service:** Students view schedule/grades/assignments; Parents monitor children

---

## 2. Architecture

### 2.1 Technology Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 10, C# |
| CQRS | MediatR 14.1.0 |
| Validation | FluentValidation 12.1.1 |
| ORM | EF Core 10 (SQL Server) |
| Identity | ASP.NET Core Identity |
| Auth | JWT Bearer (access + refresh tokens) |
| Frontend | React 18, TypeScript 5.8, Vite 6 |
| UI | Ant Design 5, Tailwind 4, Radix UI |
| State | TanStack Query 5 (server), Zustand 5 (client) |
| Forms | React Hook Form + Zod |
| Routing | React Router 7 |

### 2.2 Architecture Pattern

**Clean Architecture + CQRS:**
```
Api → Application → Domain
Infrastructure → Application (inversion)
```

- Two SQL Server databases: `PlatformDb` (Identity/Billing/Outbox) + `ApplicationDb` (tenant data)
- Multi-tenancy via `TenantEntity` base class + EF global query filters on `SchoolId`
- Pipeline: Logging → Validation → TenantAuth → AppTransaction → PlatformTransaction
- Outbox pattern for reliable side-effects (login linking, notifications)

### 2.3 Key Architectural Rules (Do Not Break)

1. **Dependency direction:** `Api → Application → Domain`; `Infrastructure → Application`
2. **CQRS discipline:** Commands return `Result<T>` through transaction behaviors; Queries use read services
3. **Tenant isolation is non-negotiable:** Every tenant entity MUST derive from `TenantEntity` with `SchoolId`
4. **Tenant context is source of truth:** Handlers use `ITenantContext.SchoolId`, not request body values
5. **Outbox for cross-context side effects:** Never call delivery infrastructure directly from handlers
6. **Two databases:** Identity/tenancy/billing/outbox in PlatformDb; operational data in ApplicationDb
7. **Authorization is centralized:** Policies in `Program.cs` + `PermissionProvider` role matrix
8. **Soft delete:** Use `IsDeleted`/`AuditableEntity` for tenant data

### 2.4 Where to Add New Features

| Change Type | Steps |
|---|---|
| New API endpoint | Controller action → `Features/<Module>/Commands|Queries/<UseCase>/` with Command/Query + Handler + Validator + DTOs. Register permission in `Program.cs` and `PermissionProvider`. |
| New domain entity | `Domain/Entities/<Context>/` → derive from `TenantEntity`/`AuditableEntity` → EF Configuration in `Infrastructure/Persistence/Configurations/` → correct DbContext → repository + DI registration → migration |
| New frontend page | `src/pages/<area>/` → feature `api.ts`+`hooks.ts` in `src/features/<module>/` → route in `src/router/routes.tsx` with guard → permissions in `authStore` |

---

## 3. Database

### 3.1 Two-Database Design

| Database | DbContext | Purpose | Entities |
|---|---|---|---|
| PlatformDb | `PlatformDbContext : IdentityDbContext` | Cross-tenant | School, Campus, UserSchoolMembership, RefreshToken, SubscriptionPlan, Subscription, Invoice, Payment, OutboxMessage |
| ApplicationDb | `ApplicationDbContext : DbContext` | Tenant-scoped | All 31 operational entities (People, Academics, Enrollment, Attendance, Assignments, Exams, Notifications, Documents) |

### 3.2 Multi-Tenancy Strategy

**Shared database, shared schema, discriminator column (`SchoolId`).** Enforced by:
1. `TenantEntity` base class adds `SchoolId`
2. EF global query filters: `e => !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId)`
3. `TenantAuthorizationBehavior` validates request `SchoolId` matches JWT context
4. `ITenantContext` resolved from JWT claims on every request

### 3.3 Critical Database Issues

| Issue | Severity | Description |
|---|---|---|
| **Missing SchoolId on ExamResult, AttendanceRecord, AssignmentSubmission** | HIGH | These entities extend `AuditableEntity` (no `SchoolId`). No global query filter applies. Tenant isolation is BROKEN — a school admin could potentially see another school's data through these entities if queries bypass the filter. |
| **345 pending migrations** | MEDIUM | Migration files in `Infrastructure/Persistence/Migrations/` are empty/placeholder. Migrations need to be regenerated from snapshot. |
| **Term entity lacks SchoolId** | MEDIUM | `Term` extends `AuditableEntity`, not `TenantEntity`. Terms are scoped to `AcademicYear` (which has `SchoolId`), but direct term queries lack tenant isolation. |
| **EducationStage/Subject not tenant-scoped** | LOW | Global lookup tables (intentional for cross-school reporting). Platform Admin manages them. |

### 3.4 Entity Inventory (40 Entities)

**Tenancy (4):** School, Campus, UserSchoolMembership, RefreshToken
**People (5):** Student, Teacher, Parent, StudentGuardian, SchoolAdminProfile
**Academics (8):** EducationStage, GradeLevel, AcademicYear, Term, ClassRoom, Room, Subject, CurriculumSubject
**Enrollment (3):** StudentEnrollment, TeachingAssignment, ClassSchedule
**Attendance (2):** AttendanceSession, AttendanceRecord
**Assignments (3):** Assignment, AssignmentSubmission, AssignmentSubmissionDocument
**Exams (5):** Exam, ExamSchedule, ExamResult, ReportCard, ReportCardSubjectResult
**Billing (4):** SubscriptionPlan, Subscription, Invoice, Payment
**Notifications (4):** NotificationTemplate, NotificationBatch, Notification, DeviceToken
**Documents (1):** Document
**Outbox (1):** OutboxMessage

### 3.5 CRUD Coverage Matrix

| Entity | Create | Read | Update | Delete | Frontend |
|--------|:------:|:----:|:------:|:------:|----------|
| EducationStage | Backend+FE | Backend+FE | Backend+FE | Backend+FE | Full |
| School | Backend+FE | Backend+FE | Backend+FE | MISSING (suspend only) | Partial |
| Student | Backend+FE | Backend+FE | Backend+FE | MISSING (withdraw only) | Partial |
| Teacher | Backend+FE | Backend+FE | Backend+FE | MISSING (terminate only) | Partial |
| Parent | Backend+FE | Backend+FE | Backend+FE | MISSING | Partial |
| AcademicYear | Backend+FE | Backend+FE | Backend only | MISSING | Partial |
| Term | Backend+FE | Backend+FE | MISSING | MISSING | Partial |
| GradeLevel | Backend+FE | Backend+FE | Backend+FE | MISSING | Partial |
| ClassRoom | Backend+FE | Backend+FE | Backend+FE | MISSING | Partial |
| Room | Backend+FE | Backend+FE | Backend+FE | MISSING | Partial |
| Subject | Backend only | Dropdown only | MISSING | MISSING | None |
| CurriculumSubject | Backend only | MISSING | MISSING | MISSING | None |
| StudentEnrollment | Backend+FE | Backend+FE | MISSING | MISSING | Partial |
| TeachingAssignment | Backend+FE | Backend+FE | MISSING | MISSING | Partial |
| ClassSchedule | Via assignment | Via teacher detail | MISSING | MISSING | None |
| AttendanceSession | Backend only | Backend only | MISSING | MISSING | None |
| AttendanceRecord | Backend only | Backend only | MISSING | MISSING | None |
| Assignment | Backend only | Backend only | MISSING | MISSING | None |
| AssignmentSubmission | Backend only | MISSING | MISSING | MISSING | None |
| Exam | Backend only | Backend only | MISSING | MISSING | None |
| ExamResult | Backend only | Backend only | MISSING | MISSING | None |
| ExamSchedule | Backend only | MISSING | MISSING | MISSING | None |
| ReportCard | Backend only | Backend only | MISSING | MISSING | None |
| ReportCardSubjectResult | Backend only | MISSING | MISSING | MISSING | None |
| SubscriptionPlan | Backend only | Backend only | MISSING | MISSING | None |
| Subscription | Backend only | Backend only | Backend only | MISSING | None |
| Invoice | Backend only | Backend only | MISSING | MISSING | None |
| Payment | Backend only | MISSING | MISSING | MISSING | None |
| Notification* (4 entities) | Backend only | Backend only | Backend only | MISSING | None |
| Document | MISSING | MISSING | MISSING | MISSING | None |
| OutboxMessage | Auto-managed | Infrastructure | N/A | N/A | None |

**Summary:** Only **EducationStage** has complete CRUD with frontend. Zero entities have Delete except EducationStage.

---

## 4. Features — Completed vs Missing

### 4.1 Backend Features — What Works

| Module | Commands | Queries | Notes |
|--------|----------|---------|-------|
| Auth | Login, RegisterSchoolAdmin, RefreshToken | Memberships, SwitchSchool | Fully functional |
| Schools | Create, Update | GetAll, GetById, Dashboard, Analytics | Dashboard returns basic counts only |
| Academics | CreateAcademicYear, UpdateAcademicYear, CreateTerm, CreateClassroom, UpdateClassroom, CreateGradeLevel, UpdateGradeLevel, CreateRoom, UpdateRoom | GetAcademicYears, GetClassrooms, GetGradeLevels, GetRooms | Missing: GetTerms, UpdateTerm, Delete operations |
| People | CreateStudent, UpdateStudent, CreateTeacher, UpdateTeacher, CreateParent, UpdateParent, LinkGuardian | GetStudents, GetStudentById, GetTeachers, GetTeacherById, GetParents, GetParentById, GetMyProfile, GetMyChildren, GetMyClasses | Missing: Delete operations |
| Enrollment | EnrollStudent, AssignTeacher | (via People queries) | Missing: Withdraw, Unassign |
| Attendance | RecordAttendance (bulk) | (via Portal stubs) | **Missing frontend entirely** |
| Assignments | CreateAssignment, SubmitAssignment, GradeAssignment | GetAssignment, GetAssignments, GetSubmissions | **Missing frontend entirely** |
| Exams | CreateExam, RecordExamResults, GenerateReportCard | GetExam, GetExams, GetClassExamResults | **Missing frontend entirely** |
| Billing | CreatePlan, CreateSubscription, CreateInvoice, RecordPayment, GetPlans, GetSubscriptions, GetInvoices, GetPayments | All exist | **Missing frontend entirely** |
| Notifications | SendNotification | GetNotifications, GetNotificationBatches | **Missing frontend entirely** |
| Portal | — | All 4 methods are **stubs** returning empty data | Needs real implementation |

### 4.2 Frontend Features — What Exists

| Area | Pages | Status |
|------|-------|--------|
| Auth | LoginPage | Working |
| Platform Admin | PlatformDashboard, SchoolsList, SchoolDetail, CreateSchool, EditSchool, RegisterAdmin | Working (some mock data) |
| School Admin | SchoolDashboard, AcademicYears, Classrooms, GradeLevels, Rooms, StudentsList, StudentDetail, TeachersList, TeacherDetail, ParentsList, ParentDetail, EnrollmentPage, AssignTeacherPage | Working (some mock data, dead links) |
| Teacher Portal | Dashboard (100% mock), Classes (placeholder) | **Non-functional** |
| Student Portal | NONE | **Not implemented** |
| Parent Portal | NONE | **Not implemented** |

### 4.3 Complete Feature Modules Missing (Backend exists, Frontend doesn't)

| Module | Backend Endpoints | Frontend Pages |
|--------|-------------------|----------------|
| Exams | CRUD Exams, Schedules, Results, Report Cards | NONE |
| Assignments | Create, Submit, Grade | NONE |
| Attendance | Record (bulk), Class Sheet, Student History | NONE |
| Notifications | List, Send, Mark Read, Device Token | NONE |
| Billing | Plans, Subscriptions, Invoices, Payments | NONE |
| Documents | Entity exists, no API | NONE |
| Subjects | Get, Create, AddCurriculum | Dropdown only |
| Student Portal | Portal endpoints exist | NONE |
| Parent Portal | Portal endpoints exist | NONE |
| Teacher Schedule | Portal endpoint exists | NONE |

### 4.4 Authorization Policy Gaps

| Issue | Description |
|-------|-------------|
| `GradeLevel.Create` policy missing | Used in `AcademicsController:94` but never registered — endpoint always rejects |
| `Room.Create` policy missing | Used in `AcademicsController:115` but never registered — endpoint always rejects |
| Missing policies for Delete operations | `GradeLevel.Delete`, `Room.Delete`, `Subject.Read/Create/Update/Delete`, `Exam.Delete`, `Assignment.Delete`, `ReportCard.Read/Create`, `Campus.Read/Create/Update`, `Document.Read/Upload/Delete`, `Announcement.Read/Create/Update/Delete` |

### 4.5 Dashboard Status

| Dashboard | Data Source | Status |
|-----------|-------------|--------|
| Platform Admin | `GET /schools/analytics` + mock | Partial — counts real, activity/health mock |
| School Admin | `GET /schools/{id}/dashboard` + mock | Partial — counts real, events/announcements mock |
| Teacher | `GET /teacher/dashboard` (doesn't exist) | **100% mock data** |
| Student | N/A | **Missing — redirects to login** |
| Parent | N/A | **Missing — redirects to login** |

### 4.6 Critical Bugs

| # | Bug | Impact |
|---|-----|--------|
| B-01 | Missing `GradeLevel.Create` and `Room.Create` policies | These endpoints reject ALL users including platform_admins |
| B-02 | Teacher dashboard endpoint missing | Teacher dashboard always 100% mock |
| B-03 | School dashboard overwrites real data with mock | Users see fake data when backend works |
| B-04 | Platform dashboard overwrites real data with mock | Same issue |
| B-05 | Student/Parent redirect to `/auth/login` (infinite loop) | Students/Parents cannot access frontend |
| B-06 | PortalReadService all stubs | Teacher/Student portal returns empty data |
| B-07 | Outbox email/SMS/Push throws NotSupportedException | Only InApp notifications work |
| B-08-10 | Student/Teacher/Parent detail pages sparse | Missing email, phone, photo, schedule, grades |
| B-11 | ChartsPlaceholder everywhere | All charts show "coming soon" |
| B-12 | Teacher portal routes missing | Dead links to attendance/grades |
| B-13 | TeachersPage missing AssignedClassesCount | Data returned but not displayed |
| B-14 | Silent error swallowing in dashboards | Mock data hides backend failures |

---

## 5. Technical Problems

### 5.1 Data Architecture Issues

| # | Problem | Severity | Fix Effort |
|---|---------|----------|------------|
| L-01 | No real dashboard data source | HIGH | Medium — build proper dashboard queries |
| L-02 | Announcements entity doesn't exist | MEDIUM | Medium — new entity + CRUD |
| L-03 | Events entity doesn't exist | MEDIUM | Medium — new entity + CRUD |
| L-04 | Subject global (not tenant-scoped) | LOW | Intentional design |
| L-05 | EducationStage not tenant-scoped | LOW | Intentional design |

### 5.2 API Design Issues

| # | Problem | Severity | Fix Effort |
|---|---------|----------|------------|
| L-07 | No pagination on several list endpoints | HIGH | Low — add PaginationParams + PagedResult |
| L-08 | Withdraw vs Delete inconsistency | MEDIUM | Medium — standardize lifecycle operations |
| L-09 | No bulk operations | LOW | Medium — generic bulk handler |
| L-10 | No real-time updates (SignalR) | LOW | High — new infrastructure |

### 5.3 Frontend Architecture Issues

| # | Problem | Severity | Fix Effort |
|---|---------|----------|------------|
| L-11 | Silent error swallowing | HIGH | Low — remove mock fallbacks, show errors |
| L-12 | No error boundary | MEDIUM | Low — add React error boundary |
| L-13 | No form state persistence | LOW | Low — add draft saving |
| L-14 | Inconsistent naming (ClassRoom vs classroom) | LOW | Low — standardize |

### 5.4 Infrastructure Gaps

| Component | Status |
|-----------|--------|
| Email service | Interface exists, zero implementation |
| SMS service | None |
| Push notification service | None (only InApp) |
| Payment gateway (Stripe) | None |
| File storage | None |
| Caching (Redis) | None |
| Background jobs (Hangfire) | Outbox processor only |
| Health checks | None |
| Rate limiting | None |
| OpenTelemetry | None |
| Unit tests | Placeholder only |
| Integration tests | Placeholder only |
| CI/CD | None |
| Docker | None |
| Logging (Serilog) | None (uses built-in) |

### 5.5 Security Observations

| Observation | Risk |
|-------------|------|
| Tokens stored in localStorage via Zustand persist | XSS exposure risk |
| No visible rate-limiting on login/refresh | Brute force risk beyond Identity lockout |
| No email verification on registration | Account takeover risk |
| No 2FA support | MFA gap |

---

## 6. Development Roadmap

### Phase 1: Fix Critical Bugs (1-2 days)

**Priority: CRITICAL — Must fix before any new features**

- [ ] Register `GradeLevel.Create` and `Room.Create` policies in `Api/Program.cs`
- [ ] Remove mock data fallbacks from dashboard API calls — show real errors
- [ ] Implement `PortalReadService` methods with real EF Core queries
- [ ] Fix `ROLE_HOME` for student/parent to new dashboard routes (not `/auth/login`)
- [ ] Fix teacher portal dead links (register routes or remove links)
- [ ] Add `SchoolId` to `ExamResult`, `AttendanceRecord`, `AssignmentSubmission` for tenant isolation
- [ ] Fix `TeachersPage` missing `AssignedClassesCount` column
- [ ] Remove `ChartsPlaceholder` — add real charts or better empty states
- [ ] Add error boundaries to React app

### Phase 2: Student & Parent Portals (1-2 weeks)

**Priority: HIGH — Two of five roles completely blocked**

Backend:
- [ ] Create `GET /portal/student/dashboard` endpoint
- [ ] Create `GET /portal/parent/dashboard` endpoint
- [ ] Implement real `PortalReadService` queries

Frontend:
- [ ] Student Dashboard page (`/student`)
- [ ] Student My Classes page (`/student/classes`)
- [ ] Student Schedule page (`/student/schedule`)
- [ ] Student Grades page (`/student/grades`)
- [ ] Student Attendance page (`/student/attendance`)
- [ ] Student Profile page (`/student/profile`)
- [ ] Parent Dashboard page (`/parent`)
- [ ] Parent Children page (`/parent/children`)
- [ ] Parent Grades page (`/parent/grades`)
- [ ] Parent Attendance page (`/parent/attendance`)
- [ ] Parent Billing page (`/parent/billing`)
- [ ] Parent Profile page (`/parent/profile`)
- [ ] Routes, sidebar navigation, role guards for both portals

### Phase 3: Teacher Portal Completion (1 week)

**Priority: HIGH — Teacher dashboard is 100% mock**

Backend:
- [ ] Create `GET /teacher/dashboard` endpoint with real data

Frontend:
- [ ] Teacher Dashboard — replace all mock data with real API
- [ ] Teacher My Classes — class list with student rosters
- [ ] Teacher Class Detail — student list, attendance, grades
- [ ] Teacher Attendance — take attendance for today's classes
- [ ] Teacher Assignments — create/grade assignments
- [ ] Teacher Grades — enter exam results, view report cards
- [ ] Teacher Schedule — weekly timetable view

### Phase 4: Exam & Grading Module (1-2 weeks)

**Priority: HIGH — Core academic feature with full backend**

Backend:
- [ ] Add missing query endpoints (GetTerms, UpdateTerm)
- [ ] Add pagination to exam/result queries
- [ ] Add filters (term, class, subject)

Frontend:
- [ ] Exam Management page (school admin)
- [ ] Exam Scheduling UI
- [ ] Exam Results Entry (teacher)
- [ ] Report Card Generation/Viewing
- [ ] Student/Parent grade viewing pages
- [ ] Import/Export for exams and results

### Phase 5: Attendance Module (1 week)

**Priority: HIGH — Core daily operation**

Backend:
- [ ] Add pagination and filters to attendance queries
- [ ] Add date range, class, status filters

Frontend:
- [ ] Attendance Recording page (teacher)
- [ ] Attendance Overview page (school admin)
- [ ] Attendance History pages (student, parent)
- [ ] Attendance Reports/Analytics
- [ ] Export attendance sheets

### Phase 6: Email Services (1 week)

**Priority: MEDIUM — Required for notifications and onboarding**

Backend:
- [ ] Add MailKit NuGet package to Infrastructure
- [ ] Implement `IEmailService` with SMTP transport
- [ ] Add SMTP configuration to `appsettings.json`
- [ ] Build email templates (welcome, password reset, enrollment, grades, attendance, invoices)
- [ ] Update `DeliverNotificationBatchHandler` for Email channel
- [ ] Add email triggers for key events
- [ ] Add forgot/reset password endpoints

Frontend:
- [ ] Forgot Password page (`/auth/forgot-password`)
- [ ] Reset Password page (`/auth/reset-password`)

### Phase 7: Public Pages & School Profiles (1-2 weeks)

**Priority: MEDIUM — Marketing and school discovery**

Backend:
- [ ] Add `LogoUrl`, `Description`, `WebsiteUrl`, `Motto` to School entity
- [ ] Create `GET /public/schools/{slug}` endpoint (no auth)
- [ ] Create `GET /public/schools` directory endpoint (no auth)
- [ ] Add SEO metadata generation

Frontend:
- [ ] Landing Page (`/`) — hero, features, pricing, CTA
- [ ] School Profile Page (`/school/:slug`)
- [ ] School Directory (`/schools`)
- [ ] Contact Us Page (`/contact`)
- [ ] About Page (`/about`)
- [ ] Pricing Page (`/pricing`)
- [ ] Registration Page (`/auth/register`)

### Phase 8: Platform Enhancements (1 week)

**Priority: MEDIUM — Improve platform admin experience**

Backend:
- [ ] Build real activity feed query from audit trail
- [ ] Build system health check endpoint
- [ ] Add pagination to remaining list endpoints

Frontend:
- [ ] User Management page (`/platform/users`)
- [ ] Revenue/Billing overview
- [ ] Replace ChartsPlaceholder with real charts (add Recharts or ECharts)
- [ ] ERP Documentation pages (`/docs`)
- [ ] Import/Export UI on all list pages

### Phase 9: Remaining CRUD & Polish (1-2 weeks)

**Priority: LOW-MEDIUM — Complete the feature set**

Backend:
- [ ] Add Delete operations for remaining entities
- [ ] Add Subject management page
- [ ] Standardize lifecycle operations (withdraw/terminate/delete)

Frontend:
- [ ] Notification Center UI
- [ ] Billing Management UI (plans, subscriptions, invoices, payments)
- [ ] Assignment Management UI
- [ ] Subject Management page
- [ ] Profile/Settings page for all roles
- [ ] Form state persistence (draft saving)
- [ ] Filters + pagination on every list page

### Phase 10: Payments, Onboarding & Activation (1-2 weeks)

**Priority: MEDIUM — Business-critical for school activation**

Backend:
- [ ] Add `Inactive`/`Pending` school status (currently `Active` on creation)
- [ ] Add `PriceYearly` + `BillingCycle` + `AcademicYearId` to billing entities
- [ ] Add `PaymentMethod.Stripe` + `PaymentMethod.Other`
- [ ] Implement `IPaymentGateway` + `StripePaymentGateway`
- [ ] `POST /billing/checkout/session` (Stripe self-service)
- [ ] `POST /billing/webhooks/stripe` (confirm + activate school)
- [ ] "Other" payment flow: School Admin initiates, Platform Admin confirms
- [ ] School activation service (idempotent) via Outbox pattern
- [ ] Block/degrade login for inactive schools
- [ ] Platform Admin billing dashboard

### Phase 11: Advanced Features (Ongoing)

**Priority: LOW — Future enhancements**

- [ ] Real-time notifications (SignalR)
- [ ] File upload/download (Document entity)
- [ ] Bulk import/export for all entities
- [ ] Advanced analytics & reporting
- [ ] PDF report card generation
- [ ] Multi-language support (i18n)
- [ ] Mobile responsive optimization
- [ ] PWA support for offline attendance
- [ ] Unit tests for all command handlers
- [ ] Integration tests with WebApplicationFactory
- [ ] Health checks + rate limiting + OpenTelemetry
- [ ] Docker + CI/CD pipeline

---

## 7. Effort Summary

| Phase | Description | Estimated Effort |
|-------|-------------|-----------------|
| 1 | Fix Critical Bugs | 1-2 days |
| 2 | Student & Parent Portals | 1-2 weeks |
| 3 | Teacher Portal Completion | 1 week |
| 4 | Exam & Grading Module | 1-2 weeks |
| 5 | Attendance Module | 1 week |
| 6 | Email Services | 1 week |
| 7 | Public Pages & School Profiles | 1-2 weeks |
| 8 | Platform Enhancements | 1 week |
| 9 | Remaining CRUD & Polish | 1-2 weeks |
| 10 | Payments, Onboarding & Activation | 1-2 weeks |
| 11 | Advanced Features | Ongoing |
| **Total** | **To production-ready MVP** | **~8-12 weeks** |

---

## 8. Quick Wins (Immediate Impact, Low Effort)

1. Register missing policies in `Program.cs` (5 minutes)
2. Remove mock data fallbacks from dashboards (30 minutes)
3. Fix `ROLE_HOME` for student/parent (10 minutes)
4. Add error boundaries to React app (30 minutes)
5. Add `AssignedClassesCount` to TeachersPage table (10 minutes)
6. Fix teacher portal dead links (15 minutes)

---

*End of Completion Plan*
