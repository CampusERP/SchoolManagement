# School Management API Documentation

> **Base URL:** `http://localhost:5124/api/v1`
> **OpenAPI Spec:** `http://localhost:5124/openapi/v1.json`
> **API Docs UI:** `http://localhost:5124/scalar/v1`

---

## Authentication

All endpoints (except login/refresh) require a **JWT Bearer token** in the `Authorization` header:

```
Authorization: Bearer <token>
```

### Login

```
POST /api/v1/Auth/login
```

**Request Body:**

| Field | Type | Required | Description |
|---|---|---|---|
| `email` | string | Yes | User email address |
| `password` | string | Yes | User password |
| `schoolId` | uuid | No | School context for multi-tenant users |

**Response (200):**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "...",
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "school_admin",
    "schoolId": "uuid",
    "isPlatformAdmin": false,
    "permissions": ["schoolread", "studentread", "..."]
  }
}
```

### Refresh Token

```
POST /api/v1/Auth/refresh
```

| Field | Type | Required |
|---|---|---|
| `token` | string | Yes (refresh token) |

**Response (200):** Same shape as login response.

---

## Roles & Permissions

### Roles

| Role | Description |
|---|---|
| `SuperAdmin` | Full platform access |
| `SchoolAdmin` | School-level management |
| `Teacher` | Class management, grades |
| `Student` | Self-service portal |
| `Parent` | Children monitoring |

### Permissions

| Permission | Module |
|---|---|
| `schoolread`, `schoolcreate`, `schoolupdate`, `schooldashboard` | Schools |
| `platform.analytics` | Platform Analytics |
| `academicyearread`, `academicyearcreate`, `academicyearupdate` | Academic Years |
| `classroomread`, `classroomcreate`, `classroomupdate` | Classrooms |
| `gradelevelread`, `gradelevelupdate` | Grade Levels |
| `roomread`, `roomupdate` | Rooms |
| `studentread`, `studentcreate`, `studentupdate` | Students |
| `teacherread`, `teachercreate`, `teacherupdate` | Teachers |
| `parentread`, `parentcreate`, `parentupdate` | Parents |
| `profileread`, `childrenread`, `myclassesread` | Self-Service |
| `enrollmentcreate` | Enrollment |
| `schedulecreate` | Schedule |

---

## Endpoints

### Auth

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/v1/Auth/login` | Anonymous | Login |
| `POST` | `/api/v1/Auth/refresh` | Anonymous | Refresh token |
| `POST` | `/api/v1/Auth/register-school-admin` | `School.Create` | Register school admin |

---

### Schools

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/v1/Schools` | `School.Read` | List all schools (paginated) |
| `POST` | `/api/v1/Schools` | `School.Create` | Create a school |
| `GET` | `/api/v1/Schools/analytics` | `Platform.Analytics` | Platform analytics |
| `GET` | `/api/v1/Schools/{schoolId}` | `School.Read` | Get school details |
| `PUT` | `/api/v1/Schools/{schoolId}` | `School.Update` | Update school |
| `GET` | `/api/v1/Schools/{schoolId}/dashboard` | `School.Dashboard` | School dashboard data |

#### GET `/api/v1/Schools`

**Query Params:** `page` (int, default 1), `pageSize` (int, default 20)

**Response (200):**

```json
{
  "items": [
    {
      "id": "uuid",
      "name": "Greenfield Academy",
      "subdomainCode": "greenfield",
      "totalStudents": 450,
      "totalTeachers": 32
    }
  ],
  "totalCount": 5,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

#### POST `/api/v1/Schools`

**Request Body:**

| Field | Type | Required |
|---|---|---|
| `name` | string | Yes |
| `subdomainCode` | string | Yes |

**Response (201):** `{ "id": "uuid" }`

#### POST `/api/v1/Auth/register-school-admin`

**Request Body:**

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `firstName` | string | Yes |
| `lastName` | string | Yes |
| `email` | string | Yes |
| `password` | string | Yes |

**Response (201):** `{ "id": "uuid" }`

#### PUT `/api/v1/Schools/{schoolId}`

**Request Body:**

| Field | Type | Required |
|---|---|---|
| `name` | string | Yes |

---

### Academics

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/v1/Academics/academic-years` | `AcademicYear.Read` | List academic years |
| `POST` | `/api/v1/Academics/academic-years` | `AcademicYear.Create` | Create academic year |
| `PUT` | `/api/v1/Academics/academic-years/{academicYearId}` | `AcademicYear.Update` | Update academic year |
| `POST` | `/api/v1/Academics/academic-years/{academicYearId}/terms` | `AcademicYear.Create` | Create term |
| `GET` | `/api/v1/Academics/classrooms` | `ClassRoom.Read` | List classrooms |
| `POST` | `/api/v1/Academics/classrooms` | `ClassRoom.Create` | Create classroom |
| `PUT` | `/api/v1/Academics/classrooms/{classRoomId}` | `ClassRoom.Update` | Update classroom |
| `GET` | `/api/v1/Academics/grade-levels` | `GradeLevel.Read` | List grade levels |
| `POST` | `/api/v1/Academics/grade-levels` | `AcademicYear.Create` | Create grade level |
| `PUT` | `/api/v1/Academics/grade-levels/{gradeLevelId}` | `GradeLevel.Update` | Update grade level |
| `GET` | `/api/v1/Academics/rooms` | `Room.Read` | List rooms |
| `POST` | `/api/v1/Academics/rooms` | `ClassRoom.Create` | Create room |
| `PUT` | `/api/v1/Academics/rooms/{roomId}` | `Room.Update` | Update room |

#### POST `/api/v1/Academics/academic-years`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `name` | string | Yes |
| `startDate` | datetime (ISO 8601) | Yes |
| `endDate` | datetime (ISO 8601) | Yes |
| `setAsCurrent` | boolean | No (default: false) |

#### POST `/api/v1/Academics/academic-years/{academicYearId}/terms`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `name` | string | Yes |
| `sequence` | int | Yes |
| `startDate` | datetime (ISO 8601) | Yes |
| `endDate` | datetime (ISO 8601) | Yes |

#### GET `/api/v1/Academics/classrooms`

**Query Params:** `schoolId` (uuid, required), `academicYearId` (uuid, optional), `gradeLevelId` (uuid, optional)

#### POST `/api/v1/Academics/classrooms`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `gradeLevelId` | uuid | Yes |
| `academicYearId` | uuid | Yes |
| `name` | string | Yes |

#### POST `/api/v1/Academics/grade-levels`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `educationStageId` | uuid | Yes |
| `name` | string | Yes |
| `sequence` | int | Yes |

#### POST `/api/v1/Academics/rooms`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `name` | string | Yes |
| `capacity` | int | Yes |

---

### People

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/v1/People/students` | `Student.Read` | List students (paginated, filterable) |
| `GET` | `/api/v1/People/students/{studentId}` | `Student.Read` | Get student details |
| `POST` | `/api/v1/People/students` | `Student.Create` | Create student |
| `PUT` | `/api/v1/People/students/{studentId}` | `Student.Update` | Update student |
| `POST` | `/api/v1/People/students/{studentId}/guardians` | `Student.Create` | Link guardian to student |
| `GET` | `/api/v1/People/teachers` | `Teacher.Read` | List teachers (paginated) |
| `GET` | `/api/v1/People/teachers/{teacherId}` | `Teacher.Read` | Get teacher details |
| `POST` | `/api/v1/People/teachers` | `Teacher.Create` | Create teacher |
| `PUT` | `/api/v1/People/teachers/{teacherId}` | `Teacher.Update` | Update teacher |
| `GET` | `/api/v1/People/parents` | `Parent.Read` | List parents (paginated) |
| `GET` | `/api/v1/People/parents/{parentId}` | `Parent.Read` | Get parent details |
| `POST` | `/api/v1/People/parents` | `Parent.Create` | Create parent |
| `PUT` | `/api/v1/People/parents/{parentId}` | `Parent.Update` | Update parent |
| `GET` | `/api/v1/People/me/student-profile` | `Profile.Read` | Current student's profile |
| `GET` | `/api/v1/People/me/children` | `Children.Read` | Current parent's children |
| `GET` | `/api/v1/People/me/classes` | `MyClasses.Read` | Current teacher's classes |

#### GET `/api/v1/People/students`

**Query Params:**

| Param | Type | Required |
|---|---|---|
| `SchoolId` | uuid | Yes |
| `SearchTerm` | string | No |
| `GradeLevelId` | uuid | No |
| `ClassRoomId` | uuid | No |
| `Pagination.Page` | int | No |
| `Pagination.PageSize` | int | No |

#### POST `/api/v1/People/students`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `studentCode` | string | Yes |
| `firstName` | string | Yes |
| `lastName` | string | Yes |
| `dateOfBirth` | datetime (ISO 8601) | Yes |
| `email` | string | No |
| `password` | string | No |

#### POST `/api/v1/People/teachers`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `employeeCode` | string | Yes |
| `firstName` | string | Yes |
| `lastName` | string | Yes |
| `email` | string | Yes |
| `password` | string | Yes |

#### PUT `/api/v1/People/teachers/{teacherId}`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `employeeCode` | string | Yes |
| `firstName` | string | Yes |
| `lastName` | string | Yes |
| `employmentStatus` | int (enum) | Yes |

#### POST `/api/v1/People/parents`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `firstName` | string | Yes |
| `lastName` | string | Yes |
| `email` | string | Yes |
| `password` | string | Yes |

#### PUT `/api/v1/People/parents/{parentId}`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `firstName` | string | Yes |
| `lastName` | string | Yes |

#### POST `/api/v1/People/students/{studentId}/guardians`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `parentId` | uuid | Yes |
| `relationshipType` | int (enum) | Yes |
| `isPrimaryContact` | boolean | No (default: false) |
| `canViewGrades` | boolean | No (default: true) |
| `canViewBilling` | boolean | No (default: false) |

#### GET `/api/v1/People/me/classes`

**Query Params:** `schoolId` (uuid, required), `termId` (uuid, required)

---

### Enrollment

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/v1/Enrollment/students` | `Enrollment.Create` | Enroll student in classroom |
| `POST` | `/api/v1/Enrollment/teachers` | `Schedule.Create` | Assign teacher to class |

#### POST `/api/v1/Enrollment/students`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `studentId` | uuid | Yes |
| `classRoomId` | uuid | Yes |
| `academicYearId` | uuid | Yes |

#### POST `/api/v1/Enrollment/teachers`

| Field | Type | Required |
|---|---|---|
| `schoolId` | uuid | Yes |
| `teacherId` | uuid | Yes |
| `subjectId` | uuid | Yes |
| `classRoomId` | uuid | Yes |
| `termId` | uuid | Yes |
| `scheduleSlots` | ScheduleSlot[] | Yes |

**ScheduleSlot object:**

| Field | Type | Required |
|---|---|---|
| `dayOfWeek` | int (0=Sunday .. 6=Saturday) | Yes |
| `startTime` | string (HH:mm:ss) | Yes |
| `endTime` | string (HH:mm:ss) | Yes |
| `roomId` | uuid | Yes |

---

## Enums

### EmploymentStatus

| Value | Meaning |
|---|---|
| 0 | Active |
| 1 | OnLeave |
| 2 | Terminated |

### GuardianRelationshipType

| Value | Meaning |
|---|---|
| 0 | Father |
| 1 | Mother |
| 2 | Guardian |
| 3 | Other |

### DayOfWeek

| Value | Meaning |
|---|---|
| 0 | Sunday |
| 1 | Monday |
| 2 | Tuesday |
| 3 | Wednesday |
| 4 | Thursday |
| 5 | Friday |
| 6 | Saturday |

---

## Error Responses

All errors follow this format:

```json
{
  "status": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "Email": ["The Email field is required."],
    "Password": ["Password must be at least 8 characters."]
  }
}
```

| Status | Meaning |
|---|---|
| 400 | Validation error / bad request |
| 401 | Unauthorized (missing or invalid token) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Resource not found |
| 500 | Internal server error |

---

## Quick Start

### 1. Get a token

```bash
curl -X POST http://localhost:5124/api/v1/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@campuserp.com","password":"AdminPassword123!"}'
```

### 2. Use the token

```bash
curl http://localhost:5124/api/v1/Schools \
  -H "Authorization: Bearer <your_token_here>"
```

### 3. Create a school

```bash
curl -X POST http://localhost:5124/api/v1/Schools \
  -H "Authorization: Bearer <your_token_here>" \
  -H "Content-Type: application/json" \
  -d '{"name":"My School","subdomainCode":"myschool"}'
```
