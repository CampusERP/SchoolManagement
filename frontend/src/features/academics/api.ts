import api from "@/lib/axios";
import type {
  AcademicYearDto,
  CreateAcademicYearCommand,
  UpdateAcademicYearCommand,
  CreateTermCommand,
  ClassRoomDetailDto,
  CreateClassRoomCommand,
  UpdateClassRoomCommand,
  GradeLevelDto,
  CreateGradeLevelCommand,
  UpdateGradeLevelCommand,
  RoomDto,
  CreateRoomCommand,
  UpdateRoomCommand,
  RosterStudentDto,
  ClassRoomTeachingAssignmentDto,
  PagedResult,
} from "@/types/academic.types";

export interface SubjectDto {
  id: string;
  code: string;
  name: string;
  description?: string;
}

export interface GetClassroomsParams {
  schoolId: string;
  academicYearId?: string;
  gradeLevelId?: string;
}

export const AcademicsApi = {
  // ── Academic Years ──────────────────────────────────────────────
  getAcademicYears: (schoolId: string) =>
    api
      .get<AcademicYearDto[]>("/academics/academic-years", {
        params: { schoolId },
      })
      .then((r) => r.data),

  createAcademicYear: (data: CreateAcademicYearCommand) =>
    api
      .post<{ id: string }>("/academics/academic-years", data)
      .then((r) => r.data),

  updateAcademicYear: (id: string, data: UpdateAcademicYearCommand) =>
    api.put(`/academics/academic-years/${id}`, data).then((r) => r.data),

  createTerm: (data: CreateTermCommand) =>
    api
      .post<{ id: string }>(
        `/academics/academic-years/${data.academicYearId}/terms`,
        data
      )
      .then((r) => r.data),

  // ── Classrooms ──────────────────────────────────────────────────
  getClassrooms: (params: GetClassroomsParams) =>
    api
      .get<ClassRoomDetailDto[]>("/academics/classrooms", {
        params: {
          schoolId: params.schoolId,
          academicYearId: params.academicYearId || undefined,
          gradeLevelId: params.gradeLevelId || undefined,
        },
      })
      .then((r) => r.data),

  createClassroom: (data: CreateClassRoomCommand) =>
    api.post<{ id: string }>("/academics/classrooms", data).then((r) => r.data),

  updateClassroom: (id: string, data: UpdateClassRoomCommand) =>
    api.put(`/academics/classrooms/${id}`, data).then((r) => r.data),

  getClassroomRoster: (
    classRoomId: string,
    schoolId: string,
    page = 1,
    pageSize = 50
  ) =>
    api
      .get<PagedResult<RosterStudentDto>>(
        `/academics/classrooms/${classRoomId}/roster`,
        { params: { schoolId, page, pageSize } }
      )
      .then((r) => r.data),

  getClassroomTeachingAssignments: (classRoomId: string, schoolId: string) =>
    api
      .get<ClassRoomTeachingAssignmentDto[]>(
        `/academics/classrooms/${classRoomId}/teaching-assignments`,
        { params: { schoolId } }
      )
      .then((r) => r.data),

  withdrawStudent: (enrollmentId: string, schoolId: string) =>
    api
      .post(`/people/students/enrollments/${enrollmentId}/withdraw`, null, {
        params: { schoolId },
      })
      .then((r) => r.data),

  // ── Grade Levels ────────────────────────────────────────────────
  getGradeLevels: (schoolId: string) =>
    api
      .get<GradeLevelDto[]>("/academics/grade-levels", {
        params: { schoolId },
      })
      .then((r) => r.data),

  createGradeLevel: (data: CreateGradeLevelCommand) =>
    api
      .post<{ id: string }>("/academics/grade-levels", data)
      .then((r) => r.data),

  updateGradeLevel: (id: string, data: UpdateGradeLevelCommand) =>
    api.put(`/academics/grade-levels/${id}`, data).then((r) => r.data),

  // ── Rooms ───────────────────────────────────────────────────────
  getRooms: (schoolId: string) =>
    api
      .get<RoomDto[]>("/academics/rooms", { params: { schoolId } })
      .then((r) => r.data),

  createRoom: (data: CreateRoomCommand) =>
    api.post<{ id: string }>("/academics/rooms", data).then((r) => r.data),

  updateRoom: (id: string, data: UpdateRoomCommand) =>
    api.put(`/academics/rooms/${id}`, data).then((r) => r.data),

  // ── Subjects ─────────────────────────────────────────────────────
  getSubjects: (gradeLevelId?: string) =>
    api
      .get<SubjectDto[]>("/subjects", {
        params: { gradeLevelId: gradeLevelId || undefined },
      })
      .then((r) => r.data),
};
