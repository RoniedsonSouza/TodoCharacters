import { useParams, Link, useLocation } from 'react-router-dom';
import { useCharacter } from '../hooks/useApi';
import { Layout } from '../components/TaskCard';

type LocationState = { page?: number };

export function CharacterDetailPage() {
  const { id } = useParams();
  const location = useLocation();
  const characterId = Number(id);
  const { data, isLoading, error } = useCharacter(characterId);

  const fromPage = (location.state as LocationState | null)?.page;
  const backTo =
    typeof fromPage === 'number' && fromPage > 1
      ? `/personagens?page=${fromPage}`
      : '/personagens';

  if (isLoading) return <Layout><p>Carregando detalhe...</p></Layout>;
  if (error) return <Layout><p role="alert">{(error as Error).message}</p></Layout>;
  if (!data) return <Layout><p>Personagem não encontrado.</p></Layout>;

  return (
    <Layout>
      <Link to={backTo}>← Voltar</Link>
      <article className="character-detail">
        <img src={data.imagemUrl} alt={data.nome} width={200} />
        <h2>{data.nome}</h2>
        <ul>
          <li>Status: {data.status}</li>
          <li>Espécie: {data.especie}</li>
          <li>Gênero: {data.genero}</li>
          <li>Origem: {data.origem}</li>
          <li>Localização: {data.localizacao}</li>
          <li>Episódios: {data.quantidadeEpisodios}</li>
        </ul>
      </article>
    </Layout>
  );
}
