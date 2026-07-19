# School Management Frontend — Cleanup & Bug Fix Plan

Goal: Fix 9 reported issues and ensure `npm run build` (tsc -b && vite build) passes with **zero** TypeScript errors.
Project root: `D:\Projects\SchoolManagement\frontend`.

## Context / Findings
- `SchoolDashboardData` (`src/types/dashboard.types.ts:36`) has: `schoolName, totalStudents, totalTeachers, totalParents, totalClassRooms, activeEnrollments, currentAcademicYear, recentStudents, upcomingEvents, announcements, attendanceSummary`. It does **NOT** have `totalClasses`, `todayAttendance`, or `pendingEnrollments`.
- `AttendanceSummary` = `{ present, absent, late, excused }`.
- There is **no** `src/components/atoms/Input.tsx` (only `Button.tsx` exists in atoms). Must create it.
- Custom `Button` (`src/components/atoms/Button.tsx`) wraps antd `Button`, omits `type`/`variant` from props, adds `variant`, and spreads the rest into antd. So `type="link"`, `size`, `icon`, `htmlType`, `block`, `loading`, `onClick` all pass through. Default `variant="primary"`.
- `DashboardTemplate` `actions` prop accepts a node (used for the Create School button).

## Fixes

### 1. `src/pages/school/SchoolDashboardPage.tsx` (lines 44-51)
Replace the broken stat cards with valid fields:
- "Classes" → `data.totalClassRooms` (rename label to "Classrooms" for clarity), icon `BookOpen`.
- "Today's Attendance" → compute percentage from `attendanceSummary`:
  `const { present, absent, late, excused } = data.attendanceSummary;`
  `const total = present + absent + late + excused;`
  `const attendancePct = total > 0 ? Math.round((present / total) * 100) : 0;`
  value = ``${attendancePct}%``.
- "Pending Enrollments" → `data.activeEnrollments`.
- Keep Students/Teachers/Parents cards unchanged (valid fields).

### 2. Dead Imports
- `src/components/molecules/UserMenu.tsx:10` — remove `import { cn } from "@/lib/utils";` (cn unused in file).
- `src/components/organisms/Sidebar.tsx:12` — remove `Menu,` from the lucide-react import list.
- `src/router/routes.tsx:15-17` — remove the unused `placeholder` helper function.

### 3. Axios Interceptor — `src/lib/axios.ts:25`
Change `await axios.post("/api/v1/auth/refresh", { token: rt })` → `await api.post("/auth/refresh", { token: rt })` so the refresh call uses `baseURL` (`/api/v1`) and timeout. (`api` is in scope.)

### 4. Mock Data Overriding Real API — `src/features/dashboard/api.ts`
- `getPlatformDashboard`: keep real `api.get("/schools/analytics")` success path; mock only in `.catch()`. **Remove** `console.error` at line 80.
- `getSchoolDashboard`: real success path stays; mock only in `.catch()`. **Remove** `console.error` at line 106.
- `getTeacherDashboard`: implement real call with fallback mock + `// TODO: Replace with real API call`:
  ```
  getTeacherDashboard: async () => {
    try {
      const response = await api.get("/teacher/dashboard");
      return response.data as TeacherDashboardData;
    } catch {
      // TODO: Replace with real API call
      return { totalClasses: 3, ... } as TeacherDashboardData;
    }
  },
  ```
- `getSchools`: add `search: params?.search` to the request `params` object (currently only page/pageSize are sent). Search filtering still applies in the `.catch()` fallback. **Remove** `console.error` at line 161.

### 5. Eradicate `any`
- `src/features/dashboard/api.ts:146` — add an interface for the school item and type `map((item: SchoolApiItem) => ...)`:
  ```
  interface SchoolApiItem {
    id: string;
    name: string;
    subdomainCode: string;
    status: string;
    totalStudents: number;
    totalTeachers: number;
    createdAtUtc: string;
  }
  ```
- `src/pages/auth/LoginPage.tsx:41` — change `catch (err: any)` → `catch (err: unknown)` and narrow:
  ```
  const message =
    err && typeof err === "object" && "response" in err
      ? ((err as { response?: { data?: { error?: string; message?: string } } }).response?.data?.error ||
         (err as { response?: { data?: { message?: string } } }).response?.data?.message)
      : err instanceof Error ? err.message : "Login failed. Please try again.";
  ```

### 6. Non-Functional Buttons — `src/pages/platform/SchoolsPage.tsx`
- Add `import { useNavigate } from "react-router-dom";` and `const navigate = useNavigate();`.
- "Create School" action button `onClick={() => navigate("/platform/schools/new")}`.
- "View" → `onClick={() => navigate(`/platform/schools/${record.id}`)}` (use column `render: (_: unknown, record: School) => ...`).
- "Edit" → `navigate(`/platform/schools/${record.id}/edit`)`.

### 7. Sidebar `useEffect` Dependency — `src/components/organisms/Sidebar.tsx`
- Wrap navigation in `useMemo` so the value is stable per role:
  `const navigation = useMemo(() => getNavigationForRole(user?.role), [user?.role]);`
  Add `useMemo` to the React import. The effect already uses `[user?.role]` as its dependency (correct), so this stabilizes the referenced `navigation` array.

### 8. Design System Consistency
- Create `src/components/atoms/Input.tsx` wrapping antd `Input` (and `Input.Password`) so LoginPage/SchoolsPage can use it. Pattern mirrors `Button.tsx`:
  - Default export `Input` = antd `Input` with `cn(...)` className passthrough via `...props`. Support `prefix`, `allowClear`, `value`, `onChange`, `placeholder`, `size`, `className`.
  - Named export `InputPassword` = antd `Input.Password` (used by LoginPage).
- `src/pages/auth/LoginPage.tsx:6` — replace `import { Form, Input, Button } from "antd";` with `import { Form } from "antd";` + `import Button from "@/components/atoms/Button";` + `import { Input, InputPassword } from "@/components/atoms/Input";`. Replace `<Input ...>` with atom `<Input ...>`, `<Input.Password ...>` with `<InputPassword ...>`, and `<Button ...>` (Sign In) with atom `<Button htmlType="submit" ...>` keeping `loading`/`block`.
  - Note: `Form` from antd remains (LoginPage relies on antd Form layout/validationStatus); instruction only targets Button + Input.
- `src/pages/platform/SchoolsPage.tsx:5` — replace `Button, Input` from `antd` with `@/components/atoms/Button` and `@/components/atoms/Input`. Keep `Table` from antd. The "View"/"Edit" links become `<Button type="link" size="small" onClick={...}>` using the atom (supports `type`/`size`/`onClick`). Search `Input` uses the atom.

### 9. Duplicate Route — `src/router/routes.tsx:109`
- Change `{ path: "classes", element: <TeacherDashboardPage /> }` to a descriptive placeholder:
  `import PlaceholderPage` is already imported (line 13).
  `{ path: "classes", element: <PlaceholderPage title="My Classes" description="My Classes — Detailed view coming soon" /> }`.

## Constraints
- Do NOT modify CSS/margins/padding, architecture, or remove business logic.
- No unnecessary comments; only the specified `// TODO` in `getTeacherDashboard`.

## Verification
1. `cd D:\Projects\SchoolManagement\frontend && npm run build`
2. Confirm `tsc -b` produces zero errors and `vite build` succeeds.
3. If TypeScript still reports errors, fix iteratively and re-run build until clean.
