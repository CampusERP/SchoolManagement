export interface ParentListDto {
  id: string;
  firstName: string;
  lastName: string;
  // Additional field returned by the backend
  childrenCount?: number;
}

export interface ParentDetailDto {
  id: string;
  firstName: string;
  lastName: string;
  children: ChildSummary[];
}

export interface ChildSummary {
  id: string;
  studentId?: string;
  firstName: string;
  lastName: string;
  studentCode: string;
  relationshipType?: string;
  currentClass?: string | null;
}

export interface CreateParentCommand {
  schoolId: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface UpdateParentCommand {
  schoolId: string;
  firstName: string;
  lastName: string;
}
