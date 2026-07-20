# SchoolManagement (CampusERP) — Comprehensive Audit Report

> **Date:** 2026-07-20
> **Scope:** Full-stack audit — Backend (.NET 10, CQRS/MediatR) + Frontend (React 18, TypeScript, Ant Design)
> **Purpose:** Document all bugs, missing features, logical problems, and required feature roadmap
> **v2 addendum:** Added payment/onboarding flow (Stripe + "Other"), yearly payments per academic year, import/export, and filters/pagination requirements.

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Bugs & Defects](#2-bugs--defects)
3. [Logical Problems & Design Issues](#3-logical-problems--design-issues)
4. [Missing CRUD Operations](#4-missing-crud-operations)
5. [Missing Features — Backend](#5-missing-features--backend)
6. [Missing Features — Frontend](#6-missing-features--frontend)
7. [Dashboard Status](#7-dashboard-status)
8. [Email Services](#8-email-services)
9. [Public School Profiles](#9-public-school-profiles)
10. [Platform Home & Public Pages](#10-platform-home--public-pages)
11. [Role-Based Control Panel (Dashboards)](#11-role-based-control-panel-dashboards)
12. [Entity Inventory](#12-entity-inventory)
13. [Authorization Policy Gaps](#13-authorization-policy-gaps)
14. [Feature Roadmap](#14-feature-roadmap)
15. [Payments, Onboarding & School Activation](#15-payments-onboarding--school-activation)
16. [Import / Export](#16-import--export)
17. [Filters & Pagination](#17-filters--pagination)

---

## 1. Executive Summary

The project is a **multi-tenant School ERP** built with .NET Clean Architecture (CQRS + MediatR) on the backend and React + TypeScript on the frontend. The core domain model is well-designed with 40 entities covering tenancy, people, academics, enrollment, attendance, assignments, exams, billing, notifications, and documents.

**Current state:**
- ~30% of backend APIs have corresponding frontend UI
- All dashboards use mock/fake data (no real dashboard API exists)
- 3 out of 5 role dashboards are completely missing (Student, Parent; Teacher is 100% mock)
- Zero email implementation despite interface + entity definitions
- Zero public-facing pages (no landing page, no school profiles)
- 7+ full backend feature modules have zero frontend UI
- Only 1 out of ~40 entities has a Delete operation
- **Billing foundations exist** (`SubscriptionPlan`, `Subscription`, `Invoice`, `Payment` + `PaymentMethod`/`InvoiceStatus`/`SubscriptionStatus` enums) but **no payment gateway integration** (Stripe) and **no school-activation-on-payment flow**
- **School lifecycle is currently `Active` on creation** — requirement: new schools must be created **inactive** and only become active after a successful payment
- **No import/export or filters/pagination UI** on list screens (backend `PagedResult` exists for some queries only)

---

## 2. Bugs & Defects

### Critical

| # | Bug | Location | Description |
|---|-----|----------|-------------|
| B-01 | **Missing auth policies** | `Api/Program.cs:111` | `GradeLevel.Create` and `Room.Create` policies are used in controllers (`AcademicsController.cs:94,115`) but never registered in the `permissionPolicies` array. These endpoints reject ALL users including platform_admins. |
| B-02 | **Teacher dashboard endpoint missing** | `frontend/src/features/dashboard/api.ts:133` | `GET /teacher/dashboard` is called but no such endpoint exists in any backend controller. Teacher dashboard is always 100% mock data. |
| B-03 | **School dashboard overwrites real data with mock** | `frontend/src/features/dashboard/api.ts:109-112` | On successful API response, `recentStudents`, `upcomingEvents`, `announcements`, `attendanceSummary` are replaced with hardcoded mock data. |
| B-04 | **Platform dashboard overwrites real data with mock** | `frontend/src/features/dashboard/api.ts:86-87` | On successful API response, `recentActivity` and `systemHealth` always use mock data. `recentSchools` is always `[]`. |
| B-05 | **Students/Parents cannot access frontend** | `frontend/src/lib/constants.ts:7-8` | `ROLE_HOME` maps `student` and `parent` to `/auth/login`. After login, they are redirected back to login in an infinite loop. |

### High

| # | Bug | Location | Description |
|---|-----|----------|-------------|
| B-06 | **PortalReadService all stubs** | `Infrastructure/Persistence/Services/PortalReadService.cs:9-19` | All 4 methods return empty results. `GetTeacherScheduleAsync`, `GetStudentSummaryAsync`, `GetStudentScheduleAsync`, `GetClassRoomRosterAsync` never query the database. |
| B-07 | **Notification channels throw NotSupportedException** | `Infrastructure/Outbox/OutboxMessageHandlers.cs:62-63` | Only InApp notifications work. Email, SMS, and Push channels throw `NotSupportedException` at runtime. |
| B-08 | **Student detail page sparse** | `frontend/src/pages/school/people/StudentDetailPage.tsx` | Only shows: code, name, DOB, national ID, login status, enrollments, guardians. Missing: email, phone, photo, attendance summary, grades. |
| B-09 | **Teacher detail page sparse** | `frontend/src/pages/school/people/TeacherDetailPage.tsx` | Only shows: code, name, status, assignments. Missing: email, phone, photo, schedule, class list, attendance. |
| B-10 | **Parent detail page sparse** | `frontend/src/pages/school/people/ParentDetailPage.tsx` | Only shows: name, children list. Missing: email, phone, photo, relationship details, login status, permissions. |

### Medium

| # | Bug | Location | Description |
|---|-----|----------|-------------|
| B-11 | **No charts implementation** | `frontend/src/components/organisms/ChartsPlaceholder.tsx:17` | All charts show "Chart coming soon" placeholder. Affects SchoolDashboard and TeacherDashboard. |
| B-12 | **Teacher portal routes missing** | `frontend/src/router/routes.tsx:132` | `/teacher/classes` renders `PlaceholderPage`. Quick-action links to `/teacher/attendance` and `/teacher/grades` don't exist (catch-all redirects to login). |
| B-13 | **TeachersPage missing AssignedClassesCount column** | `frontend/src/pages/school/people/TeachersPage.tsx:67-108` | Backend returns `assignedClassesCount` in `TeacherListDto` but the table doesn't display it. |
| B-14 | **All mock data fallbacks hide real errors** | `frontend/src/features/dashboard/api.ts` | Every API call has a catch block that returns mock data silently, making it impossible for users to know the backend is down. |

---

## 3. Logical Problems & Design Issues

### 3.1 Data Architecture

| # | Problem | Description |
|---|---------|-------------|
| L-01 | **No real dashboard data source** | There is no `GET /schools/{id}/dashboard` endpoint that returns the full dashboard payload. The backend `SchoolDashboard` endpoint returns only basic counts, not the rich data the frontend expects (recent students, events, announcements, attendance summary). |
| L-02 | **Announcements entity doesn't exist** | Dashboard types reference `Announcement` objects, but there is no `Announcement` domain entity or API endpoint. Mock data is the only source. |
| L-03 | **Events entity doesn't exist** | Dashboard types reference `EventItem` objects (exams, holidays, meetings). No domain entity exists for calendar events. |
| L-04 | **No Subject entity in TenantEntity** | `Subject` is a global lookup (`Entity` base, no `SchoolId`). This is intentional for cross-school reporting but means school admins cannot create custom subjects — they must be Platform Admins. |
| L-05 | **EducationStage not tenant-scoped** | `EducationStage` extends `Entity` directly (no audit, no soft delete, no SchoolId). All education stages are global — a school cannot have its own stages. |
| L-06 | **No soft delete on EducationStage/Subject** | These are the only 2 domain entities without `AuditableEntity` base. They cannot be soft-deleted or audited. |

### 3.2 API Design

| # | Problem | Description |
|---|---------|-------------|
| L-07 | **No pagination on several list endpoints** | Some query endpoints return unbounded lists (e.g., `GetSubjectsQuery`, grade levels, rooms) without pagination support. |
| L-08 | **Withdraw vs Delete inconsistency** | Students have a `Withdraw` operation instead of Delete. Teachers have `Terminate`. Other entities have no removal mechanism at all. There is no consistent lifecycle management. |
| L-09 | **No bulk operations** | No bulk create/update/delete endpoints. Only attendance has bulk record. Import is handled via a separate import flow. |
| L-10 | **No real-time updates** | No SignalR/WebSocket hub exists. All data is polling-based via React Query. |

### 3.3 Frontend Architecture

| # | Problem | Description |
|---|---------|-------------|
| L-11 | **Silent error swallowing** | Dashboard API catch blocks return mock data instead of propagating errors. Users see fake data when the backend is down. |
| L-12 | **No error boundary** | No React error boundary component exists. Unhandled rendering errors crash the entire app. |
| L-13 | **No form state persistence** | Navigating away from a partially-filled form loses all data. No draft saving. |
| L-14 | **Inconsistent naming** | Backend uses `ClassRoom` (PascalCase compound), frontend alternates between `classRoom` and `classroom`. DTOs use `ClassRoomName` and `classroomName` on the same type. |

---

## 4. Missing CRUD Operations

### Entity-by-Entity Matrix

| Entity | Create | Read | Update | Delete | Notes |
|--------|:------:|:----:|:------:|:------:|-------|
| **School** | Backend+FE | Backend+FE | Backend+FE | MISSING | No delete (only suspend) |
| **Campus** | Backend only | Indirect | MISSING | MISSING | No frontend UI at all |
| **Student** | Backend+FE | Backend+FE | Backend+FE | MISSING | No delete, only Withdraw |
| **Teacher** | Backend+FE | Backend+FE | Backend+FE | MISSING | No delete, only Terminate |
| **Parent** | Backend+FE | Backend+FE | Backend+FE | MISSING | No delete |
| **SchoolAdminProfile** | Auto-created | MISSING | MISSING | MISSING | Created via Outbox, no UI |
| **StudentGuardian** | Backend only | Indirect | MISSING | MISSING | Linked via PeopleController, no management UI |
| **AcademicYear** | Backend+FE | Backend+FE | Backend only | MISSING | No update UI in frontend |
| **Term** | Backend+FE | Backend+FE | MISSING | MISSING | Created inline, no edit |
| **GradeLevel** | Backend+FE | Backend+FE | Backend+FE | MISSING | No delete |
| **EducationStage** | Backend+FE | Backend+FE | Backend+FE | Backend+FE | Only entity with full CRUD |
| **ClassRoom** | Backend+FE | Backend+FE | Backend+FE | MISSING | No delete |
| **Room** | Backend+FE | Backend+FE | Backend+FE | MISSING | No delete |
| **Subject** | Backend only | Dropdown only | MISSING | MISSING | No dedicated management UI |
| **CurriculumSubject** | Backend only | MISSING | MISSING | MISSING | No frontend |
| **StudentEnrollment** | Backend+FE | Backend+FE | MISSING | MISSING | No update, withdraw exists in backend only |
| **TeachingAssignment** | Backend+FE | Backend+FE | MISSING | MISSING | No update or remove |
| **ClassSchedule** | Via assignment | Via teacher detail | MISSING | MISSING | No standalone management |
| **AttendanceSession** | Backend only | Backend only | MISSING | MISSING | No frontend UI |
| **AttendanceRecord** | Backend only | Backend only | MISSING | MISSING | No frontend UI |
| **Assignment** | Backend only | Backend only | MISSING | MISSING | No frontend UI |
| **AssignmentSubmission** | Backend only | MISSING | MISSING | MISSING | No frontend UI |
| **Exam** | Backend only | Backend only | MISSING | MISSING | No frontend UI |
| **ExamResult** | Backend only | Backend only | MISSING | MISSING | No frontend UI |
| **ExamSchedule** | Backend only | Backend only | MISSING | MISSING | No frontend UI |
| **ReportCard** | Backend only | Backend only | MISSING | MISSING | No frontend UI |
| **SubscriptionPlan** | Backend only | Backend only | MISSING | MISSING | No frontend UI. Needs: price granularity (monthly vs yearly), active flag toggle UI |
| **Subscription** | Backend only | Backend only | Backend only | MISSING | Cancel only. Needs: tie to academic year (yearly), activation-on-payment |
| **Invoice** | Backend only | Backend only | MISSING | MISSING | No frontend UI. Needs: payment-status, method, Stripe linkage |
| **Payment** | Backend only | MISSING | MISSING | MISSING | No frontend UI. `PaymentMethod` enum exists (BankTransfer, CreditCard, Cash, Online) but **no Stripe / "Other" method handling** |
| **Notification** | Backend only | Backend only | Backend only | MISSING | No frontend UI |
| **NotificationBatch** | Backend only | MISSING | MISSING | MISSING | No frontend UI |
| **NotificationTemplate** | Backend only | MISSING | MISSING | MISSING | No frontend UI |
| **DeviceToken** | Backend only | MISSING | MISSING | MISSING | No frontend UI |
| **Document** | MISSING | MISSING | MISSING | MISSING | Entity exists, no upload/download API |
| **OutboxMessage** | Auto-managed | MISSING | MISSING | MISSING | Internal infrastructure |

**Summary:** Out of 40 entities, only **EducationStage** has full CRUD. Zero entities have Delete operations except EducationStage.

---

## 5. Missing Features — Backend

### 5.1 Authentication & User Management

| Feature | Status | Notes |
|---------|--------|-------|
| Login | Implemented | JWT with refresh tokens |
| Register School Admin | Implemented | Via AuthController |
| Refresh Token | Implemented | Automatic via interceptor |
| Switch School | Implemented | Multi-membership support |
| Forgot Password | MISSING | No endpoint, no flow |
| Reset Password | MISSING | No endpoint, no flow |
| Change Password | MISSING | No self-service endpoint |
| Email Verification | MISSING | No email verification on registration |
| Two-Factor Auth | MISSING | No 2FA support |

### 5.2 Dashboard APIs

| Endpoint | Status | Notes |
|----------|--------|-------|
| `GET /schools/analytics` | Partial | Returns counts only, no recent activity or system health |
| `GET /schools/{id}/dashboard` | Partial | Returns basic counts, not the rich data frontend expects |
| `GET /teacher/dashboard` | MISSING | Frontend calls it, backend doesn't have it |
| `GET /student/dashboard` | MISSING | No endpoint |
| `GET /parent/dashboard` | MISSING | No endpoint |

### 5.3 Portal APIs (All Stubs)

| Endpoint | Status | Returns |
|----------|--------|---------|
| `GET /portal/teacher/{id}/schedule` | Stub | Empty list |
| `GET /portal/classroom/{id}/roster` | Stub | Empty result |
| `GET /portal/student/{enrollmentId}/schedule` | Stub | Empty list |
| `GET /portal/student/{enrollmentId}/summary` | Stub | Null |

### 5.4 Missing API Groups

| Group | Endpoints Needed |
|-------|-----------------|
| **Announcements CRUD** | Create/Read/Update/Delete announcements for a school |
| **Calendar Events** | CRUD for school events (holidays, meetings, exams) |
| **Documents** | Upload, download, list, delete files |
| **Subject Management** | Update/Delete subjects (currently only Create + Read) |
| **Report Generation** | PDF report card generation, attendance reports |
| **Audit Log** | Query audit trail from audit fields |
| **Payments & Activation** | Stripe checkout + webhook, "Other" manual method (Platform Admin), school activation-on-payment — see [§15](#15-payments-onboarding--school-activation) |
| **Import / Export** | Bulk create + export for all entities — see [§16](#16-import--export) |
| **Filters & Pagination** | Standardized filtering + paging on all list endpoints — see [§17](#17-filters--pagination) |

---

## 6. Missing Features — Frontend

### 6.1 Complete Feature Modules Missing (Backend exists, Frontend doesn't)

| Module | Backend Endpoints | Frontend Pages |
|--------|-------------------|----------------|
| **Exams** | CRUD Exams, Schedules, Results, Report Cards | NONE |
| **Assignments** | Create, Submit, Grade | NONE |
| **Attendance** | Record (bulk), Class Sheet, Student History | NONE |
| **Notifications** | List, Send, Mark Read, Device Token | NONE |
| **Billing** | Plans, Subscriptions, Invoices, Payments | NONE |
| **Documents** | Entity exists, no API | NONE |
| **Subjects** | Get, Create, AddCurriculum | Dropdown only |
| **Student Portal** | Portal endpoints exist | NONE |
| **Parent Portal** | Portal endpoints exist | NONE |
| **Teacher Schedule** | Portal endpoint exists | NONE |

### 6.2 Individual Missing Pages

| Page | Route | Required Role |
|------|-------|---------------|
| Student Dashboard | `/student` | student |
| Parent Dashboard | `/parent` | parent |
| Teacher My Classes | `/teacher/classes` | teacher |
| Teacher Attendance | `/teacher/attendance` | teacher |
| Teacher Grades | `/teacher/grades` | teacher |
| Teacher Schedule | `/teacher/schedule` | teacher |
| Student My Classes | `/student/classes` | student |
| Student Schedule | `/student/schedule` | student |
| Student Grades | `/student/grades` | student |
| Student Attendance | `/student/attendance` | student |
| Parent Children | `/parent/children` | parent |
| Parent Grades | `/parent/grades` | parent |
| Parent Attendance | `/parent/attendance` | parent |
| Parent Billing | `/parent/billing` | parent |
| Exam Management | `/exams` | school_admin |
| Exam Results | `/exams/:id/results` | school_admin + teacher |
| Report Cards | `/exams/report-cards` | school_admin |
| Assignment Management | `/assignments` | school_admin + teacher |
| Attendance Management | `/attendance` | school_admin + teacher |
| Notification Center | `/notifications` | all |
| Billing Management | `/billing` | school_admin + platform_admin |
| Subject Management | `/academics/subjects` | school_admin |
| User Profile / Settings | `/profile` | all |
| Forgot Password | `/auth/forgot-password` | public |
| Reset Password | `/auth/reset-password` | public |

---

## 7. Dashboard Status

### Current Implementation

| Dashboard | Route | Data Source | Status |
|-----------|-------|-------------|--------|
| **Platform Admin** | `/platform` | `GET /schools/analytics` + mock | Partial — counts real, activity/health mock |
| **School Admin** | `/school` | `GET /schools/{id}/dashboard` + mock | Partial — counts real, events/announcements/mock students mock |
| **Teacher** | `/teacher` | `GET /teacher/dashboard` (doesn't exist) | 100% mock data |
| **Student** | N/A | N/A | MISSING — redirects to login |
| **Parent** | N/A | N/A | MISSING — redirects to login |

### What Each Dashboard Should Show

#### Platform Admin Dashboard
- Total/Active/Suspended schools (exists, real data)
- Total users by role (exists, real data)
- **Recent school registrations** (currently `[]`)
- **System health metrics** (currently mock)
- **Recent platform activity feed** (currently mock)
- **Revenue/billing overview** (missing entirely)
- **System alerts/notifications** (missing)

#### School Admin Dashboard
- Student/Teacher/Parent/Classroom counts (exists, real data)
- Active enrollments count (exists, real data)
- Current academic year (exists, real data)
- **Recent enrollments** (currently mock)
- **Attendance summary** (currently mock)
- **Upcoming events** (currently mock, no entity)
- **Announcements** (currently mock, no entity)
- **Enrollment trend chart** (placeholder)
- **Revenue/billing status** (missing)

#### Teacher Dashboard (needs new backend endpoint)
- My classes with student counts
- Today's schedule (real, from teaching assignments)
- Pending attendance to record
- Pending assignments to grade
- Recent student performance
- Class announcements

#### Student Dashboard (needs new backend + frontend)
- My enrolled classes
- Today's schedule
- Upcoming assignments
- Recent grades/exam results
- Attendance summary
- Announcements from teachers/school

#### Parent Dashboard (needs new backend + frontend)
- Children list with status
- Per-child: grades, attendance, schedule
- Upcoming events for children
- Announcements from school
- Billing/payment status (if applicable)
- Communication with teachers

---

## 8. Email Services

### Current State

| Component | Status |
|-----------|--------|
| `IEmailService` interface | Defined at `Application/Common/Interfaces/Services/IEmailService.cs` |
| Implementation class | **NONE** — zero classes implement `IEmailService` |
| MailKit / SendGrid NuGet | **NOT referenced** in any `.csproj` |
| SMTP configuration | **NONE** in `appsettings.json` |
| Email templates | **NONE** exist |
| Email sending code | **NONE** — no `SmtpClient`, no `MimeMessage` |
| Outbox email channel | Throws `NotSupportedException` |
| SMS service | **NONE** |
| Push notification service | **NONE** (only InApp works) |

### What Needs to Be Built

1. **Email Infrastructure**
   - Add MailKit NuGet package to Infrastructure project
   - Implement `IEmailService` with SMTP transport
   - Add SMTP configuration to `appsettings.json` (`Smtp:Host`, `Smtp:Port`, `Smtp:Username`, `Smtp:Password`, `Smtp:From`)
   - Register in DI container

2. **Email Templates (Razor or Handlebars)**
   - Welcome email (school admin registration)
   - Password reset email
   - Student enrollment confirmation
   - Assignment notification
   - Exam result notification
   - Attendance alert (absent)
   - Invoice/payment notification
   - School announcement broadcast

3. **Email Triggers**
   - On school admin registration → send welcome email
   - On student enrollment → send confirmation to parent
   - On assignment created → notify students
   - On grade posted → notify student + parent
   - On absence recorded → notify parent
   - On invoice generated → notify school admin
   - Password reset request → send reset link

4. **Outbox Integration**
   - Update `DeliverNotificationBatchHandler` to support `NotificationChannel.Email`
   - Resolve user email from `ApplicationUser` via `IIdentityService`
   - Queue email via outbox for reliable delivery

---

## 9. Public School Profiles

### Current State

**No public school profile system exists.** The `School` entity has `SubdomainCode` (e.g., "greenfield") but it is only used as metadata in platform admin CRUD. There are:
- No public routes (`/school/:slug`)
- No public-facing pages
- No SEO metadata
- No school profile rendering

### School Entity — What Exists vs What's Needed

| Field | Exists | Needed for Public Profile |
|-------|:------:|:-------------------------:|
| Name | Yes | Yes |
| SubdomainCode | Yes | Yes (as URL slug) |
| Address | Yes (nullable string) | Yes (needs structured format) |
| Phone | Yes (nullable string) | Yes |
| Email | Yes (nullable string) | Yes |
| Logo/LogoUrl | NO | Yes |
| Description/About | NO | Yes |
| Website URL | NO | Yes |
| Motto/Slogan | NO | Nice to have |
| Founded Date | NO | Nice to have |
| Principal Name | NO | Nice to have |
| Gallery/Images | NO | Nice to have |
| Social Media Links | NO | Nice to have |
| Operating Hours | NO | Nice to have |
| Campus Locations | Yes (Campus entity) | Yes |

### What Needs to Be Built

#### Backend
1. Add new properties to `School` entity: `LogoUrl`, `Description`, `WebsiteUrl`, `Motto`, `FoundedDate`, `PrincipalName`
2. Create `GET /public/schools/{subdomainCode}` endpoint (no auth required)
3. Create `GET /public/schools` endpoint for school directory (no auth required)
4. Add SEO metadata generation (meta title, description, og tags)
5. Add `SchoolProfileDto` for public data (exclude sensitive fields like subscription, billing)

#### Frontend
1. **Public School Profile Page** (`/school/:slug`)
   - School header with logo, name, motto
   - About section (description, founded date, principal)
   - Contact info (address, phone, email, website)
   - Campuses list
   - Programs/grades offered
   - Photo gallery
   - "Contact this school" form
2. **School Directory Page** (`/schools`)
   - Searchable list of all active public schools
   - Filter by location, size, programs
3. **School Application/Inquiry Form**
   - Parents can express interest / apply

---

## 10. Platform Home & Public Pages

### Current State

The only unauthenticated page is `/auth/login`. The catch-all route redirects to login. There are no marketing, informational, or public-facing pages.

### Pages to Build

#### 1. Landing Page (`/`)
- Hero section with product description
- Feature highlights (academics, attendance, grading, parent portal, billing)
- Call-to-action buttons (Register School, Login)
- Testimonials / social proof
- Pricing plans preview (from `SubscriptionPlan` entity)
- Footer with links

#### 2. Contact Us Page (`/contact`)
- Contact form (name, email, subject, message)
- Support email/phone
- Office address
- FAQ section
- Integration with notification system to notify admins

#### 3. ERP Documentation Pages (`/docs`)
- Getting Started guide
- Feature overview per role (Admin, Teacher, Student, Parent)
- API documentation (could link to Scalar/OpenAPI)
- FAQ / Troubleshooting
- Video tutorial placeholders

#### 4. About Page (`/about`)
- Product story/mission
- Team information
- Technology stack

#### 5. Pricing Page (`/pricing`)
- Dynamic listing of `SubscriptionPlan` entities
- Feature comparison table
- "Contact Sales" CTA

#### 6. Registration Page (`/auth/register`)
- School admin self-registration
- School name, subdomain selection
- Admin account creation
- Email verification flow

#### 7. Forgot/Reset Password (`/auth/forgot-password`, `/auth/reset-password`)
- Email-based password reset
- Secure token generation
- Password strength validation

---

## 11. Role-Based Control Panel (Dashboards)

### Architecture

The system defines 5 roles:
```
platform_admin → Full system access (bypasses all permission checks)
school_admin   → School-scoped, permission-based
teacher        → Teacher portal
student        → Student portal (NOT IMPLEMENTED)
parent         → Parent portal (NOT IMPLEMENTED)
```

Permission enforcement:
- **Backend:** JWT claims `is_platform_admin` and `permission` checked in `Program.cs:131-133`
- **Frontend:** `RequirePermission` component checks decoded JWT permissions
- **Frontend:** `hasPermission()` in authStore (platform_admin always returns true)

### Required Dashboard Per Role

#### Platform Admin Control Panel (`/platform`)
Already partially implemented. Needs:
- [ ] Real activity feed (backend query from audit trail)
- [ ] Real system health endpoint (check DB, API, outbox status)
- [ ] Revenue/billing overview
- [ ] User management page (`/platform/users`)
- [ ] System notifications/alerts
- [ ] Platform settings

#### School Admin Control Panel (`/school`)
Already partially implemented. Needs:
- [ ] Real dashboard data (remove all mock data)
- [ ] Announcements CRUD page
- [ ] Events/Calendar page
- [ ] Exam management section
- [ ] Assignment management section
- [ ] Attendance overview section
- [ ] Billing/subscription management
- [ ] School profile settings
- [ ] Staff directory with contact info
- [ ] Reports & analytics

#### Teacher Control Panel (`/teacher`)
Currently 100% mock. Needs new backend endpoint + full frontend:
- [ ] **Dashboard** — My classes, today's schedule, pending tasks, announcements
- [ ] **My Classes** (`/teacher/classes`) — Class list with student rosters
- [ ] **Class Detail** (`/teacher/classes/:id`) — Student list, attendance, grades
- [ ] **Attendance** (`/teacher/attendance`) — Take attendance for today's classes
- [ ] **Assignments** (`/teacher/assignments`) — Create/grade assignments
- [ ] **Grades** (`/teacher/grades`) — Enter exam results, view report cards
- [ ] **Schedule** (`/teacher/schedule`) — Weekly timetable view
- [ ] **Notifications** (`/teacher/notifications`) — View/send notifications

#### Student Control Panel (`/student`)
Entirely missing. Needs new backend endpoint + full frontend:
- [ ] **Dashboard** — Today's schedule, upcoming assignments, recent grades, attendance summary
- [ ] **My Classes** (`/student/classes`) — Enrolled classes with details
- [ ] **Schedule** (`/student/schedule`) — Weekly timetable
- [ ] **Assignments** (`/student/assignments`) — View/submit assignments
- [ ] **Grades** (`/student/grades`) — View exam results, report cards
- [ ] **Attendance** (`/student/attendance`) — View attendance history
- [ ] **Notifications** (`/student/notifications`) — View notifications
- [ ] **Profile** (`/student/profile`) — View/edit personal info

#### Parent Control Panel (`/parent`)
Entirely missing. Needs new backend endpoint + full frontend:
- [ ] **Dashboard** — Children overview, upcoming events, announcements
- [ ] **My Children** (`/parent/children`) — List of children with links
- [ ] **Child Detail** (`/parent/children/:id`) — Grades, attendance, schedule, assignments
- [ ] **Grades** (`/parent/grades`) — View children's exam results and report cards
- [ ] **Attendance** (`/parent/attendance`) — View children's attendance history
- [ ] **Notifications** (`/parent/notifications`) — View school/class notifications
- [ ] **Billing** (`/parent/billing`) — View invoices and make payments (if applicable)
- [ ] **Profile** (`/parent/profile`) — View/edit personal info, manage children links

### Sidebar Navigation Per Role

| Menu Item | platform_admin | school_admin | teacher | student | parent |
|-----------|:-:|:-:|:-:|:-:|:-:|
| Platform Dashboard | Yes | - | - | - | - |
| Schools Management | Yes | - | - | - | - |
| Billing Management | Yes | - | - | - | - |
| Platform Settings | Yes | - | - | - | - |
| School Dashboard | - | Yes | - | - | - |
| Academics | - | Yes | - | - | - |
| People Management | - | Yes | - | - | - |
| Enrollment | - | Yes | - | - | - |
| Exams & Grading | - | Yes | - | - | - |
| Assignments | - | Yes | Yes | - | - |
| Attendance Overview | - | Yes | - | - | - |
| Announcements | - | Yes | Yes | Yes | Yes |
| My Classes | - | - | Yes | Yes | - |
| Take Attendance | - | - | Yes | - | - |
| Enter Grades | - | - | Yes | - | - |
| My Schedule | - | - | Yes | Yes | - |
| My Assignments | - | - | - | Yes | - |
| My Grades | - | - | - | Yes | Yes |
| My Attendance | - | - | - | Yes | Yes |
| Children | - | - | - | - | Yes |
| My Profile | - | - | Yes | Yes | Yes |
| Notifications | Yes | Yes | Yes | Yes | Yes |

---

## 12. Entity Inventory

### 40 Domain Entities

| # | Entity | Domain Area | Base | Tenant | Audit | SoftDelete |
|---|--------|-------------|------|:------:|:-----:|:----------:|
| 1 | School | Tenancy | AuditableEntity | - | Yes | Yes |
| 2 | Campus | Tenancy | AuditableEntity | - | Yes | Yes |
| 3 | UserSchoolMembership | Tenancy | AuditableEntity | - | Yes | Yes |
| 4 | RefreshToken | Tenancy | AuditableEntity | - | Yes | Yes |
| 5 | Student | People | TenantEntity | Yes | Yes | Yes |
| 6 | Teacher | People | TenantEntity | Yes | Yes | Yes |
| 7 | Parent | People | TenantEntity | Yes | Yes | Yes |
| 8 | SchoolAdminProfile | People | TenantEntity | Yes | Yes | Yes |
| 9 | StudentGuardian | People | TenantEntity | Yes | Yes | Yes |
| 10 | AcademicYear | Academics | TenantEntity | Yes | Yes | Yes |
| 11 | Term | Academics | AuditableEntity | - | Yes | Yes |
| 12 | GradeLevel | Academics | TenantEntity | Yes | Yes | Yes |
| 13 | EducationStage | Academics | Entity | - | - | - |
| 14 | ClassRoom | Academics | TenantEntity | Yes | Yes | Yes |
| 15 | Room | Academics | TenantEntity | Yes | Yes | Yes |
| 16 | Subject | Academics | Entity | - | - | - |
| 17 | CurriculumSubject | Academics | TenantEntity | Yes | Yes | Yes |
| 18 | StudentEnrollment | Enrollment | TenantEntity | Yes | Yes | Yes |
| 19 | TeachingAssignment | Enrollment | TenantEntity | Yes | Yes | Yes |
| 20 | ClassSchedule | Enrollment | AuditableEntity | - | Yes | Yes |
| 21 | AttendanceSession | Attendance | TenantEntity | Yes | Yes | Yes |
| 22 | AttendanceRecord | Attendance | AuditableEntity | - | Yes | Yes |
| 23 | Assignment | Assignments | TenantEntity | Yes | Yes | Yes |
| 24 | AssignmentSubmission | Assignments | AuditableEntity | - | Yes | Yes |
| 25 | AssignmentSubmissionDocument | Assignments | AuditableEntity | - | Yes | Yes |
| 26 | Exam | Exams | TenantEntity | Yes | Yes | Yes |
| 27 | ExamSchedule | Exams | Entity | - | - | - |
| 28 | ExamResult | Exams | AuditableEntity | - | Yes | Yes |
| 29 | ReportCard | Exams | TenantEntity | Yes | Yes | Yes |
| 30 | ReportCardSubjectResult | Exams | AuditableEntity | - | Yes | Yes |
| 31 | SubscriptionPlan | Billing | Entity | - | - | - |
| 32 | Subscription | Billing | AuditableEntity | - | Yes | Yes |
| 33 | Invoice | Billing | AuditableEntity | - | Yes | Yes |
| 34 | Payment | Billing | AuditableEntity | - | Yes | Yes |
| 35 | NotificationTemplate | Notifications | TenantEntity | Yes | Yes | Yes |
| 36 | NotificationBatch | Notifications | TenantEntity | Yes | Yes | Yes |
| 37 | Notification | Notifications | AuditableEntity | - | Yes | Yes |
| 38 | DeviceToken | Notifications | AuditableEntity | - | Yes | Yes |
| 39 | Document | Documents | TenantEntity | Yes | Yes | Yes |
| 40 | OutboxMessage | Outbox | Entity | - | - | - |

### Entity Relationships

```
School (1) ──── (*) Campus
School (1) ──── (*) Student (via SchoolId)
School (1) ──── (*) Teacher (via SchoolId)
School (1) ──── (*) Parent (via SchoolId)
School (1) ──── (*) AcademicYear
School (1) ──── (*) GradeLevel
School (1) ──── (*) ClassRoom
School (1) ──── (*) Room
School (1) ──── (*) Subscription

AcademicYear (1) ──── (*) Term
AcademicYear (1) ──── (*) ClassRoom
GradeLevel (1) ──── (*) ClassRoom
GradeLevel (1) ──── (*) CurriculumSubject ──── Subject (N:1)

ClassRoom (*) ──── (*) StudentEnrollment ──── Student (N:1)
ClassRoom (1) ──── (*) TeachingAssignment ──── Teacher (N:1)
TeachingAssignment (1) ──── (*) ClassSchedule
TeachingAssignment (1) ──── (*) Assignment ──── (*) AssignmentSubmission

AttendanceSession (1) ──── (*) AttendanceRecord
AttendanceRecord ──── StudentEnrollment (N:1)

Exam (*) ──── (*) ExamSchedule ──── ClassRoom (N:1)
Exam (1) ──── (*) ExamResult ──── StudentEnrollment (N:1)
Exam (1) ──── (*) Subject (N:1)
Exam (1) ──── (*) Term (N:1)

ReportCard ──── StudentEnrollment (N:1)
ReportCard (1) ──── (*) ReportCardSubjectResult

Subscription ──── SubscriptionPlan (N:1)
Subscription (1) ──── (*) Invoice ──── (*) Payment

NotificationBatch (1) ──── (*) Notification
NotificationTemplate (1) ──── (*) NotificationBatch
```

---

## 13. Authorization Policy Gaps

### Registered Policies (49 total in `Api/Program.cs:108-119`)

```
AcademicYear.Read/Create/Update
ClassRoom.Read/Create/Update
GradeLevel.Read/Update          ← Missing: Create
Room.Read/Update                ← Missing: Create
EducationStage.Read/Create/Update/Delete
School.Read/Dashboard/Create/Update
Platform.Analytics
Student.Read/Create/Update
Teacher.Read/Create/Update
Parent.Read/Create/Update
Profile.Read / Children.Read / MyClasses.Read
Enrollment.Create / Schedule.Create / Schedule.Read
School.Manage
Assignment.Create/Submit/Read/ReadOwn
Attendance.Record/ReadOwn/ReadChild
Grade.Enter
Billing.Read/Manage
Exam.Read/Create/Manage
Notification.Read/Send
```

### Policies Used But Not Registered

| Policy | Used In | Effect |
|--------|---------|--------|
| `GradeLevel.Create` | `AcademicsController.cs:94` | Endpoint always rejects (policy doesn't exist) |
| `Room.Create` | `AcademicsController.cs:115` | Endpoint always rejects (policy doesn't exist) |

### Policies Missing Entirely

| Missing Policy | Needed For |
|----------------|------------|
| `GradeLevel.Delete` | If delete endpoint is added |
| `Room.Delete` | If delete endpoint is added |
| `Subject.Read` | Dedicated subject management page |
| `Subject.Create` | Subject management (currently on School.Manage) |
| `Subject.Update` | Subject management |
| `Subject.Delete` | Subject management |
| `Exam.Delete` | Exam management |
| `Assignment.Delete` | Assignment management |
| `ReportCard.Read` | Report card viewing |
| `ReportCard.Create` | Report card generation |
| `Campus.Read/Create/Update` | Campus management |
| `Document.Read/Upload/Delete` | File management |
| `Announcement.Read/Create/Update/Delete` | Announcement system |

---

## 14. Feature Roadmap

### Phase 1: Fix Critical Bugs (1-2 days)

- [ ] Register missing `GradeLevel.Create` and `Room.Create` policies in `Program.cs`
- [ ] Remove mock data from dashboard API calls (show errors or real data)
- [ ] Implement `PortalReadService` methods with real EF Core queries
- [ ] Update `ROLE_HOME` for student/parent to new dashboard routes
- [ ] Fix all placeholder pages with real content or better UX

### Phase 2: Student & Parent Portals (1-2 weeks)

- [ ] Create `GET /portal/student/dashboard` backend endpoint
- [ ] Create `GET /portal/parent/dashboard` backend endpoint
- [ ] Build Student Dashboard page
- [ ] Build Parent Dashboard page
- [ ] Build Student My Classes / Schedule / Grades / Attendance pages
- [ ] Build Parent Children / Grades / Attendance pages
- [ ] Add routes, sidebar navigation, and role guards

### Phase 3: Teacher Portal Completion (1 week)

- [ ] Create `GET /teacher/dashboard` backend endpoint
- [ ] Build Teacher My Classes page with student rosters
- [ ] Build Attendance recording page
- [ ] Build Assignment creation/grading page
- [ ] Build Grade entry page
- [ ] Build Teacher Schedule page
- [ ] Replace all mock data with real API calls

### Phase 4: Exam & Grading Module (1-2 weeks)

- [ ] Build Exam management UI (school admin)
- [ ] Build Exam scheduling UI
- [ ] Build Exam results entry UI (teacher)
- [ ] Build Report card generation/viewing UI
- [ ] Build Student/Parent grade viewing pages

### Phase 5: Attendance Module (1 week)

- [ ] Build Attendance recording page (teacher)
- [ ] Build Attendance overview page (school admin)
- [ ] Build Attendance history pages (student, parent)
- [ ] Build Attendance reports/analytics

### Phase 6: Email Services (1 week)

- [ ] Add MailKit NuGet package
- [ ] Implement `IEmailService` with SMTP
- [ ] Add SMTP configuration
- [ ] Build email templates (welcome, password reset, notifications)
- [ ] Update `DeliverNotificationBatchHandler` for Email channel
- [ ] Add email triggers for key events

### Phase 7: Public Pages & School Profiles (1-2 weeks)

- [ ] Add `LogoUrl`, `Description`, `WebsiteUrl`, `Motto` to School entity
- [ ] Create `GET /public/schools/{slug}` endpoint
- [ ] Create `GET /public/schools` directory endpoint
- [ ] Build Landing Page (`/`)
- [ ] Build School Profile Page (`/school/:slug`)
- [ ] Build School Directory (`/schools`)
- [ ] Build Contact Us Page (`/contact`)
- [ ] Build About Page (`/about`)
- [ ] Build Pricing Page (`/pricing`)

### Phase 8: Platform Enhancements (1 week)

- [ ] Build real activity feed query from audit trail
- [ ] Build system health check endpoint
- [ ] Build User Management page (`/platform/users`)
- [ ] Build Revenue/Billing overview
- [ ] Add charts library (replace ChartsPlaceholder)
- [ ] Build ERP Documentation pages (`/docs`)

### Phase 9: Remaining CRUD & Polish (1-2 weeks)

- [ ] Add Delete operations for remaining entities
- [ ] Add Subject management page
- [ ] Build Notification center UI
- [ ] Build Billing management UI
- [ ] Build Assignment management UI
- [ ] Add forgot/reset password flow
- [ ] Add profile/settings page for all roles
- [ ] Add error boundaries
- [ ] Add form state persistence

### Phase 10: Advanced Features (ongoing)

- [ ] Real-time notifications (SignalR)
- [ ] File upload/download (Document entity)
- [ ] Bulk import/export for all entities
- [ ] Advanced analytics & reporting
- [ ] PDF report card generation
- [ ] Multi-language support (i18n)
- [ ] Mobile responsive optimization
- [ ] PWA support for teacher/student offline attendance

---

## 15. Payments, Onboarding & School Activation

### 15.1 Current Billing State (Confirmed)

| Component | Status |
|-----------|--------|
| `SubscriptionPlan` entity | Exists — `Name`, `PriceMonthly`, `MaxStudents`, `MaxTeachers`, `HasParentPortal`, `HasExamModule`, `HasAnalytics`, `IsActive`. **Only monthly price** (`PriceMonthly`); no yearly price field. |
| `Subscription` entity | Exists — `SchoolId`, `SubscriptionPlanId`, `Status` (Active/Suspended/Cancelled/Expired), `StartDate`, `EndDate`, `AutoRenew`. No link to `AcademicYear`, no payment-method flag. |
| `Invoice` entity | Exists — `SubscriptionId`, `SchoolId`, `Amount`, `DueDate`, `Status` (Draft/Issued/Paid/Overdue/Cancelled), `Notes`. No Stripe/`PaymentIntent` reference. |
| `Payment` entity | Exists — `InvoiceId`, `Amount`, `Method` (`PaymentMethod` enum: BankTransfer, CreditCard, Cash, Online), `Reference`, `PaidAtUtc`. No gateway transaction id. |
| Stripe SDK | **NOT referenced** in any `.csproj` |
| Payment gateway service | **NONE** — no `IPaymentGateway` / Stripe client / webhook handler |
| School status on creation | **`Active` immediately** (`School.Create` sets `Status = "Active"`). No inactive/pending state. |

### 15.2 Required Behavior (from product owner)

1. **Two payment methods**
   - **Stripe** — self-service online payment (checkout session + webhook confirmation).
   - **"Other"** — manual/offline method (bank transfer, cash, etc.). **Created and reconciled by the Platform Admin only**, NOT by School Admins. School Admin picks "Other" at signup but cannot mark it paid; Platform Admin confirms receipt and activates the school.
2. **School activation gating**
   - A newly registered school is created with status **`Inactive`** (or `Pending`), **not** `Active`.
   - School stays inaccessible/inactive until a **successful payment** is recorded:
     - Stripe → webhook (`checkout.session.completed`) flips school to `Active`.
     - "Other" → Platform Admin marks the payment as completed → school flips to `Active`.
   - Until active, the school admin login should be blocked or shown a "pending payment" state.
3. **Yearly payments per academic year (no enforcement)**
   - Schools may pay **per academic year** (yearly billing), tied to the selected `AcademicYear`.
   - This is **optional / not enforced** — schools are NOT forced to pay yearly; it is a billing choice, not a hard gate on academic-year usage. No automatic suspension/lockout if a yearly payment is missed.
   - `Subscription` / `Invoice` should carry an optional `AcademicYearId` so yearly cycles can be tracked, but the system must not block academic operations when a yearly invoice is unpaid.

### 15.3 Model / Schema Changes Required

| Change | Where | Notes |
|--------|-------|-------|
| Add `School.Status` value `"Inactive"` / `"Pending"` | `Domain/Entities/Tenancy/School.cs` | Currently only `Active`/`Suspended`. Add `Inactive` + `Activate()`/`MarkPendingPayment()`. Seed `Create` should NOT set `Active`. |
| Add `PriceYearly` to `SubscriptionPlan` | `SubscriptionPlan.cs` | Support both monthly and yearly pricing. |
| Add `BillingCycle` (Monthly/Yearly) | `Subscription` (+ enum) | Record which cycle was chosen. |
| Add `AcademicYearId` (nullable) | `Subscription`, `Invoice` | For yearly-per-academic-year tracking (optional, non-enforced). |
| Add `PaymentMethod.Stripe` + `PaymentMethod.Other` | `PaymentMethod` enum | Keep existing values; Stripe = gateway, Other = manual (platform-reconciled). |
| Add Stripe references | `Invoice`/`Payment` | `StripePaymentIntentId`, `StripeSessionId`, `GatewayTransactionId`. |
| Add `PlatformConfirmedBy` / `ConfirmationNote` | `Payment` / `Invoice` | Audit who (Platform Admin) confirmed an "Other" payment. |

### 15.4 New Backend Building Blocks

- **`IPaymentGateway`** abstraction with `StripePaymentGateway` implementation (create checkout session, verify webhook signature, confirm payment).
- **Checkout endpoints**: `POST /billing/checkout/session` (Stripe) returning a session URL; school/admin pays; webhook `POST /billing/webhooks/stripe` updates `Invoice`+`Payment` and activates the school.
- **"Other" payment flow**: `POST /billing/payments/other` (Platform Admin only) to record a manual payment; `Platform.Admin` policy required. School Admin can only *initiate* the "Other" intent, not confirm it.
- **School activation service**: `School.Activate()` called by the payment-confirmation path (webhook or platform confirmation). Use the existing Outbox pattern to keep activation reliable.
- **Policies**: add `Billing.Manage` (already exists for platform/school), and a new `Billing.ConfirmOther` (Platform Admin only) for manual reconciliation.

### 15.5 Authorization Notes

- **Stripe** payments: can be triggered by the registering School Admin (self-service).
- **"Other" method**: School Admin may *select* it during onboarding, but **only Platform Admin can confirm/activate** — enforce via `Billing.ConfirmOther` policy and a guard in the handler (`if (!IsPlatformAdmin) throw Forbidden`).
- Activation must be idempotent (webhook may retry).

---

## 16. Import / Export

### 16.1 Current State (Confirmed)

- Frontend has a **foundation**: `features/importExport/importRunner.ts`, `importTemplates.ts`, `components/importExport/ImportExportButtons.tsx`, `lib/excel.ts`, `xlsx` dependency.
- Mappers exist for **students, teachers, parents** only.
- No **export** implementation (download templates / export lists) beyond the import scaffolding.
- No backend import endpoints — import appears to call the existing create APIs row-by-row from the client.

### 16.2 Required Coverage ("add import and export in each needed position")

| Area | Import | Export |
|------|:------:|:------:|
| Students | ✅ partial (mapper exists) | ➕ needed (CSV/XLSX of student list) |
| Teachers | ✅ partial | ➕ needed |
| Parents | ✅ partial | ➕ needed |
| StudentGuardians (link) | ➕ needed | ➕ needed |
| AcademicYears / Terms | ➕ needed | ➕ needed |
| GradeLevels | ➕ needed | ➕ needed |
| ClassRooms / Rooms | ➕ needed | ➕ needed |
| Subjects / CurriculumSubjects | ➕ needed | ➕ needed |
| Enrollments (student→class) | ➕ needed | ➕ needed |
| TeachingAssignments (teacher→class+schedule) | ➕ needed | ➕ needed |
| Attendance (bulk) | ✅ backend bulk record exists | ➕ export attendance sheet |
| Assignments / Submissions | ➕ needed | ➕ needed |
| Exams / Results / ReportCards | ➕ needed | ➕ export report cards (PDF/XLSX) |
| Billing (Invoices/Payments) | ➕ needed (platform) | ➕ export statements |
| People (all) | — | ➕ bulk export |

### 16.3 Implementation Guidance

- **Backend**: add `BulkImport` commands per entity (or a generic import handler) that validate via FluentValidation and run inside the transaction/UoW pipeline. Reuse the Outbox for side effects (e.g., login linking on bulk student import). Provide `Export` queries returning `PagedResult`/streamed data; generate XLSX server-side or return JSON for client-side `xlsx` rendering.
- **Frontend**: extend `importTemplates.ts` mappers to all above entities; add `ImportExportButtons` to every list/table page (Students, Teachers, Parents, Academics, Enrollment, Exams, Billing, Attendance). Provide downloadable blank templates + exported current filtered view.
- **Templates**: ship Excel templates with column headers + validation notes for each entity.

---

## 17. Filters & Pagination

### 17.1 Current State (Confirmed)

- `PagedResult<T>` + `PaginationParams` exist in `Application/Common/Models`.
- Pagination is implemented on **some** queries only: `GetAllSchoolsQuery` (page/pageSize), `GetStudentsQuery` (schoolId, searchTerm, gradeLevelId, classRoomId, pagination), `GetTeachersQuery`, `GetParentsQuery`.
- Many list endpoints return **unbounded** lists (e.g., `GetSubjectsQuery`, grade levels, rooms, terms, exams, assignments, attendance) — see **L-07**.
- Frontend list pages mostly render raw arrays; no reusable filter bar / pagination component is consistently applied.

### 17.2 Required Coverage ("add filters and pagination in all needed positions")

| Endpoint / Page | Needs |
|-----------------|-------|
| Schools (`/platform/schools`) | ✅ has pagination; add filters: status, name/search, plan |
| Students | ✅ partial; add filters: status, enrollment status, guardian, date range |
| Teachers | add filters: employment status, subject, class; ensure pagination |
| Parents | add search + pagination |
| AcademicYears / Terms | add pagination + filter by status (current/closed) |
| GradeLevels / ClassRooms / Rooms | add pagination + search |
| Subjects / CurriculumSubjects | add pagination + filter by education stage / grade |
| Enrollments | add filters: academic year, class, student, status + pagination |
| TeachingAssignments | filter by teacher/subject/term/class + pagination |
| Attendance | filter by date range, class, status + pagination |
| Assignments | filter by class/subject/status/due date + pagination |
| Exams / Results | filter by term/class/subject + pagination |
| ReportCards | filter by student/class/term + pagination |
| Billing Invoices / Payments | filter by status/method/date + pagination (platform + school) |
| Notifications | filter by read/type/channel + pagination |

### 17.3 Implementation Guidance

- **Backend**: standardize every list query on `PaginationParams` + a `Filter` object; return `PagedResult<T>`; apply `AsNoTracking()` for reads; use `SplitQuery` (already configured) for included collections.
- **Frontend**: build one shared `<FilterBar>` + `<Pagination>` (Ant Design `Table` `pagination` + `Form` filters) used by all list pages; preserve filter state in URL query params.
- Ensure tenant scoping is preserved (all filtered queries must still flow through `TenantAuthorizationBehavior` + global `SchoolId` filter).

---

## 14. Feature Roadmap (Updated)

> Phases 1–10 below are the original roadmap; new payment/import/export/filter items are folded into the phases where they belong.

### Phase 1: Fix Critical Bugs (1-2 days)
*(unchanged)* — register missing policies, remove mock dashboard data, implement `PortalReadService`, fix `ROLE_HOME`, fix placeholders.

### Phase 2: Student & Parent Portals (1-2 weeks)
*(unchanged)*

### Phase 3: Teacher Portal Completion (1 week)
*(unchanged)*

### Phase 4: Exam & Grading Module (1-2 weeks)
*(unchanged)* — add **import/export of exams, results, report cards** (§16) and **filters/pagination** on exam/result pages (§17).

### Phase 5: Attendance Module (1 week)
*(unchanged)* — add **attendance export** (§16) and **date/class/status filters + pagination** (§17).

### Phase 6: Email Services (1 week)
*(unchanged)*

### Phase 7: Public Pages & School Profiles (1-2 weeks)
*(unchanged)*

### Phase 8: Platform Enhancements (1 week)
*(unchanged)*

### Phase 9: Remaining CRUD & Polish (1-2 weeks)
- [ ] Add Delete operations for remaining entities
- [ ] Add Subject management page
- [ ] Build Notification center UI
- [ ] Build **Billing management UI** (plans, subscriptions, invoices, payments) — see §15
- [ ] Build Assignment management UI
- [ ] Add forgot/reset password flow
- [ ] Add profile/settings page for all roles
- [ ] Add error boundaries
- [ ] Add form state persistence
- [ ] **Import/Export UI on every list page** — see §16
- [ ] **Filters + pagination on every list page** — see §17

### Phase 10: Advanced Features (ongoing)
- [ ] Real-time notifications (SignalR)
- [ ] File upload/download (Document entity)
- [ ] Bulk import/export for all entities
- [ ] Advanced analytics & reporting
- [ ] PDF report card generation
- [ ] Multi-language support (i18n)
- [ ] Mobile responsive optimization
- [ ] PWA support for teacher/student offline attendance

### New — Phase 11: Payments, Onboarding & Activation (1-2 weeks)  *(added)*

- [ ] Add `Inactive`/`Pending` school status; `School.Create` no longer sets `Active` (§15.3)
- [ ] Add `PriceYearly` + `BillingCycle` + `AcademicYearId` (nullable, non-enforced) to billing entities (§15.3)
- [ ] Add `PaymentMethod.Stripe` + `PaymentMethod.Other`; Stripe/`GatewayTransactionId` fields on `Invoice`/`Payment`
- [ ] Implement `IPaymentGateway` + `StripePaymentGateway`; add Stripe NuGet package
- [ ] `POST /billing/checkout/session` (Stripe self-service) + `POST /billing/webhooks/stripe` (confirm + activate school)
- [ ] "Other" payment flow: School Admin initiates, **Platform Admin confirms** via `Billing.ConfirmOther` policy
- [ ] School activation service (idempotent) via existing Outbox pattern
- [ ] Block/degrade login for inactive schools (pending-payment screen)
- [ ] Platform Admin billing dashboard: confirm manual payments, view yearly-per-academic-year invoices
- [ ] Email confirmation on payment success (ties into §8 email work)

---

*End of Report*
