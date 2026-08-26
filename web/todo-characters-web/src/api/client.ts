import type {
  ApiError,
  CharacterDetailDto,
  CharacterPageDto,
  TaskDto,
  TaskStatus,
  TasksGroupedDto,
} from './types';

const API_BASE = import.meta.env.VITE_API_URL ?? '/api';

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    headers: { 'Content-Type': 'application/json', ...init?.headers },
    ...init,
  });

  if (!response.ok) {
    const error = (await response.json().catch(() => null)) as ApiError | null;
    throw new Error(error?.message ?? `Erro HTTP ${response.status}`);
  }

  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export const api = {
  getTasks: () => request<TasksGroupedDto>('/tasks'),
  createTask: (body: { titulo: string; descricao?: string; tarefaPaiId?: string | null }) =>
    request<TaskDto>('/tasks', { method: 'POST', body: JSON.stringify(body) }),
  updateTaskStatus: (id: string, status: TaskStatus) =>
    request<TaskDto>(`/tasks/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ status }),
    }),
  updateTask: (id: string, body: { titulo: string; descricao?: string | null }) =>
    request<TaskDto>(`/tasks/${id}`, {
      method: 'PUT',
      body: JSON.stringify(body),
    }),
  getCharacters: (page: number) => request<CharacterPageDto>(`/characters?page=${page}`),
  getCharacter: (id: number) => request<CharacterDetailDto>(`/characters/${id}`),
};
