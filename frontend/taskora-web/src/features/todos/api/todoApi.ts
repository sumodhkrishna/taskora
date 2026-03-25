import { apiClient } from "../../../services/apiClient";
import type {
  CreateTodoRequest,
  PagedResultDto,
  TodoDto,
  TodoListQuery,
  UpdateTodoRequest,
} from "../types/todo";

export async function getTodos(
  query: TodoListQuery
): Promise<PagedResultDto<TodoDto>> {
  const response = await apiClient.get<PagedResultDto<TodoDto>>("/api/Todos", {
    params: {
      Status: query.status,
      Priority: query.priority,
      Search: query.search,
      DueBefore: query.dueBefore,
      Page: query.page,
      PageSize: query.pageSize,
    },
  });

  return response.data;
}

export async function createTodo(request: CreateTodoRequest): Promise<TodoDto> {
  const response = await apiClient.post<TodoDto>("/api/Todos", request);
  return response.data;
}

export async function updateTodo(
  id: number | string,
  request: UpdateTodoRequest
): Promise<TodoDto> {
  const response = await apiClient.put<TodoDto>(`/api/Todos/${id}`, request);
  return response.data;
}

export async function completeTodo(id: number | string): Promise<TodoDto> {
  const response = await apiClient.patch<TodoDto>(`/api/Todos/${id}/complete`);
  return response.data;
}

export async function reopenTodo(id: number | string): Promise<TodoDto> {
  const response = await apiClient.patch<TodoDto>(`/api/Todos/${id}/reopen`);
  return response.data;
}

export async function deleteTodo(id: number | string): Promise<void> {
  await apiClient.delete(`/api/Todos/${id}`);
}