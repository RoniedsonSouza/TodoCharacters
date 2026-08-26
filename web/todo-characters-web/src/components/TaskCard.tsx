import { NavLink } from 'react-router-dom';
import {
  useEffect,
  useRef,
  useState,
  type FormEvent,
  type FocusEvent,
} from 'react';
import type { TaskDto, TaskStatus } from '../api/types';

const statuses: TaskStatus[] = ['Não iniciado', 'Em progresso', 'Concluído'];
const MIN_TITLE = 3;
const MAX_TITLE = 120;

export function isValidTaskTitle(value: string): boolean {
  const t = value.trim();
  return t.length >= MIN_TITLE && t.length <= MAX_TITLE;
}

interface Props {
  task: TaskDto;
  isSubtask?: boolean;
  onStatusChange: (id: string, status: TaskStatus) => void;
  onAddSubtask: (parentId: string) => void;
  onTitleChange?: (id: string, titulo: string, descricao?: string | null) => Promise<void>;
  draftParentId?: string | null;
  onSaveDraft?: (parentId: string, titulo: string) => Promise<void>;
  onDiscardDraft?: () => void;
  isSavingDraft?: boolean;
  error?: string | null;
}

export function TaskCard({
  task,
  isSubtask = false,
  onStatusChange,
  onAddSubtask,
  onTitleChange,
  draftParentId = null,
  onSaveDraft,
  onDiscardDraft,
  isSavingDraft = false,
  error,
}: Props) {
  const showDraft = !isSubtask && draftParentId === task.id && onSaveDraft && onDiscardDraft;
  const [editing, setEditing] = useState(false);
  const [draftTitle, setDraftTitle] = useState(task.titulo);
  const [titleError, setTitleError] = useState<string | null>(null);
  const [isSavingTitle, setIsSavingTitle] = useState(false);
  const titleInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (editing) {
      titleInputRef.current?.focus();
    }
  }, [editing]);

  const cancelEdit = () => {
    setDraftTitle(task.titulo);
    setTitleError(null);
    setEditing(false);
  };

  const saveEdit = async () => {
    if (!onTitleChange || isSavingTitle) return;
    const next = draftTitle.trim();
    if (next === task.titulo) {
      setEditing(false);
      setTitleError(null);
      return;
    }
    if (!isValidTaskTitle(next)) {
      cancelEdit();
      return;
    }
    setIsSavingTitle(true);
    setTitleError(null);
    try {
      await onTitleChange(task.id, next, task.descricao ?? null);
      setEditing(false);
    } catch (err) {
      setTitleError(err instanceof Error ? err.message : 'Erro ao atualizar título');
    } finally {
      setIsSavingTitle(false);
    }
  };

  return (
    <article
      className={`task-card ${isSubtask ? 'subtask' : ''}`}
      aria-label={`Tarefa ${task.titulo}`}
      data-status={task.status}
    >
      <div className="task-title-row">
        {!editing && onTitleChange && (
          <button
            type="button"
            className="edit-title-btn"
            aria-label={`Editar título de ${task.titulo}`}
            onClick={() => {
              setDraftTitle(task.titulo);
              setEditing(true);
            }}
          >
            <svg
              width="14"
              height="14"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden="true"
            >
              <path d="M12 20h9" />
              <path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4Z" />
            </svg>
          </button>
        )}
        {editing ? (
          <input
            ref={titleInputRef}
            className="task-title-input"
            value={draftTitle}
            onChange={(e) => setDraftTitle(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                e.preventDefault();
                void saveEdit();
              } else if (e.key === 'Escape') {
                cancelEdit();
              }
            }}
            onBlur={() => {
              if (isValidTaskTitle(draftTitle)) {
                void saveEdit();
              } else {
                cancelEdit();
              }
            }}
            minLength={MIN_TITLE}
            maxLength={MAX_TITLE}
            disabled={isSavingTitle}
            aria-label={`Novo título de ${task.titulo}`}
          />
        ) : (
          <h3>{task.titulo}</h3>
        )}
      </div>
      {titleError && <p role="alert" className="error">{titleError}</p>}
      {task.descricao && <p>{task.descricao}</p>}
      <label>
        Status{' '}
        <select
          value={task.status}
          onChange={(e) => onStatusChange(task.id, e.target.value as TaskStatus)}
          aria-label={`Status da tarefa ${task.titulo}`}
        >
          {statuses.map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
      </label>
      {!isSubtask && (
        <button
          type="button"
          onClick={() => onAddSubtask(task.id)}
          disabled={draftParentId === task.id}
        >
          Adicionar subtarefa
        </button>
      )}
      {error && <p role="alert" className="error">{error}</p>}
      {(task.subtarefas.length > 0 || showDraft) && (
        <div className="subtasks">
          {task.subtarefas.map((sub) => (
            <TaskCard
              key={sub.id}
              task={sub}
              isSubtask
              onStatusChange={onStatusChange}
              onAddSubtask={onAddSubtask}
              onTitleChange={onTitleChange}
            />
          ))}
          {showDraft && (
            <SubtaskDraft
              parentId={task.id}
              onSave={onSaveDraft}
              onDiscard={onDiscardDraft}
              isSaving={isSavingDraft}
            />
          )}
        </div>
      )}
    </article>
  );
}

interface DraftProps {
  parentId: string;
  onSave: (parentId: string, titulo: string) => Promise<void>;
  onDiscard: () => void;
  isSaving: boolean;
}

function SubtaskDraft({ parentId, onSave, onDiscard, isSaving }: DraftProps) {
  const [titulo, setTitulo] = useState('');
  const [saveError, setSaveError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const dialogRef = useRef<HTMLDialogElement>(null);
  const skipBlurModal = useRef(false);

  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  const valid = isValidTaskTitle(titulo);

  const openModal = () => {
    if (!dialogRef.current?.open) {
      dialogRef.current?.showModal();
    }
  };

  const closeModal = () => {
    if (dialogRef.current?.open) {
      dialogRef.current.close();
    }
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!valid || isSaving) return;
    skipBlurModal.current = true;
    setSaveError(null);
    try {
      await onSave(parentId, titulo.trim());
    } catch (err) {
      skipBlurModal.current = false;
      setSaveError(err instanceof Error ? err.message : 'Erro ao criar subtarefa');
      inputRef.current?.focus();
    }
  };

  const handleBlur = (e: FocusEvent<HTMLInputElement>) => {
    const next = e.relatedTarget as Node | null;
    if (next && e.currentTarget.form?.contains(next)) return;

    window.setTimeout(() => {
      if (skipBlurModal.current || dialogRef.current?.open) return;
      if (!isValidTaskTitle(titulo)) {
        openModal();
      }
    }, 0);
  };

  const handleFill = () => {
    closeModal();
    skipBlurModal.current = false;
    window.setTimeout(() => inputRef.current?.focus(), 0);
  };

  const handleDiscard = () => {
    skipBlurModal.current = true;
    closeModal();
    onDiscard();
  };

  return (
    <>
      <article
        className="task-card subtask draft-subtask"
        aria-label="Nova subtarefa"
      >
        <form onSubmit={handleSubmit} className="draft-subtask-form">
          <input
            ref={inputRef}
            value={titulo}
            onChange={(e) => setTitulo(e.target.value)}
            onBlur={handleBlur}
            placeholder="Título (mín. 3 caracteres)"
            minLength={MIN_TITLE}
            maxLength={MAX_TITLE}
            required
            aria-label="Título da subtarefa"
            disabled={isSaving}
          />
          <button type="submit" disabled={!valid || isSaving}>
            {isSaving ? 'Salvando...' : 'Salvar subtarefa'}
          </button>
        </form>
        {saveError && <p role="alert" className="error">{saveError}</p>}
      </article>

      <dialog
        ref={dialogRef}
        className="modal"
        aria-labelledby="subtask-draft-modal-title"
        onCancel={(e) => {
          e.preventDefault();
        }}
      >
        <h2 id="subtask-draft-modal-title">Título obrigatório</h2>
        <p>
          A subtarefa precisa de um título com no mínimo {MIN_TITLE} caracteres.
          Deseja preencher ou descartar este item?
        </p>
        <div className="modal-actions">
          <button type="button" onClick={handleFill}>
            Preencher
          </button>
          <button type="button" className="modal-danger" onClick={handleDiscard}>
            Descartar
          </button>
        </div>
      </dialog>
    </>
  );
}

export function Layout({ children }: { children: React.ReactNode }) {
  return (
    <div className="app">
      <header>
        <h1>TodoCharacters</h1>
        <nav aria-label="Principal">
          <NavLink to="/tarefas" className={({ isActive }) => (isActive ? 'active-link' : undefined)}>
            Tarefas
          </NavLink>
          <NavLink to="/personagens" className={({ isActive }) => (isActive ? 'active-link' : undefined)}>
            Personagens
          </NavLink>
        </nav>
      </header>
      <main>{children}</main>
    </div>
  );
}
