# TodoCharacters

Aplicação full-stack de to-do list com consulta de personagens Rick and Morty. Clean Architecture (.NET 8) + React/TypeScript.

## Pré-requisitos

- .NET 8 SDK
- Node.js 20+
- Docker Desktop (SQL Server local)

## Início rápido

```powershell
# 1. Banco de dados
docker compose up -d

# 2. API (aplicar migrations manualmente se dotnet-ef falhar)
# Opção A: dotnet ef database update -p src/TodoCharacters.Infrastructure -s src/TodoCharacters.WebApi
# Opção B: migrations já versionadas em src/TodoCharacters.Infrastructure/Persistence/Migrations

dotnet run --project src/TodoCharacters.WebApi/TodoCharacters.WebApi.csproj

# 3. Frontend
cd web/todo-characters-web
npm install
npm run dev
```

- API: http://localhost:5000/swagger (Development)
- Web: http://localhost:5173

## Comandos de teste

```powershell
dotnet test tests/TodoCharacters.ArchitectureTests
dotnet test tests/TodoCharacters.UnitTests
dotnet test tests/TodoCharacters.IntegrationTests
dotnet test tests/TodoCharacters.FunctionalTests

cd web/todo-characters-web
npm run test:e2e   # usa ASPNETCORE_ENVIRONMENT=Testing (InMemory + FakeCharacterApi)
```

## Configuração Rick and Morty API

`appsettings.json`:

```json
"RickAndMortyApi": {
  "BaseUrl": "https://rickandmortyapi.com/api/",
  "TimeoutSeconds": 10
}
```

Para testes E2E e ambiente `Testing`, a API usa `FakeCharacterApiClient` — **nunca depende da API externa real**.

---

## Decisões e inconsistências do enunciado

Esta seção documenta pegadinhas, lacunas do PDF e como foram resolvidas.

### Cascade delete self-reference (SQL Server)

**Inconsistência / pegadinha:** `DeleteBehavior.Cascade` na FK `tarefaPaiId` faz o SQL Server rejeitar a migration (*cycles or multiple cascade paths*).

**Opções consideradas:**
- `DeleteBehavior.Cascade` (SQL) — rejeitado pelo banco
- Cascade manual no handler — funciona, mais código
- **`DeleteBehavior.ClientCascade`** — EF deleta filhos na mesma transação

**Decisão adotada:** `ClientCascade` em `TaskItemConfiguration`.

**Justificativa:** Atende exigência de configuração EF Core + transação única sem SQL manual.

**Implementação:** `src/TodoCharacters.Infrastructure/Persistence/Configurations/TaskItemConfiguration.cs`

**Cobertura de teste:** `DeleteParentTaskCascadeTests` (IntegrationTests, requer SQL Server)

---

### FluentValidation + Result Pattern

**Inconsistência / pegadinha:** Behavior padrão do FluentValidation lança `ValidationException`; enunciado proíbe exceção como fluxo de negócio.

**Decisão adotada:** `ValidationBehavior<TRequest,TResponse>` + `IResultFactory` com reflection.

**Implementação:** `src/TodoCharacters.Application/Common/Behaviors/ValidationBehavior.cs`

**Cobertura de teste:** `ValidationBehaviorTests`

---

### Episódios ≠ campo numérico

**Decisão:** `quantidadeEpisodios = episode.length` (array de URLs da API externa).

**Cobertura de teste:** `CharacterMapperTests.Should_map_episode_count_from_array_length`

---

### Dados opcionais ausentes (Rick and Morty API)

**Inconsistência:** API retorna `"unknown"` ou `""`, nunca `null`.

**Decisão:** `CharacterMapper.NormalizeOptional` trata valor presente, `"unknown"` e string vazia.

**Cobertura de teste:** `CharacterMapperTests` (3 cenários)

---

### Página fora do range (`?page=999`)

**Decisão:** Adaptador converte HTTP 404 externo em página vazia interna (`items: []`, `hasNext: false`).

**Implementação:** `RickAndMortyClient.GetCharactersAsync`

---

### Paginação — info.next / info.prev

**Decisão:** Contrato interno `{ page, totalPages, totalCount, hasNext, hasPrev, items }` — sem URLs externas.

---

### UTC + SQL Server

**Decisão:** `DateTimeOffset` para `criadaEm` e `concluidaEm`.

**Cobertura de teste:** `TaskItemTests.CreateRoot_should_start_as_NaoIniciado_with_utc_timestamp`

---

### Timeout da integração

**Decisão:** **503 Service Unavailable** com código `EXTERNAL_API_UNAVAILABLE`.

**Justificativa:** Indisponibilidade temporária; enunciado oferece 502/503 (504 não listado).

---

### Status inverso (lacuna do enunciado)

**Cenário:** Pai `Concluído` + subtarefa deixa de ser `Concluído`.

**Decisão:** Reverter pai para **`Em progresso`** ("em aberto"), limpar `concluidaEm`.

**Justificativa:** Mantém coerência sem bloquear reabertura da subtarefa.

**Cobertura de teste:** `ChangeTaskStatusCommandHandlerTests.Deve_reverter_pai_para_em_progresso_quando_subtarefa_deixa_de_ser_concluida`

---

### Subtarefa em subtarefa — 409 vs 400

**Decisão:** **409 Conflict**

**Argumentação:**
1. Payload JSON é sintaticamente válido — 400 seria enganoso
2. Viola regra de domínio (profundidade máxima = 1), alinhado ao exemplo 409 do PDF
3. Distingue de 404 (pai inexistente)
4. Consistência: todas violações de negócio retornam 409

**Cobertura de teste:** `CreateTaskCommandHandlerTests.Subtask_in_subtask_should_return_conflict`

---

### Filtro por status (item opcional — front-end)

**Inconsistência / lacuna:** o enunciado lista "ordenação de tarefas e filtro por status" nos opcionais; a tabela de rotas do `GET /api/tasks` **não** define query params. Não há campo de prioridade, e "fila principal" já separa ativos de concluídos.

**Opções consideradas:**
- Query param no backend (`?status=`) — exige alterar o contrato mínimo e não está na tabela de rotas
- **Filtro no React** sobre a resposta completa — respeita o contrato e atende o diferencial opcional

**Decisão adotada:**
1. `GET /api/tasks` continua retornando **tudo** agrupado em `{ ativas, concluidas }`
2. A tela `/tarefas` oferece filtro client-side: `Todos`, `Não iniciado`, `Em progresso`, `Concluído`
3. A seção "Fila principal" some quando o filtro é só `Concluído`; a seção "Concluídas" some quando o filtro é um status ativo

**Justificativa:** o filtro por status é diferencial opcional; implementá-lo no front evita inventar query params fora do contrato HTTP mínimo e reutiliza o agrupamento já exigido pela UI.

**Implementação:** `web/todo-characters-web/src/pages/TasksPage.tsx`

---

### Outras decisões

| Tópico | Decisão |
|--------|---------|
| "Fila principal de prioridades" | Lista ativa no React; concluídos em seção separada |
| Filtro por status | Client-side no React (sem query param no `GET /api/tasks`) — ver seção acima |
| `tarefaPaiId` inexistente | 404 Not Found |
| PUT `/api/tasks/{id}` | Apenas título e descrição |
| Idioma | Código EN; JSON PT (`titulo`, `status`, `criadaEm`) |
| Integration vs Functional vs E2E | Integration = DB + HTTP mock; Functional = WebApplicationFactory; E2E = Playwright |
| IntegrationTests cascade | Executa com SQL Server; pula silenciosamente se container offline |
| CSS | Mínimo funcional (peso UX = 5%) |

---

## Fora do escopo (MVP)

- Autenticação / multiusuário
- Ordenação de tarefas (opcional; filtro por status foi implementado no front)
- Cache in-memory de personagens (opcional)
- characterId em tarefa (opcional)
- Pipeline CI (opcional)
- CSS elaborado

---

## Estrutura

```
src/          — Domain, Application, Infrastructure, WebApi
tests/        — Architecture, Unit, Integration, Functional
web/          — React + Playwright E2E
docs/         — Especificações e passo a passo de implementação
```

Documentação detalhada: [docs/](docs/)
