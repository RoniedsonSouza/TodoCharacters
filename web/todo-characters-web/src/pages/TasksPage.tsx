import { type FormEvent, useMemo, useState } from 'react';
import type { TaskDto, TaskStatus } from '../api/types';
import { useCreateTask, useTasks, useUpdateTask, useUpdateTaskStatus } from '../hooks/useApi';
import { Layout, TaskCard } from '../components/TaskCard';

type StatusFilter = 'Todos' | TaskStatus;

const STATUS_FILTERS: StatusFilter[] = [
  'Todos',
  'Não iniciado',
  'Em progresso',
  'Concluído',
];

function matchesStatus(task: TaskDto, filter: StatusFilter): boolean {
  return filter === 'Todos' || task.status === filter;
}

export function TasksPage() {
  const { data, isLoading, error } = useTasks();
  const createTask = useCreateTask();
  const updateTask = useUpdateTask();
  const updateStatus = useUpdateTaskStatus();
  const [titulo, setTitulo] = useState('');
  const [statusError, setStatusError] = useState<string | null>(null);
  const [draftParentId, setDraftParentId] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('Todos');

  const filteredAtivas = useMemo(
    () => (data?.ativas ?? []).filter((task) => matchesStatus(task, statusFilter)),
    [data?.ativas, statusFilter],
  );

  const filteredConcluidas = useMemo(
    () => (data?.concluidas ?? []).filter((task) => matchesStatus(task, statusFilter)),
    [data?.concluidas, statusFilter],
  );

  const showAtivas = statusFilter !== 'Concluído';
  const showConcluidas = statusFilter === 'Todos' || statusFilter === 'Concluído';

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault();
    await createTask.mutateAsync({ titulo });
    setTitulo('');
  };

  const handleSaveDraft = async (parentId: string, draftTitulo: string) => {
    await createTask.mutateAsync({
      titulo: draftTitulo,
      tarefaPaiId: parentId,
    });
    setDraftParentId(null);
  };

  const handleStatusChange = async (id: string, status: TaskStatus) => {
    setStatusError(null);
    try {
      await updateStatus.mutateAsync({ id, status });
    } catch (err) {
      setStatusError(err instanceof Error ? err.message : 'Erro ao alterar status');
    }
  };

  const handleTitleChange = async (
    id: string,
    titulo: string,
    descricao?: string | null,
  ) => {
    await updateTask.mutateAsync({ id, titulo, descricao: descricao ?? null });
  };

  if (isLoading) return <Layout><p>Carregando tarefas...</p></Layout>;
  if (error) return <Layout><p role="alert">Erro: {(error as Error).message}</p></Layout>;

  return (
    <Layout>
      <section aria-labelledby="nova-tarefa">
        <h2 id="nova-tarefa">Nova tarefa</h2>
        <form onSubmit={handleCreate}>
          <input
            value={titulo}
            onChange={(e) => setTitulo(e.target.value)}
            placeholder="Título (mín. 3 caracteres)"
            minLength={3}
            maxLength={120}
            required
            aria-label="Título da tarefa"
          />
          <button type="submit">Criar</button>
        </form>
      </section>

      <section aria-labelledby="filtro-status" className="status-filter">
        <h2 id="filtro-status">Filtrar por status</h2>
        <div role="group" aria-label="Filtro de status das tarefas">
          {STATUS_FILTERS.map((filter) => (
            <button
              key={filter}
              type="button"
              className={statusFilter === filter ? 'filter-active' : undefined}
              aria-pressed={statusFilter === filter}
              onClick={() => setStatusFilter(filter)}
            >
              {filter}
            </button>
          ))}
        </div>
      </section>

      {showAtivas && (
        <section aria-labelledby="ativas">
          <h2 id="ativas">Fila principal</h2>
          {filteredAtivas.length === 0 && <p>Nenhuma tarefa ativa neste filtro.</p>}
          {filteredAtivas.map((task) => (
            <TaskCard
              key={task.id}
              task={task}
              onStatusChange={handleStatusChange}
              onAddSubtask={setDraftParentId}
              onTitleChange={handleTitleChange}
              draftParentId={draftParentId}
              onSaveDraft={handleSaveDraft}
              onDiscardDraft={() => setDraftParentId(null)}
              isSavingDraft={createTask.isPending}
              error={statusError}
            />
          ))}
        </section>
      )}

      {showConcluidas && (
        <section aria-labelledby="concluidas">
          <h2 id="concluidas">Concluídas</h2>
          {filteredConcluidas.length === 0 && <p>Nenhuma tarefa concluída neste filtro.</p>}
          {filteredConcluidas.map((task) => (
            <TaskCard
              key={task.id}
              task={task}
              onStatusChange={handleStatusChange}
              onAddSubtask={setDraftParentId}
              onTitleChange={handleTitleChange}
              draftParentId={draftParentId}
              onSaveDraft={handleSaveDraft}
              onDiscardDraft={() => setDraftParentId(null)}
              isSavingDraft={createTask.isPending}
            />
          ))}
        </section>
      )}
    </Layout>
  );
}
