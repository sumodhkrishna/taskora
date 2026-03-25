export type TodoPriority = 1 | 2 | 3;
export type TodoStatusFilter = "all" | "pending" | "completed";

export interface TodoDto {
  id: number | string;
  title: string;
  description: string | null;
  priority: number | string;
  toBeCompletedByDateUtc: string | null;
  isCompleted: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  completedAtUtc: string | null;
}

export interface CreateTodoRequest {
  title: string;
  description?: string | null;
  priority?: TodoPriority;
  toBeCompletedByDateUtc?: string | null;
}

export interface UpdateTodoRequest {
  title: string;
  description?: string | null;
  priority?: TodoPriority;
  toBeCompletedByDateUtc?: string | null;
}

export interface TodoListQuery {
  status?: string;
  priority?: number;
  search?: string;
  dueBefore?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedResultDto<T> {
  items: T[];
  page: number | string;
  pageSize: number | string;
  totalCount: number | string;
  totalPages: number | string;
}