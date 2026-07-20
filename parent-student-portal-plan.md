# Parent & Student Portal Integration Plan

## Scope and decision

This document specifies how to add a distinct Parent/Student experience to the existing React application without creating another frontend, deployment, or backend. It is based on the current `frontend`, `Api`, `Application`, `Domain`, and `Infrastructure` structure. It deliberately does not prescribe implementation code.

The application should be treated as two product experiences in one shell:

| Product area | Primary users | Primary goal |
|---|---|---|
| Operations workspace | Platform Admin, School Admin, Teacher | Configure, manage, create, record, and operate the school |
| Family portal | Student, Parent | Understand school life, act on assigned work, and stay informed |

The backend is already materially ahead of the frontend: `PortalController` and `PortalReadService` expose student and parent dashboard, attendance, assignments, grades, report cards, billing, timetable/classes, and notification projections. The plan should consume these read models rather than recreate administrative queries in the browser.

## 1. Current architecture

### Frontend structure

The React frontend is a Vite/TypeScript single-page application using React Router, TanStack Query, Zustand, Axios, Tailwind-style utility classes, Ant Design, Radix primitives, Lucide icons, React Hook Form, Zod, and Sonner.

- Routes are declared centrally in `frontend/src/router/routes.tsx` and rendered through `useRoutes` inside a `BrowserRouter`.
- Pages are lazy-loaded. Query client, Ant Design theme, global toaster, and router are composed in `frontend/src/router/index.tsx`.
- The current hierarchy is role-oriented but not product-oriented: `/platform`, `/school`, `/academics`, `/people`, `/enrollment`, `/teacher`, `/student`, and `/parent`.
- Platform and school administration pages are concrete feature pages. Teacher has one real dashboard but several placeholders. Student and parent have route placeholders only.
- The application currently applies `AppLayout` to every authenticated product area, including student and parent routes.

### Layout, navigation, and dashboard architecture

`AppLayout` is a desktop administrative shell: fixed left `Sidebar`, a `TopBar`, breadcrumb/search controls, a collapsing desktop sidebar, and a mobile slide-over sidebar. `Sidebar` returns detailed navigation for platform admins, school admins, and teachers; it returns no navigation for students and parents. The result is that the existing `/student` and `/parent` routes technically exist but cannot feel complete or navigable.

Reusable dashboard primitives are already useful foundations: `DashboardTemplate`, `DashboardHeader`, `StatCard`, `InfoCard`, `QuickActionCard`, `AttendanceSummary`, `UpcomingLessons`, `AnnouncementsPanel`, `TimelinePanel`, `EmptyState`, `Spinner`, `ErrorBoundary`, `NotificationBell`, `UserMenu`, `Avatar`, buttons, inputs, badges, and typography. Several dashboard data modules include mock data, so portal work must avoid presenting mock information as parent/student records.

### Authentication and authorization

Authentication is handled by `AuthApi`, Axios, and the persisted Zustand `authStore`.

- `POST /auth/login` returns access token, rotating refresh token, user ID, email, active school ID, and backend role.
- The store maps `SuperAdmin`, `SchoolAdmin`, `Teacher`, `Student`, and `Parent` to frontend roles and parses JWT `permission` and `is_platform_admin` claims.
- Axios attaches the bearer token, attempts one refresh after a 401, and clears state/returns to `/auth/login` on refresh failure.
- `ProtectedRoute` performs authentication and role checks. `RequirePermission` adds a client-side permission check. `ROLE_HOME` maps all five roles to a default route.
- The API enforces authorization independently through policy attributes. Its JWT includes school scope and permissions. Parent/child ownership and student ownership are validated in portal read services, not only in the UI.

### Role system and API organization

The backend permission provider defines separate Parent and Student permissions, while portal endpoints use policies such as `Profile.Read`, `Children.Read`, `Attendance.ReadOwn`, `Attendance.ReadChild`, `Assignment.ReadOwn`, `Assignment.Submit`, `Exam.Read`, and `Notification.Read`. The app also has tenant membership and active-school selection during login. Portal queries explicitly receive/validate a school ID and are tenant-filtered.

Frontend feature modules keep API calls, hooks, and types near administrative domains (`academics`, `students`, `parents`, etc.). React Query is the server-state mechanism; Zustand is reserved for authentication, theme, and shell UI state. This is a sound split to retain.

### Strengths

- One type-safe, lazy-loaded SPA with a shared API client, query cache, visual primitives, theme state, and error boundary.
- Existing role-home mapping already recognizes Parent and Student.
- JWT refresh and backend authorization exist.
- Backend portal endpoints are purpose-built, paginated where appropriate, tenant-scoped, and ownership-checked.
- Domain support already exists for student/guardian links, enrollment, attendance, assignments/submissions, exams/report cards, invoices/payments, documents, notifications, and schedules.

### Limitations to address

- Parent and student routes are placeholders and share an operations-focused `AppLayout`.
- `Sidebar` has no parent/student navigation; top-bar search, breadcrumbs, and generic labels are not family-portal-oriented.
- The frontend has no portal API modules, portal DTO types, or portal React Query hooks despite the backend support.
- The frontend obtains display names by deriving them from email; portal profile DTOs should become the display source.
- Roles and permissions are not fully aligned by name across provider, API policies, and frontend route checks (for example `assignment.read` versus `Assignment.ReadOwn`, and duplicated spelling variants such as `profileread`). Treat backend policy behavior as the authority and standardize this contract before relying on client gating.
- The current route hierarchy mixes product roots with cross-cutting admin roots (`/academics`, `/people`, `/enrollment`), which is unsuitable for a self-service product namespace.
- Notification UI is currently a placeholder toast, and no message/conversation API currently exists.

## 2. Parent/Student portal architecture

Create a self-service portal area inside the same app, isolated at the layout, route, feature, data-contract, and navigation levels while sharing application infrastructure.

### Layouts

Use a dedicated `PortalLayout` for both roles, configured by an explicit portal mode (`student` or `parent`), rather than branching throughout `AppLayout`.

- **Desktop:** lightweight branded header, concise primary navigation, a contextual child switcher for parents, notification entry point, account menu, and optional secondary navigation on wide screens.
- **Mobile:** bottom navigation for the highest-frequency destinations, with a full menu/sheet for the remainder. Do not reuse the admin collapsing sidebar as the default mobile paradigm.
- **Content:** comfortable reading width, fewer dense tables, task/status cards, clear empty states, and student/child context visible where data is scoped.
- **Page header:** portal-specific title, period/filter controls where relevant, and no generic global administration search until a useful portal search capability exists.

Keep `AppLayout` as the operations shell. Teacher can remain there initially, although a future teacher-specific layout may be warranted.

### Dashboards and navigation

Use separate dashboard pages and route groups:

- Student dashboard is about today: next lesson, assignments due, attendance trend, latest results, unread notices, and quick links.
- Parent dashboard is about children: one clear child summary per child, alerts (absence, overdue work, unpaid invoice), recent grades, upcoming events, and a child selector.
- Parent child detail pages must carry `studentId` in the route and show the selected child consistently. Never infer a child from a mutable global state alone.
- Navigation definitions should be declarative and portal-specific, with eligibility/feature flags and permission metadata. Do not add parent/student cases to the existing admin `Sidebar` navigation switch.

### Shared versus isolated code

**Share:** Axios/refresh behavior, QueryClient, auth session, theme preferences, tokens/styles, atom/molecule components, loading/error/empty states, pagination/filter conventions, notification plumbing, profile account actions, document-download utilities, and API error normalization.

**Isolate:** portal layouts, portal navigation, portal route definitions and guards, portal pages, dashboard assemblies, portal API modules/hooks/types, child-selection state, portal-specific visual components, and portal-only copy/content. Administrative people/academic CRUD pages must not be reused as portal pages.

## 3. Recommended folder structure

The following is a proposed target structure. Existing administrative folders remain in place; it avoids a risky reorganization.

```text
frontend/src/
  layouts/
    AppLayout.tsx                     # retained operations shell
    AuthLayout.tsx                    # retained
    portal/
      PortalLayout.tsx
      PortalHeader.tsx
      PortalMobileNav.tsx
      PortalNavigation.ts
      ChildContextBar.tsx
  router/
    routes.tsx                        # compose route modules
    portalRoutes.tsx
    guards.tsx                        # retain generic authentication guard
    RequirePermission.tsx             # retain/align client hints
    RequirePortalRole.tsx
  features/
    portal/
      api/
        studentPortal.api.ts
        parentPortal.api.ts
        notifications.api.ts
      hooks/
        useStudentPortal.ts
        useParentPortal.ts
      types/
        portal.types.ts
      state/
        childSelectionStore.ts
      components/
        StudentSummaryCard.tsx
        ChildSummaryCard.tsx
        AttendanceTrend.tsx
        AssignmentStatusCard.tsx
        GradeSummary.tsx
  pages/
    student/
      StudentDashboardPage.tsx
      StudentProfilePage.tsx
      StudentClassesPage.tsx
      StudentTimetablePage.tsx
      StudentAssignmentsPage.tsx
      StudentAssignmentDetailPage.tsx
      StudentAttendancePage.tsx
      StudentGradesPage.tsx
      StudentReportCardsPage.tsx
      StudentExamsPage.tsx
      StudentNotificationsPage.tsx
      StudentDownloadsPage.tsx
    parent/
      ParentDashboardPage.tsx
      ParentChildrenPage.tsx
      ParentChildOverviewPage.tsx
      ParentChildAttendancePage.tsx
      ParentChildGradesPage.tsx
      ParentChildAssignmentsPage.tsx
      ParentChildTimetablePage.tsx
      ParentChildReportCardsPage.tsx
      ParentBillingPage.tsx
      ParentNotificationsPage.tsx
      ParentProfilePage.tsx
      ParentSettingsPage.tsx
  components/
    atoms/ molecules/ organisms/ templates/  # retained shared design system
```

Keep generic components under `components`; place components that encode portal concepts under `features/portal/components`. Keep backend portal contracts out of administrative `student.types.ts` and `parent.types.ts` unless those files are deliberately split into shared entity types and portal-view types. This prevents admin editing DTOs and self-service read DTOs from becoming indistinguishable.

## 4. Routing strategy

Use stable product namespaces. The current `/student` and `/parent` roots are suitable and should become the only portal roots.

| Route group | Role | Suggested destinations |
|---|---|---|
| `/platform/*` | Platform Admin | existing platform operations |
| `/school`, `/academics/*`, `/people/*`, `/enrollment/*` | School Admin (platform admin as explicitly supported) | existing school operations |
| `/teacher/*` | Teacher | existing teacher area, expanded separately |
| `/student/*` | Student | dashboard, classes, timetable, assignments, attendance, grades, exams, report cards, notifications, profile, downloads |
| `/parent/*` | Parent | dashboard, children, child-scoped academic routes, billing, notices, profile, settings |

Suggested student routes:

- `/student` — dashboard
- `/student/classes`, `/student/timetable`
- `/student/assignments`, `/student/assignments/:assignmentId`
- `/student/attendance`, `/student/grades`, `/student/exams`, `/student/report-cards`
- `/student/notifications`, `/student/profile`, `/student/downloads`

Suggested parent routes:

- `/parent` — dashboard
- `/parent/children`
- `/parent/children/:studentId` — child overview
- `/parent/children/:studentId/attendance|grades|assignments|timetable|report-cards`
- `/parent/billing`, `/parent/notifications`, `/parent/profile`, `/parent/settings`

Nest each group beneath authentication and a role guard, then beneath `PortalLayout`. The role guard should reject cross-portal access even when a user guesses a URL. Portal page guards should map to the matching server policy as a UI convenience only; they are not a security boundary. Use a dedicated unauthorized/not-found experience within the portal for authenticated users rather than silently redirecting all cases to a dashboard; preserve a safe return route where appropriate.

## 5. Authentication & authorization

### Login and redirect flow

Continue using the single login endpoint and role-home redirect. After login or successful token refresh, route users to their product home (`/student` or `/parent`). If a non-platform account has multiple active school memberships, introduce an explicit school-selection step before portal data loads; silently choosing the first membership is not a durable family-portal experience.

The authenticated session should retain only identity/session claims and selected school context. Portal profile, enrollment ID, current student information, and children must be fetched from server projections and cached with school- and child-specific query keys.

### Permissions and ownership

- The API remains the source of truth. Every request must include the bearer token and required school context.
- Student data queries must validate that the enrollment/student belongs to the authenticated student. The existing portal read service already does this for its portal queries.
- Parent data queries must validate the `StudentGuardian` relationship for every requested child. Never trust a client-side child ID.
- Parent access should use child-specific permissions (`Children.Read`, `Attendance.ReadChild`, and analogous policies) rather than administrative student permissions.
- Use frontend permissions to hide unavailable actions and render useful explanatory states, not to protect data.
- Align policy names, permission-provider strings, JWT claims, and frontend permission constants in a compatibility pass. Establish a canonical permission dictionary before adding many portal guards.

### Session handling

Retain rotating refresh behavior, but define a portal-friendly expiration path: preserve the intended URL, show a clear “session expired” message after refresh fails, and return the user to login. Invalidate React Query data and clear selected-child state on logout, school switch, and identity change. Avoid storing sensitive child records in persisted Zustand state; React Query memory cache is the preferred location for server data.

## 6. Student experience

| Page | Purpose and expected UI | Access and backend support |
|---|---|---|
| Dashboard | Today-first overview: next lessons, due/overdue assignments, attendance percentage, recent grades, unread notices, and quick actions. | Student ownership; `GET /portal/student/{enrollmentId}/dashboard` (`Profile.Read`). |
| Profile | Read-only personal/school profile, class and academic-year context; later allow separately approved self-service edits. | `GET /portal/student/profile` (`Profile.Read`). Profile editing is not currently exposed. |
| Classes / Subjects | Current subjects, teacher names, class membership, and meeting times. Cards can open subject/context views. | `GET /portal/student/{enrollmentId}/classes` (`MyClasses.Read`). |
| Timetable | Responsive weekly timetable plus an agenda/today mode, term selector, room and teacher labels. | `GET /portal/student/{enrollmentId}/schedule` (`Attendance.ReadOwn` in current API; policy naming should be corrected before launch). |
| Assignments / Homework | Filterable assignment list with due date, subject, status, score, and late/overdue indicators. Detail view supports instructions, attached materials, submission status, and files. | Read: `GET /portal/student/{enrollmentId}/assignments` (`Assignment.ReadOwn`). Submit: `POST /assignments/{assignmentId}/submissions` (`Assignment.Submit`), multipart, up to current API limit. Assignment detail/download endpoints may be needed if not contained in list DTOs. |
| Attendance | Summary first, then paginated history with date/status filters and clear explanations of absent/late/excused. | `GET /portal/student/{enrollmentId}/attendance` and/or `/attendance/students/{enrollmentId}` (`Attendance.ReadOwn`). |
| Grades | Recent exam results, subject breakdown, filters by term, and a plain-language grade/percentage presentation. | `GET /portal/student/{enrollmentId}/exams` (`Exam.Read` in current controller). Confirm student policy issuance includes this policy. |
| Report cards | Term report cards, subject rows, overall grade, lock/published state, and downloadable/printable artifact when backend supports it. | `GET /portal/student/{enrollmentId}/report-cards` (`Exam.Read`). PDF/download endpoint is a future contract. |
| Exams | Upcoming exam schedule and completed result history. | Result endpoint exists; a student-visible upcoming-exam schedule projection is not clearly exposed and should be added before this page is committed. |
| Notifications | Inbox with unread count, read state, notice categories, and deep links to relevant portal content. | `GET /portal/student/notifications`; general notification endpoints support read actions and device registration. |
| Downloads / Library | Central list of shared documents, materials, and report-card downloads. | Domain has `Document`; a student-authorized discovery/download API is not currently evident. Define it before implementation. |
| Calendar | Consolidated schedule, assignments, exams, and school events. | Requires a composed read endpoint or client composition with bounded queries; school-event API is not currently evident. |
| Messages | Teacher/school communication, only if approved moderation/retention rules exist. | No conversation/message backend contract currently exists; do not simulate with notifications. |
| Fees | Student-visible fee status only where school policy allows it. | Parent billing read endpoint exists; no student billing endpoint is evident. Make this optional. |

## 7. Parent experience

| Page | Purpose and expected UI | Access and backend support |
|---|---|---|
| Dashboard | Child cards with attendance, pending work, latest results, upcoming exams, unread notices, and outstanding balance. Alerts should be actionable and child-labelled. | `GET /portal/parent/dashboard` (`Children.Read`). |
| Children | A dedicated child list and selector. Each card shows class, attendance, pending work, and visible alerts. | Dashboard supplies summaries; add a focused children-list projection only if dashboard data is insufficient. |
| Child overview | Child’s academic profile, current class/year, attendance percentage, assignment summary, and latest overall grade. | `GET /portal/parent/children/{studentId}/profile` (`Children.Read`). |
| Child attendance | History and trend with filters, clear attendance status explanations, and school contact route for corrections. | `GET /portal/parent/children/{studentId}/attendance` (`Attendance.ReadChild`). |
| Child grades | Subject/exam results with term filtering and published-state messaging. | `GET /portal/parent/children/{studentId}/grades` (`Children.Read` in current controller); policy should be made semantically explicit. |
| Child homework | Assigned/completed/pending work, due dates, status and teacher feedback. Read-only for parents unless a policy explicitly permits guardian submission. | `GET /portal/parent/children/{studentId}/assignments` (`Children.Read`). |
| Child timetable | Weekly schedule and today’s agenda for the selected child. | No parent-specific timetable endpoint is documented. Add a guardian-validated endpoint; do not use a student endpoint without server ownership support. |
| Child report cards | Published term cards and subject breakdown. | `GET /portal/parent/children/{studentId}/report-cards` (`Children.Read`). |
| Payments / fees | Invoice list, paid/balance status, due dates, receipts, and payment action only after payment-provider design. | `GET /portal/parent/billing` currently returns school invoices. Confirm invoice-to-child/family association before presenting it as a child fee ledger. Online payment/receipts need new APIs. |
| Notices and notifications | School notices and targeted alerts, with read state and destination links. | `GET /portal/parent/notifications` (`Notification.Read`) plus generic notification read actions. |
| Events / calendar | School events, conferences, deadlines, child timetable, and exams. | Event/calendar read API is not currently evident; specify before implementation. |
| Teacher communication | Controlled contact/meeting requests or message threads, using child context. | No messaging endpoint exists. Requires new domain model, authorization, moderation, retention, and notification design. |
| Profile and settings | Parent account details, contact preferences, language/theme, device notifications, and security/session controls. | Current parent profile read/update contract is not evident; device token registration exists. |

## 8. Shared features

Build these once and use them across operations and portal areas where the interaction is genuinely the same:

- Authentication, refresh, logout, active-school switching, and session-expiry handling.
- Theme preference, accessibility preferences, language/localization framework, and design tokens.
- Request/error normalization, query caching, loading skeletons/spinners, error boundary, empty states, pagination, date/number formatting, and toasts.
- Account menu, notification count/list/read actions, device-token registration, profile/security settings where fields are shared.
- File upload/download, document preview, image/avatar display, and print/export primitives where permissions allow.
- Audit-safe route analytics and client error telemetry, with no student information in event payloads.

Do not share operational data tables, CRUD forms, admin search, management controls, or the current `Sidebar` merely for implementation convenience.

## 9. UI/UX guidelines

The family portal should be modern, calm, mobile-first, responsive, accessible, and reassuring. It should feel like a personal school companion, not a database console.

- Prefer an agenda and card-based information hierarchy over wide tables. Tables remain appropriate for detailed attendance/grade history at larger breakpoints.
- Put urgent information first: today’s schedule, assignment due dates, attendance alerts, unread notices, then historical data.
- Use plain labels (“Homework”, “My child’s attendance”) and explain education-specific terms. Avoid admin terms such as “entity”, “roster”, or “manage”.
- Use a distinct portal visual mode within existing brand tokens: warmer/clearer illustrations or contextual icons, larger touch targets, less chrome, and fewer simultaneous controls. Retain the same core brand, theme, and accessibility contrast rules.
- On mobile, optimize common tasks for one-handed use: bottom navigation, compact filters, sticky child context, and clearly separated destructive/account actions.
- Ensure keyboard navigation, focus visibility, semantic headings, announced async states, sufficient contrast, non-color-only statuses, screen-reader labels, and locale-aware dates/times.
- Make data confidence visible: published/locked grades, last refresh where meaningful, empty-state reasons, and permissions/availability explanations.
- Use child name/photo/initials as a strong context marker for every parent child-specific view; never rely only on color or tab position.

## 10. Permission matrix

This is a product-level matrix, not an API authorization substitute. “Own” means the user’s own record; “child” means verified guardian relationship; “scoped” means assigned class/school scope.

| Capability | Platform Admin | School Admin | Teacher | Parent | Student |
|---|---|---|---|---|---|
| View | All tenant/platform scopes as authorized | School-wide | Assigned classes | Own children | Own record |
| Create | Schools and authorized operations | School data | Assigned learning records | No, except future requests | Assignment submission only |
| Edit | Authorized operations | School data | Attendance, grades, assigned work | Profile/preferences; future approved requests | Profile/preferences if approved; own submission |
| Delete | Authorized operations | School data where policy allows | Assigned drafts/records where policy allows | No academic deletion | No academic deletion |
| Export | Platform/school reporting | School reporting | Assigned class reports | Own child documents/receipts | Own documents/report cards |
| Messaging | Administrative/operational | School notices | Assigned-family communication | Contact/meeting request for own child | Contact subject to school policy |
| Grades | Manage/reporting | Create/manage/publish | Enter for assigned classes | View child published grades | View own published grades |
| Attendance | Reporting/oversight | View/manage rules | Record assigned class | View child history | View own history |
| Timetable | View/manage | Create/manage | View own/assigned | View child | View own |
| Reports | Platform analytics | School reports | Assigned class reports | Child report cards | Own report cards |
| Profile | Own account | Own account | Own account | Own account | Own account |

Before implementation, translate this matrix into canonical backend policy names, role permissions, and frontend navigation/action visibility rules. Parent/student client permissions must never grant access beyond validated ownership.

## 11. Migration strategy

### Incremental delivery

1. **Contract and security alignment:** inventory current portal endpoints against the proposed pages; normalize policy/permission naming; verify login, membership selection, student ownership, parent guardian ownership, and invoice association. Add contract tests before frontend screens.
2. **Portal foundation:** introduce route module, role guard, `PortalLayout`, navigation model, portal API/types/hooks, query-key conventions, unauthorized/not-found states, and child selection. Keep existing admin routes/layout unchanged.
3. **Read-only student MVP:** dashboard, timetable/classes, assignments list, attendance, grades/report cards, and notifications, using existing read endpoints. Validate mobile behavior and empty/error states.
4. **Read-only parent MVP:** dashboard, children/child overview, attendance, grades, assignments, report cards, notifications, and correctly scoped billing. Validate multi-child behavior and direct URL access.
5. **Actions:** assignment submission, notification read/device registration, profile/preferences. Introduce payments, messaging, profile edits, approvals, and downloads only after their server contracts and policies exist.
6. **Refinement and rollout:** accessibility audit, performance tuning, analytics, documentation, feature flags, and gradual school rollout.

### Testing strategy

- Unit test route/role/permission decisions, DTO mappers, child-context behavior, dates/statuses, and all loading/error/empty states.
- Component test responsive portal navigation, keyboard interaction, child switching, and page-level states.
- Integration test API modules against documented portal contracts and ensure query keys include school and child/enrollment IDs.
- End-to-end test login redirects for every role; direct URL denial; student access only to own data; parent access only to linked children; multiple children; expired refresh token; school switching; assignment submission; and notification read flow.
- Backend authorization/ownership tests are mandatory regression coverage for every new parent/student endpoint.
- Run visual and accessibility checks on narrow mobile, tablet, and desktop breakpoints.

### Rollout plan

Release portal routes behind a feature flag or school capability flag. First enable a non-production tenant with representative student/parent links and multi-child data, then a small pilot school. Instrument errors and endpoint latency, gather family/teacher feedback, publish support guidance, and retain the existing operations workspace untouched. A rollback should disable portal navigation/routes without schema rollback or changes to administrative flows.

## 12. Risks and technical debt

- **Permission drift:** current role permission strings, controller policy names, and frontend route checks are not consistently named. This can create invisible navigation, false client denials, or reliance on platform-admin bypass. Establish one canonical vocabulary.
- **Identifier/context ambiguity:** several portal endpoints require `schoolId` and enrollment/student IDs. Missing or stale school/child context can lead to confusing failures; query keys and route parameters must be explicit.
- **Multiple memberships:** login currently selects the first active membership when no school is supplied. This is insufficient for parents/students attached to more than one school.
- **Invoice semantics:** parent billing appears school-scoped. Validate parent/family/student invoice linkage before exposing balances as personal obligations.
- **Data publication:** grades/report cards need an explicit published/locked visibility policy to avoid exposing drafts.
- **Sensitive data:** family information, minors’ data, attendance, grades, and documents require strict logging, caching, telemetry, and retention discipline. Do not persist detailed portal responses in local storage.
- **N+1/performance:** parent dashboards may aggregate several children. Establish response size limits, server-side batching, pagination, and cache policies.
- **Placeholders and mock data:** portal pages must not reuse dashboard mock data. Treat a missing API as a dependency, not a reason to fake school records.
- **Message scope:** messaging is a substantial domain feature, not a UI tab. It needs authorization, moderation, audit trail, delivery, retention, and abuse handling.
- **Layout coupling:** enhancing the existing admin sidebar for portal users would increase conditional complexity and compromise both experiences; maintain isolated portal layout/navigation.

## 13. Future improvements

- PWA installation with offline shell, cached timetable/notifications, controlled stale-data indicators, and later offline assignment drafts.
- Native mobile applications if notifications, camera uploads, device integration, or offline workflows exceed PWA needs; retain the same backend contracts.
- Web/device push notifications through the existing device-token foundation, with granular parent/student preferences and quiet hours.
- Parent approval workflows: absence excuses, consent forms, profile change requests, trip approvals, and payment approvals, all with auditable status.
- A moderated, policy-driven teacher/family messaging and meeting-request system with attachment controls and notification integration.
- Digital payments, receipts, installments, refunds, and family/child invoice allocation after a payment-provider and reconciliation design.
- A calendar/event module combining school events, timetable, assessment dates, and conferences, exportable to personal calendars.
- Document center/library with authorized materials, report-card PDFs, consent forms, and secure expiring downloads.
- Localization, RTL support where required, accessibility personalization, and age-appropriate student modes.
- Advanced insights: attendance/assignment risk signals, trends over time, and carefully designed nudges, with staff-visible explanations and privacy review.
