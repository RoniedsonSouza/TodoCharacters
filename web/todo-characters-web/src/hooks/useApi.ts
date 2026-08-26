import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../api/client';
import type { TaskStatus } from '../api/types';

export function useTasks() {
  return useQuery({ queryKey: ['tasks'], queryFn: api.getTasks });
}

export function useCreateTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: api.createTask,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['tasks'] }),
  });
}

export function useUpdateTaskStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: TaskStatus }) =>
      api.updateTaskStatus(id, status),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['tasks'] }),
  });
}

export function useUpdateTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      titulo,
      descricao,
    }: {
      id: string;
      titulo: string;
      descricao?: string | null;
    }) => api.updateTask(id, { titulo, descricao }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['tasks'] }),
  });
}

export function useCharacters(page: number) {
  return useQuery({ queryKey: ['characters', page], queryFn: () => api.getCharacters(page) });
}

export function useCharacter(id: number) {
  return useQuery({ queryKey: ['character', id], queryFn: () => api.getCharacter(id), enabled: id > 0 });
}
