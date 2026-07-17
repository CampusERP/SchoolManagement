# School Management API

School Management API is a multi-tenant ASP.NET Core backend for managing schools, users, academics, and enrollment.

## Overview

The system supports multiple schools and separates their data securely. It provides JWT authentication and role-based permissions for:

- Super administrators
- School administrators
- Teachers
- Students
- Parents

## Main modules

- Authentication and user access
- School and campus management
- Academic years, grades, classrooms, rooms, and terms
- Students, teachers, parents, and guardians
- Student enrollment and teacher assignments
- School dashboards and platform analytics

## Architecture

The project follows Clean Architecture:

- `Api` handles HTTP endpoints.
- `Application` contains use cases and business workflows.
- `Domain` contains core entities and rules.
- `Infrastructure` handles databases, Identity, and external implementation details.

The API uses SQL Server, Entity Framework Core, ASP.NET Identity, JWT tokens, and Swagger in development.
