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
  CurriculumSubjectDto,
  SubjectBookInput,
} from "@/types/academic.types";

export interface SubjectDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  isSchoolSpecific?: boolean;
}

export interface SubjectInput {
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

  deleteAcademicYear: (id: string, schoolId: string) =>
    api.delete(`/academics/academic-years/${id}`, { params: { schoolId } }).then((r) => r.data),

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
  getSubjects: (schoolId: string, gradeLevelId?: string) =>
    api
      .get<SubjectDto[]>("/subjects", {
        params: { schoolId, gradeLevelId: gradeLevelId || undefined },
      })
      .then((r) => r.data),

  getGradeCurriculum: (schoolId: string, gradeLevelId: string) =>
    api.get<CurriculumSubjectDto[]>(`/subjects/curriculum/${gradeLevelId}`, { params: { schoolId } }).then((r) => r.data),

  addCurriculumSubject: (schoolId: string, gradeLevelId: string, subjectId: string) =>
    api.post<{ id: string }>("/subjects/curriculum", { schoolId, gradeLevelId, subjectId }).then((r) => r.data),

  createSchoolSubject: (schoolId: string, gradeLevelId: string, data: SubjectInput) =>
    api.post<{ id: string }>("/subjects/school-subjects", { schoolId, gradeLevelId, ...data }).then((r) => r.data),

  updateSchoolSubject: (schoolId: string, curriculumSubjectId: string, data: SubjectInput) =>
    api.put(`/subjects/curriculum/${curriculumSubjectId}/subject`, { schoolId, ...data }).then((r) => r.data),

  deleteSchoolSubject: (schoolId: string, subjectId: string) =>
    api.delete(`/subjects/school-subjects/${subjectId}`, { params: { schoolId } }).then((r) => r.data),

  removeCurriculumSubject: (schoolId: string, curriculumSubjectId: string) =>
    api.delete(`/subjects/curriculum/${curriculumSubjectId}`, { params: { schoolId } }).then((r) => r.data),

  createSubjectBook: (schoolId: string, curriculumSubjectId: string, data: SubjectBookInput) =>
    api.post<{ id: string }>(`/subjects/curriculum/${curriculumSubjectId}/books`, { schoolId, ...data }).then((r) => r.data),

  updateSubjectBook: (schoolId: string, curriculumSubjectId: string, bookId: string, data: SubjectBookInput) =>
    api.put(`/subjects/curriculum/${curriculumSubjectId}/books/${bookId}`, { schoolId, ...data }).then((r) => r.data),

  deleteSubjectBook: (schoolId: string, curriculumSubjectId: string, bookId: string) =>
    api.delete(`/subjects/curriculum/${curriculumSubjectId}/books/${bookId}`, { params: { schoolId } }).then((r) => r.data),

  uploadBookPdf: (schoolId: string, curriculumSubjectId: string, bookId: string, file: File) => {
    const form = new FormData();
    form.append("file", file);
    return api.post(`/subjects/curriculum/${curriculumSubjectId}/books/${bookId}/documents`, form, { params: { schoolId }, headers: { "Content-Type": "multipart/form-data" } }).then((r) => r.data);
  },
};
