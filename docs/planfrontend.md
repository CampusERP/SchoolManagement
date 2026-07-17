# Frontend Plan — Multi-Tenant School Management SaaS
> **Stack**: React 19 · TypeScript · Vite · Tailwind CSS v4 · Ant Design · Radix UI · Zustand · TanStack Query · Sonner · React Router v7

---

## Branch Status
> **Active backend branch**: `master` (CleanUp1 already merged — up to date)
> **Frontend location**: `d:\Projects\SchoolManagement\Frontend\`

---

## Tech Stack Decisions

| Concern | Library | Reason |
|---|---|---|
| Framework | **React 19** | Ecosystem, concurrent features |
| Build Tool | **Vite 6** | Fast HMR, ESM-native |
| Language | **TypeScript 5.5+** | Type safety across all layers |
| Styling | **Tailwind CSS v4** | Utility-first, design tokens via CSS vars |
| Component Library | **Ant Design 5** | Rich data-heavy components (Tables, Forms, Datepicker) |
| Headless Primitives | **Radix UI** | Accessible dialogs, dropdowns, popovers, tooltips |
| State (client) | **Zustand** | Lightweight, zero-boilerplate store |
| State (server) | **TanStack Query v5** | Caching, refetching, pagination, mutations |
| Toasts / Alerts | **Sonner** | Beautiful stacked toasts; integrates with TanStack |
| Forms | **React Hook Form + Zod** | Type-safe, schema-validated forms |
| Routing | **React Router v7** | Nested layouts, lazy routes, loaders |
| HTTP Client | **Axios** | Interceptors for JWT attach + refresh |
| Icons | **Lucide React + Ant Icons** | Consistent icon set |
| Date Utils | **Day.js** | Lightweight, Ant Design-compatible |
| Design Pattern | **Atomic Design** | Atoms → Molecules → Organisms → Templates → Pages |

---

## Atomic Design Methodology

All UI is built bottom-up following the Atomic Design pattern:

```
src/components/
├── atoms/          # Smallest indivisible units
│   ├── Button/
│   ├── Badge/
│   ├── Avatar/
│   ├── Input/
│   ├── Spinner/
│   ├── Divider/
│   └── Typography/
│
├── molecules/      # Combinations of atoms with one purpose
│   ├── FormField/          # Label + Input + Error message
│   ├── SearchInput/        # Input + Icon + Clear button
│   ├── StatCard/           # Icon + Value + Label + Trend
│   ├── UserMenu/           # Avatar + Dropdown
│   ├── BreadcrumbNav/      # Breadcrumb trail
│   ├── StatusBadge/        # Colored pill for statuses
│   ├── ConfirmPopover/     # Radix Popover confirm action
│   └── EmptyState/         # Illustration + CTA message
│
├── organisms/      # Complex, self-contained sections
│   ├── DataTable/          # Ant Table + search + pagination + actions
│   ├── PageHeader/         # Title + breadcrumb + action buttons
│   ├── Sidebar/            # Collapsible nav with role-aware items
│   ├── TopBar/             # Header with school switcher + user menu
│   ├── LoginForm/          # Full login card with validation
│   ├── StudentForm/        # Create/Edit student form organism
│   ├── TeacherForm/
│   ├── ParentForm/
│   ├── SchoolForm/
│   ├── AcademicYearForm/
│   ├── ClassroomForm/
│   └── EnrollmentForm/
│
├── templates/      # Page-level layout scaffolds (no real data)
│   ├── DashboardTemplate/  # Sidebar + TopBar + content area
│   ├── AuthTemplate/       # Split-screen auth layout
│   ├── ListTemplate/       # PageHeader + Filters + DataTable
│   └── DetailTemplate/     # PageHeader + cards + tabs
│
└── pages/          # Route-level components that wire data into templates
    ├── auth/
    ├── platform/
    ├── school/
    ├── academics/
    ├── people/
    ├── enrollment/
    ├── teacher-portal/
    ├── student-portal/
    └── parent-portal/
```

---

## Project Structure

```
Frontend/
├── public/
├── src/
│   ├── components/          # Atomic Design component tree (above)
│   ├── layouts/
│   │   ├── AppLayout.tsx    # Sidebar + TopBar shell
│   │   └── AuthLayout.tsx   # Centered auth shell
│   ├── features/            # Domain-scoped logic
│   │   ├── auth/
│   │   │   ├── api.ts
│   │   │   ├── hooks.ts     # useLogin, useRefreshToken
│   │   │   └── store.ts
│   │   ├── schools/
│   │   │   ├── api.ts
│   │   │   ├── hooks.ts     # useSchools, useSchoolById
│   │   │   └── types.ts
│   │   ├── academics/
│   │   ├── people/
│   │   └── enrollment/
│   ├── lib/
│   │   ├── axios.ts         # Axios instance + interceptors
│   │   ├── queryClient.ts   # TanStack Query client config
│   │   └── utils.ts         # cn(), formatDate(), etc.
│   ├── hooks/
│   │   ├── useDebounce.ts
│   │   ├── usePagination.ts
│   │   └── usePermissions.ts
│   ├── store/
│   │   ├── authStore.ts     # user, token, role, schoolId, permissions
│   │   ├── themeStore.ts    # dark/light mode
│   │   └── uiStore.ts       # sidebar collapsed, active menu
│   ├── router/
│   │   ├── index.tsx
│   │   ├── guards.tsx       # ProtectedRoute, RoleRoute
│   │   └── routes.ts
│   ├── styles/
│   │   ├── globals.css      # Tailwind directives + CSS vars
│   │   └── antd-theme.ts    # Ant Design theme token overrides
│   ├── types/
│   │   ├── api.types.ts     # PagedResult<T>, Result<T>
│   │   ├── auth.types.ts
│   │   ├── school.types.ts
│   │   ├── academics.types.ts
│   │   ├── people.types.ts
│   │   └── enrollment.types.ts
│   ├── App.tsx
│   └── main.tsx
├── tailwind.config.ts
├── vite.config.ts
├── tsconfig.json
└── package.json
```

---

## Design System — Tailwind CSS v4 + Ant Design Theme

### CSS Design Tokens (`globals.css`)

```css
@import "tailwindcss";

:root {
  /* Brand */
  --color-primary:        #4F46E5;   /* Indigo 600 */
  --color-primary-hover:  #4338CA;
  --color-primary-light:  #EEF2FF;
  --color-secondary:      #0EA5E9;   /* Sky 500 */
  --color-accent:         #8B5CF6;   /* Violet 500 */

  /* Neutrals */
  --color-surface:        #F8FAFC;
  --color-surface-card:   #FFFFFF;
  --color-border:         #E2E8F0;
  --color-text-primary:   #0F172A;
  --color-text-secondary: #64748B;
  --color-text-muted:     #94A3B8;

  /* Semantic */
  --color-success:  #10B981;
  --color-warning:  #F59E0B;
  --color-danger:   #EF4444;
  --color-info:     #3B82F6;

  /* Layout */
  --sidebar-width:         260px;
  --sidebar-collapsed-w:   64px;
  --topbar-height:         64px;
  --content-padding:       24px;
  --card-radius:           12px;
  --border-radius:         8px;

  /* Shadows */
  --shadow-card:     0 1px 3px 0 rgb(0 0 0 / .06), 0 1px 2px -1px rgb(0 0 0 / .06);
  --shadow-elevated: 0 4px 6px -1px rgb(0 0 0 / .08);
}

[data-theme="dark"] {
  --color-surface:        #0F172A;
  --color-surface-card:   #1E293B;
  --color-border:         #334155;
  --color-text-primary:   #F1F5F9;
  --color-text-secondary: #94A3B8;
  --color-text-muted:     #64748B;
}
```

### Ant Design Theme Token Overrides (`antd-theme.ts`)

```typescript
export const antdTheme = {
  token: {
    colorPrimary:     '#4F46E5',
    colorSuccess:     '#10B981',
    colorWarning:     '#F59E0B',
    colorError:       '#EF4444',
    colorInfo:        '#3B82F6',
    borderRadius:      8,
    fontFamily:       "'Inter', sans-serif",
    colorBgContainer: 'var(--color-surface-card)',
    colorBorder:      'var(--color-border)',
    colorText:        'var(--color-text-primary)',
  },
  components: {
    Table:  { headerBg: 'var(--color-surface)' },
    Menu:   { itemBorderRadius: 8 },
    Button: { borderRadius: 8 },
    Card:   { borderRadius: 12 },
  }
};
```

### Typography
```
Font Family: Inter (Google Fonts)
├── Display: 700 weight, tracking-tight
├── Heading: 600 weight
├── Body:    400 weight, 15px / line-height 1.6
├── Label:   500 weight, 0.875rem
└── Caption: 400 weight, 0.75rem, text-muted
```

---

## State Management — Zustand

### Auth Store (`authStore.ts`)
```typescript
interface AuthStore {
  user:          AuthenticatedUser | null;
  accessToken:   string | null;
  role:          'platform_admin' | 'school_admin' | 'teacher' | 'student' | 'parent' | null;
  schoolId:      string | null;
  permissions:   string[];
  setAuth:       (payload: LoginResponse) => void;
  clearAuth:     () => void;
  hasPermission: (perm: string) => boolean;
}
```

### UI Store (`uiStore.ts`)
```typescript
interface UiStore {
  sidebarCollapsed: boolean;
  activeSchoolId:   string | null;
  toggleSidebar:    () => void;
  setActiveSchool:  (id: string) => void;
}
```

### Theme Store (`themeStore.ts`)
```typescript
interface ThemeStore {
  theme:       'light' | 'dark';
  toggleTheme: () => void;
}
```

---

## Server State — TanStack Query v5

All API data uses `useQuery` / `useMutation`. No hand-rolled loading/error state.

```typescript
// features/people/hooks.ts
export const useStudents = (params: StudentQueryParams) =>
  useQuery({
    queryKey: ['students', params],
    queryFn:  () => PeopleApi.getStudents(params),
    staleTime: 30_000,
  });

export const useCreateStudent = () =>
  useMutation({
    mutationFn: PeopleApi.createStudent,
    onSuccess:  () => {
      queryClient.invalidateQueries({ queryKey: ['students'] });
      toast.success('Student created successfully');
    },
    onError: (err) => toast.error(err.message),
  });
```

---

## Notifications — Sonner

```typescript
import { toast } from 'sonner';

toast.success('Student enrolled successfully');
toast.error('Failed to save — please try again');
toast.warning('Academic year is ending soon');
toast.info('Syncing data...');
toast.promise(savePromise, {
  loading: 'Saving...',
  success: 'Saved!',
  error:   'Could not save.'
});
```

`<Toaster />` placed once in `App.tsx`, positioned `bottom-right`, `richColors` enabled.

---

## Radix UI Usage

| Radix Primitive | Used For |
|---|---|
| `@radix-ui/react-dialog` | Modals: Create/Edit forms, Confirm delete |
| `@radix-ui/react-dropdown-menu` | User menu, row actions (⋮ button) |
| `@radix-ui/react-popover` | Inline confirm action popover |
| `@radix-ui/react-tooltip` | Icon button tooltips throughout |
| `@radix-ui/react-tabs` | Profile pages (Overview / Enrollments / Grades) |
| `@radix-ui/react-select` | Custom styled select where AntD doesn't fit |
| `@radix-ui/react-switch` | Active/Inactive toggle switches |
| `@radix-ui/react-avatar` | Fallback avatar with initials |
| `@radix-ui/react-separator` | Visual dividers in sidebars and cards |
| `@radix-ui/react-scroll-area` | Scrollable containers (sidebar, long forms) |

---

## HTTP Layer — Axios

```typescript
// lib/axios.ts
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  timeout: 15_000,
});

// Attach JWT
api.interceptors.request.use(config => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// 401 → refresh → retry
api.interceptors.response.use(
  res => res,
  async err => {
    if (err.response?.status === 401 && !err.config._retry) {
      err.config._retry = true;
      await AuthApi.refresh();
      return api(err.config);
    }
    return Promise.reject(err);
  }
);
```

---

## Role-Based Portals

| Role | JWT Claim | Route | Landing |
|---|---|---|---|
| Platform Admin | `is_platform_admin: true` | `/platform` | Analytics Dashboard |
| School Admin | `permission: schoolread` | `/school` | School Dashboard |
| Teacher | `permission: myclassesread` | `/teacher` | My Classes |
| Student | `permission: profileread` | `/student` | My Profile |
| Parent | `permission: childrenread` | `/parent` | My Children |

---

## Routing Structure

```typescript
<BrowserRouter>
  <Routes>
    <Route path="/auth/login" element={<LoginPage />} />

    {/* Platform Admin */}
    <Route element={<ProtectedRoute role="platform_admin" />}>
      <Route element={<AppLayout />}>
        <Route path="/platform"                  element={<PlatformDashboardPage />} />
        <Route path="/platform/schools"          element={<SchoolsListPage />} />
        <Route path="/platform/schools/new"      element={<CreateSchoolPage />} />
        <Route path="/platform/schools/:id"      element={<SchoolDetailPage />} />
        <Route path="/platform/schools/:id/edit" element={<EditSchoolPage />} />
        <Route path="/platform/admins/new"       element={<RegisterAdminPage />} />
      </Route>
    </Route>

    {/* School Admin */}
    <Route element={<ProtectedRoute role="school_admin" />}>
      <Route element={<AppLayout />}>
        <Route path="/school"                       element={<SchoolDashboardPage />} />
        <Route path="/academics/years"              element={<AcademicYearsPage />} />
        <Route path="/academics/years/:id/terms"    element={<TermsPage />} />
        <Route path="/academics/classrooms"         element={<ClassroomsPage />} />
        <Route path="/academics/grade-levels"       element={<GradeLevelsPage />} />
        <Route path="/academics/rooms"              element={<RoomsPage />} />
        <Route path="/people/students"              element={<StudentsListPage />} />
        <Route path="/people/students/:id"          element={<StudentProfilePage />} />
        <Route path="/people/teachers"              element={<TeachersListPage />} />
        <Route path="/people/teachers/:id"          element={<TeacherProfilePage />} />
        <Route path="/people/parents"               element={<ParentsListPage />} />
        <Route path="/people/parents/:id"           element={<ParentProfilePage />} />
        <Route path="/enrollment"                   element={<EnrollmentPage />} />
        <Route path="/enrollment/assign-teacher"    element={<AssignTeacherPage />} />
      </Route>
    </Route>

    {/* Self-Service Portals */}
    <Route element={<ProtectedRoute role="teacher" />}>
      <Route element={<AppLayout />}>
        <Route path="/teacher/classes" element={<MyClassesPage />} />
      </Route>
    </Route>

    <Route element={<ProtectedRoute role="student" />}>
      <Route element={<AppLayout />}>
        <Route path="/student/profile" element={<StudentSelfProfilePage />} />
      </Route>
    </Route>

    <Route element={<ProtectedRoute role="parent" />}>
      <Route element={<AppLayout />}>
        <Route path="/parent/children" element={<MyChildrenPage />} />
      </Route>
    </Route>

    <Route path="*" element={<Navigate to="/auth/login" replace />} />
  </Routes>
</BrowserRouter>
```

---

## Feature Modules & Pages

### Auth
| Page | Level | API |
|---|---|---|
| Login | `LoginPage` → `AuthTemplate` → `LoginForm` | `POST /api/auth/login` |
| Token Refresh (silent) | Axios interceptor | `POST /api/auth/refresh` |

### Platform Admin (`/platform`)
| Page | Key Organisms | API |
|---|---|---|
| Analytics Dashboard | `StatCard[]`, charts | `GET /api/schools/analytics` |
| Schools List | `DataTable`, `SearchInput` | `GET /api/schools` |
| School Detail | Info cards, `Tabs` | `GET /api/schools/{id}` |
| Create School | `SchoolForm` | `POST /api/schools` |
| Edit School | `SchoolForm` | `PUT /api/schools/{id}` |
| Register Admin | `RegisterAdminForm` | `POST /api/auth/register-school-admin` |

### School Admin (`/school`)
| Page | Key Organisms | API |
|---|---|---|
| School Dashboard | `StatCard[]`, quick-actions | `GET /api/schools/{id}/dashboard` |

### Academics (`/academics`)
| Page | Organisms | API |
|---|---|---|
| Academic Years | `DataTable` + `AcademicYearForm` in Dialog | `GET /api/academics/academic-years` |
| Terms | `DataTable` + `TermForm` in Dialog | `POST /api/academics/academic-years/{id}/terms` |
| Classrooms | `DataTable` + `ClassroomForm` + filters | `GET /api/academics/classrooms` |
| Grade Levels | `DataTable` + `GradeLevelForm` | `GET /api/academics/grade-levels` |
| Rooms | `DataTable` + `RoomForm` | `GET /api/academics/rooms` |

### People (`/people`)
| Page | Organisms | API |
|---|---|---|
| Students List | `DataTable` + avatar, search | `GET /api/people/students` |
| Student Profile | Tabs: Overview / Guardians | `GET /api/people/students/{id}` |
| Add/Edit Student | `StudentForm` | `POST/PUT /api/people/students` |
| Link Guardian | `LinkGuardianForm` in Dialog | `POST /api/people/students/{id}/guardians` |
| Teachers List | `DataTable` | `GET /api/people/teachers` |
| Teacher Profile | Tabs: Overview / Classes | `GET /api/people/teachers/{id}` |
| Add/Edit Teacher | `TeacherForm` | `POST/PUT /api/people/teachers` |
| Parents List | `DataTable` | `GET /api/people/parents` |
| Parent Profile | Tabs: Overview / Children | `GET /api/people/parents/{id}` |
| Add/Edit Parent | `ParentForm` | `POST/PUT /api/people/parents` |

### Enrollment (`/enrollment`)
| Page | Organisms | API |
|---|---|---|
| Enroll Student | `EnrollmentForm` | `POST /api/enrollment/students` |
| Assign Teacher | `AssignTeacherForm` | `POST /api/enrollment/teachers` |

### Self-Service Portals
| Portal | Page | API |
|---|---|---|
| Teacher | My Classes | `GET /api/people/me/classes` |
| Student | My Profile | `GET /api/people/me/student-profile` |
| Parent | My Children | `GET /api/people/me/children` |

---

## Sidebar Navigation (Role-Aware)

```
Platform Admin:
  ├── 📊 Analytics
  └── 🏫 Schools

School Admin:
  ├── 🏠 Dashboard
  ├── 📅 Academics
  │    ├── Academic Years
  │    ├── Classrooms
  │    ├── Grade Levels
  │    └── Rooms
  ├── 👥 People
  │    ├── Students
  │    ├── Teachers
  │    └── Parents
  └── 📋 Enrollment

Teacher:   📚 My Classes
Student:   👤 My Profile
Parent:    👨‍👩‍👧 My Children
```

---

## Install Commands

```bash
# Tailwind CSS v4
npm install tailwindcss @tailwindcss/vite

# Ant Design
npm install antd @ant-design/icons

# Radix UI
npm install @radix-ui/react-dialog @radix-ui/react-dropdown-menu \
            @radix-ui/react-popover @radix-ui/react-tooltip \
            @radix-ui/react-tabs @radix-ui/react-select \
            @radix-ui/react-switch @radix-ui/react-avatar \
            @radix-ui/react-separator @radix-ui/react-scroll-area

# State Management
npm install zustand
npm install @tanstack/react-query @tanstack/react-query-devtools

# Forms & Validation
npm install react-hook-form zod @hookform/resolvers

# Toasts
npm install sonner

# HTTP + Routing
npm install axios react-router-dom

# Utilities
npm install clsx tailwind-merge lucide-react dayjs

# Dev
npm install -D @types/node prettier eslint
```

### Vite Proxy Config
```typescript
// vite.config.ts
server: {
  proxy: {
    '/api': {
      target: 'http://localhost:5000',
      changeOrigin: true,
      secure: false,
    }
  }
}
```

---

## Pages Count Summary

| Module | Pages | Components (est.) | Status |
|---|---|---|---|
| Auth | 1 | 3 | 🔴 Not started |
| Platform Admin | 6 | 12 | 🔴 Not started |
| School Admin Shell | 1 | 5 | 🔴 Not started |
| Academics | 5 | 15 | 🔴 Not started |
| People | 12 | 25 | 🔴 Not started |
| Enrollment | 3 | 8 | 🔴 Not started |
| Teacher Portal | 1 | 4 | 🔴 Not started |
| Student Portal | 1 | 3 | 🔴 Not started |
| Parent Portal | 1 | 3 | 🔴 Not started |
| Shared atoms/molecules/organisms | — | ~35 | 🔴 Not started |
| **Total** | **~31 pages** | **~113 components** | **0% done** |

---

## Phased Implementation Roadmap

```
Phase 1 — Foundation (Week 1)
  ├── Tailwind v4 + CSS design tokens
  ├── Ant Design theme config
  ├── Axios instance + interceptors
  ├── TanStack Query client setup
  ├── Zustand stores (auth, ui, theme)
  ├── Sonner <Toaster /> setup
  ├── React Router + guards
  └── Atoms: Button, Badge, Avatar, Spinner, Typography

Phase 2 — Layout & Auth (Week 1-2)
  ├── AppLayout (Sidebar + TopBar)
  ├── AuthLayout (split-screen)
  ├── LoginPage + LoginForm organism
  ├── JWT decode → role redirect
  └── Token refresh interceptor

Phase 3 — Platform Admin (Week 2-3)
  ├── Analytics Dashboard (StatCards + charts)
  ├── Schools DataTable with search + pagination
  ├── Create/Edit School forms (Zod + RHF)
  └── Register School Admin form

Phase 4 — School Admin + Academics (Week 3-4)
  ├── School Dashboard KPI cards
  ├── Academic Years list + Create/Edit dialogs
  ├── Terms management (nested under AY)
  ├── Classrooms list + filters + CRUD
  ├── Grade Levels + Rooms CRUD
  └── DataTable organism (reusable)

Phase 5 — People Management (Week 5-6)
  ├── Students: List → Profile → Create/Edit
  ├── Guardian linking flow
  ├── Teachers: List → Profile → Create/Edit
  ├── Parents: List → Profile → Create/Edit
  └── Profile tabs organism (Radix Tabs)

Phase 6 — Enrollment (Week 7)
  ├── Enroll Student form
  ├── Assign Teacher form
  └── Enrollment overview

Phase 7 — Self-Service Portals (Week 7-8)
  ├── Teacher portal: My Classes
  ├── Student portal: My Profile
  └── Parent portal: My Children

Phase 8 — Future Features (After backend completes)
  ├── Attendance management module
  ├── Grade Book & Transcripts
  ├── Timetable / Schedule view
  ├── Announcements feed
  ├── Finance & Fees
  └── Reporting (PDF export)
```

---

## Open Questions

> **Q1**: Will charts/graphs be needed on dashboards from day 1?
> If yes → add **Recharts** or **Apache ECharts for React**.

> **Q2**: Is dark mode required from the start or a later enhancement?

> **Q3**: Deployment target? (Vercel, Azure Static Web Apps, Docker + Nginx, IIS)

> **Q4**: Should the app support Arabic/RTL layout?
> Ant Design has full RTL support via `direction="rtl"`.
