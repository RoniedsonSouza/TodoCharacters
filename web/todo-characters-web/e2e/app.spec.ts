import { test, expect } from '@playwright/test';

test('fluxo completo: tarefa, subtarefa, status e conclusão', async ({ page }) => {
  await page.goto('/tarefas');

  await page.getByLabel('Título da tarefa').fill('Tarefa E2E Fluxo');
  await page.getByRole('button', { name: 'Criar' }).click();
  await expect(page.getByRole('heading', { name: 'Tarefa E2E Fluxo' })).toBeVisible();

  const parent = page.getByRole('article', { name: 'Tarefa Tarefa E2E Fluxo' });
  await parent.getByRole('button', { name: 'Adicionar subtarefa' }).click();
  await parent.getByLabel('Título da subtarefa').fill('Sub E2E Fluxo');
  await parent.getByRole('button', { name: 'Salvar subtarefa' }).click();

  await page.getByLabel('Status da tarefa Sub E2E Fluxo').selectOption('Em progresso');
  await page.getByLabel('Status da tarefa Tarefa E2E Fluxo').selectOption('Em progresso');

  await page.getByLabel('Status da tarefa Sub E2E Fluxo').selectOption('Concluído');
  await page.getByLabel('Status da tarefa Tarefa E2E Fluxo').selectOption('Concluído');

  await expect(page.getByRole('heading', { name: 'Concluídas' })).toBeVisible();
});

test('impede conclusão do pai com subtarefa pendente', async ({ page }) => {
  await page.goto('/tarefas');

  await page.getByLabel('Título da tarefa').fill('Pai Bloqueio E2E');
  await page.getByRole('button', { name: 'Criar' }).click();

  const parent = page.getByRole('article', { name: 'Tarefa Pai Bloqueio E2E' });
  await parent.getByRole('button', { name: 'Adicionar subtarefa' }).click();
  await parent.getByLabel('Título da subtarefa').fill('Filho Pendente E2E');
  await parent.getByRole('button', { name: 'Salvar subtarefa' }).click();

  await page.getByLabel('Status da tarefa Pai Bloqueio E2E').selectOption('Concluído');

  await expect(page.getByRole('alert')).toContainText(/subtarefa/i);
});

test('paginação de personagens e detalhe', async ({ page }) => {
  await page.goto('/personagens');
  await expect(page.getByText('Rick Sanchez')).toBeVisible();

  await page.getByRole('button', { name: 'Próxima' }).click();
  await expect(page.getByText('Summer Smith')).toBeVisible();

  await page.getByRole('link', { name: 'Summer Smith' }).click();
  await expect(page.getByText('Episódios:')).toBeVisible();
});

test('edita título da tarefa via lápis inline', async ({ page }) => {
  await page.goto('/tarefas');

  await page.getByLabel('Título da tarefa').fill('Titulo Original E2E');
  await page.getByRole('button', { name: 'Criar' }).click();
  await expect(page.getByRole('heading', { name: 'Titulo Original E2E' })).toBeVisible();

  const card = page.getByRole('article', { name: 'Tarefa Titulo Original E2E' });
  await card.getByRole('button', { name: 'Editar título de Titulo Original E2E' }).click();
  await card.getByLabel('Novo título de Titulo Original E2E').fill('Titulo Renomeado E2E');
  await card.getByLabel('Novo título de Titulo Original E2E').press('Enter');

  await expect(page.getByRole('heading', { name: 'Titulo Renomeado E2E' })).toBeVisible();
  await expect(page.getByRole('article', { name: 'Tarefa Titulo Renomeado E2E' })).toBeVisible();
});
