# TodoCharacters

Desafio técnico de um processo seletivo. A empresa não é nomeada neste repositório.
O pedido: API de tarefas em SQL Server, consulta à API do Rick and Morty, .NET 8 em Clean Architecture, JSON em português, cliente React/TypeScript e testes (unidade, integração, arquitetura, funcionais e E2E).

## Funcionalidades

- Tarefas com um nível de subtarefas, status (`Não iniciado`, `Em progresso`, `Concluído`) e bloqueio da conclusão do pai com subtarefa em aberto.
- Exclusão em cascata, datas em UTC e `Result` no lugar de exceção para validação e regra de negócio.
- Lista dividida em ativas e concluídas, com filtro de status no cliente.
- Personagens paginados (grade ou lista), detalhe e HTTP 503 se a API externa falhar.
- Fora do escopo: autenticação, ordenação, cache, vínculo entre tarefa e personagem e CI.

## Stack

.NET 8, Clean Architecture, EF Core, SQL Server 2022, MediatR, FluentValidation, React, TypeScript, Vite e Playwright.

## Arquitetura

`Domain` não referencia outras camadas. `Application` (MediatR, FluentValidation e `Result`) referencia só o domínio. `Infrastructure` (EF Core e o cliente Rick and Morty) referencia a aplicação. `WebApi` é o composition root, com minimal APIs. Em Development, se o SQL Server não conectar, a API usa banco em memória e avisa no console. No ambiente `Testing`, o banco é em memória e os personagens vêm de um cliente falso.

## Testes

`dotnet test` nos projetos em `tests/`. Em `web/todo-characters-web`, `npm run test:e2e` sobe a API em `Testing` e o Vite. O teste de cascade só executa a asserção com o container no ar e `MSSQL_SA_PASSWORD` definido.

## Como rodar

Pré-requisitos: .NET 8 SDK e Node.js 20.19+ ou 22.12+. Docker só é necessário para persistir no SQL Server.

```bash
cp .env.example .env
# Edite MSSQL_SA_PASSWORD (senha forte: maiúscula, minúscula, número e símbolo)
set -a && source .env && set +a
docker compose up -d
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=TodoCharacters;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True"
dotnet run --project src/TodoCharacters.WebApi/TodoCharacters.WebApi.csproj
```

```bash
cd web/todo-characters-web
npm install
npm run dev
```

API em http://localhost:5000/swagger e interface em http://localhost:5173. Sem o container, `dotnet run` sobe com banco em memória.

No PowerShell, copie `.env.example` para `.env`, carregue `MSSQL_SA_PASSWORD` e defina `$env:ConnectionStrings__DefaultConnection` com a mesma senha antes do `dotnet run`.

## Licença

[MIT](LICENSE). Copyright (c) 2026 Roniedson de Souza Valadares Silva.
