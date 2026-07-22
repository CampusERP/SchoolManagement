export interface AcademicYearDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  status?: string;
  terms: TermDto[];
}

export interface TermDto {
  id: string;
  name: string;
  sequence: number;
  startDate: string;
  endDate: string;
}

export interface CreateAcademicYearCommand {
  schoolId: string;
  name: string;
  startDate: string;
  endDate: string;
  setAsCurrent: boolean;
}

export interface UpdateAcademicYearCommand {
  schoolId: string;
  name: string;
  startDate: string;
  endDate: string;
}

export interface CreateTermCommand {
  schoolId: string;
  academicYearId: string;
  name: string;
  sequence: number;
  startDate: string;
  endDate: string;
}

export interface ClassRoomDetailDto {
  id: string;
  name: string;
  gradeLevelName: string;
  gradeLevel?: string;
  academicYearName: string;
  academicYear?: string;
  studentCount: number;
  enrolledStudents?: number;
  teachingAssignmentCount?: number;
}

export interface CreateClassRoomCommand {
  schoolId: string;
  gradeLevelId: string;
  academicYearId: string;
  name: string;
}

export interface UpdateClassRoomCommand {
  schoolId: string;
  name: string;
}

export interface GradeLevelDto {
  id: string;
  name: string;
  sequence: number;
  educationStageId: string;
  educationStage?: string;
  classRoomCount?: number;
}

export interface CreateGradeLevelCommand {
  schoolId: string;
  educationStageId: string;
  name: string;
  sequence: number;
}

export interface UpdateGradeLevelCommand {
  schoolId: string;
  name: string;
  sequence: number;
}

export interface EducationStageDto {
  id: string;
  name: string;
}

export interface CreateEducationStageCommand {
  name: string;
}

export interface UpdateEducationStageCommand {
  name: string;
}

export interface RoomDto {
  id: string;
  name: string;
  capacity: number;
}

export interface CreateRoomCommand {
  schoolId: string;
  name: string;
  capacity: number;
}

export interface UpdateRoomCommand {
  schoolId: string;
  name: string;
  capacity: number;
}

export interface RosterStudentDto {
  enrollmentId: string;
  studentId: string;
  studentCode: string;
  firstName: string;
  lastName: string;
  enrollmentStatus: string;
}

export interface ClassRoomTeachingAssignmentDto {
  id: string;
  subjectName: string;
  subjectCode: string;
  teacherFirstName: string;
  teacherLastName: string;
  termName: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
