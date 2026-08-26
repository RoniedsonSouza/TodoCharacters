import { Link, useSearchParams } from 'react-router-dom';
import { useState } from 'react';
import { useCharacters } from '../hooks/useApi';
import { Layout } from '../components/TaskCard';
import { ViewModeToggle, type ViewMode } from '../components/ViewModeToggle';

function parsePage(value: string | null): number {
  const n = Number(value);
  return Number.isFinite(n) && n >= 1 ? Math.floor(n) : 1;
}

export function CharactersPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const page = parsePage(searchParams.get('page'));
  const [viewMode, setViewMode] = useState<ViewMode>('grade');
  const { data, isLoading, error, refetch, isFetching } = useCharacters(page);

  const setPage = (next: number) => {
    const safe = Math.max(1, next);
    setSearchParams(safe <= 1 ? {} : { page: String(safe) });
  };

  const prevDisabled = page <= 1;
  const nextDisabled =
    isLoading ||
    !data ||
    !data.hasNext ||
    page >= data.totalPages;

  return (
    <Layout>
      <div className="characters-header">
        <h2>Personagens</h2>
        <ViewModeToggle value={viewMode} onChange={setViewMode} />
      </div>

      {error && (
        <div className="characters-error">
          <p role="alert">
            Personagens indisponíveis: {(error as Error).message}
          </p>
          <button type="button" onClick={() => refetch()} disabled={isFetching}>
            {isFetching ? 'Atualizando...' : 'Atualizar'}
          </button>
        </div>
      )}

      {isLoading && !error && <p>Carregando personagens...</p>}

      {!isLoading && !error && data?.items.length === 0 && (
        <p>Nenhum personagem nesta página.</p>
      )}

      {!isLoading && !error && viewMode === 'grade' && (
        <ul className="character-grid">
          {data?.items.map((c) => (
            <li key={c.id}>
              <img src={c.imagemUrl} alt={c.nome} width={160} height={160} />
              <Link to={`/personagens/${c.id}`} state={{ page }}>
                {c.nome}
              </Link>
              <p>{c.status} — {c.especie}</p>
            </li>
          ))}
        </ul>
      )}

      {!isLoading && !error && viewMode === 'lista' && (
        <ul className="character-list">
          {data?.items.map((c) => (
            <li key={c.id}>
              <img src={c.imagemUrl} alt={c.nome} width={80} height={80} />
              <div>
                <Link to={`/personagens/${c.id}`} state={{ page }}>
                  {c.nome}
                </Link>
                <p>{c.status} — {c.especie}</p>
              </div>
            </li>
          ))}
        </ul>
      )}

      <nav aria-label="Paginação">
        <button type="button" disabled={prevDisabled} onClick={() => setPage(page - 1)}>
          Anterior
        </button>
        <span>Página {data?.page ?? page}{data ? ` de ${data.totalPages}` : ''}</span>
        <button
          type="button"
          disabled={nextDisabled}
          onClick={() => setPage(page + 1)}
        >
          Próxima
        </button>
      </nav>
    </Layout>
  );
}
