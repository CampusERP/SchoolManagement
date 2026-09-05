# CampusERP - Multi-Tenant School Management SaaS

A full-stack school-management platform that enables multiple schools to manage academic operations through a shared, tenant-isolated system.

**Live Demo:** https://campuserpt.vercel.app/  
**Frontend:** React, TypeScript, Vite  
**Backend:** ASP.NET Core Web API, C#, SQL Server

## Features

### Platform administration
- Create, manage, suspend, and reactivate schools.
- Register school administrators.
- View platform-level analytics and subscription information.

### School administration
- Manage academic years, education stages, grade levels, classrooms, rooms, and subjects.
- Manage students, teachers, parents, and student guardians.
- Enroll students and assign teachers to classes.
- Apply role- and permission-based access control.

### Portals and operations
- Teacher dashboard, classes, attendance, grades, schedule, and exams.
- Protected student and parent portal routes.
- API support for assignments, report cards, notifications, attendance, and billing.

## Architecture

```text
Api -> Application -> Domain
       ^
Infrastructure
```

- **API:** ASP.NET Core REST API with thin controllers.
- **Application:** CQRS commands and queries with MediatR.
- **Domain:** Core entities and business rules.
- **Infrastructure:** EF Core, SQL Server, ASP.NET Identity, JWT, repositories, and background processing.

## Technical highlights

- Multi-tenancy using `SchoolId` and EF Core global query filters.
- JWT authentication with refresh-token handling.
- Role- and permission-based authorization.
- CQRS and MediatR pipeline behaviors.
- FluentValidation for request validation.
- Repository and Unit of Work patterns.
- Outbox pattern for reliable asynchronous side effects.
- React Query for server state and Zustand for client/auth state.
- React Hook Form and Zod for typed form handling.
- Axios interceptors for attaching and refreshing access tokens.

## Tech stack

| Area | Technologies |
| --- | --- |
| Backend | C#, ASP.NET Core, MediatR, FluentValidation |
| Database | SQL Server, Entity Framework Core |
| Security | ASP.NET Identity, JWT, Refresh Tokens, RBAC |
| Frontend | React, TypeScript, Vite, Tailwind CSS, Ant Design |
| State and forms | TanStack Query, Zustand, React Hook Form, Zod |
| API | REST, OpenAPI/Scalar |

## Local setup

### Prerequisites
- .NET SDK 10
- Node.js 20+
- SQL Server

### Run the backend

```bash
cd Api
dotnet restore
dotnet run
```

### Run the frontend

```bash
cd Frontend
npm install
npm run dev
```

Configure connection strings, JWT settings, and allowed frontend origins through local configuration or environment variables. Never commit production secrets.

## Demo access

> Use only deliberately seeded, restricted demo accounts. Never expose production or high-privilege administrator accounts.

| Role | Email | Password |
| --- | --- | --- |
| Super Administrator | `superadmin@demo-school.school` | `Demo@123456` |
| School Administrator | `schooladmin@demo-school.school` | `Demo@123456` |
| Student | `student@demo-school.school` | `Demo@123456` |
| Teacher | `teacher@demo-school.school` | `Demo@123456` |
| Parent | `parent@demo-school.school` | `Demo@123456` |

## Project status

The core platform, school administration, academic setup, people management, enrollment, and teacher workflows are implemented. Some portal experiences and advanced modules are still being expanded.

## Author

Mazen Essam  
GitHub: https://github.com/mazenessam2000

Ahmed Samir
GitHub: https://github.com/ahmedsamir46
