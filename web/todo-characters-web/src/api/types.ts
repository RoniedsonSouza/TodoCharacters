export type TaskStatus = 'Não iniciado' | 'Em progresso' | 'Concluído';

export interface TaskDto {
  id: string;
  titulo: string;
  descricao?: string | null;
  status: TaskStatus;
  criadaEm: string;
  concluidaEm?: string | null;
  tarefaPaiId?: string | null;
  subtarefas: TaskDto[];
}

export interface TasksGroupedDto {
  ativas: TaskDto[];
  concluidas: TaskDto[];
}

export interface CharacterSummaryDto {
  id: number;
  nome: string;
  status: string;
  especie: string;
  imagemUrl: string;
}

export interface CharacterDetailDto extends CharacterSummaryDto {
  genero: string;
  origem: string;
  localizacao: string;
  quantidadeEpisodios: number;
}

export interface CharacterPageDto {
  items: CharacterSummaryDto[];
  page: number;
  totalPages: number;
  totalCount: number;
  hasNext: boolean;
  hasPrev: boolean;
}

export interface ApiError {
  code: string;
  message: string;
}
