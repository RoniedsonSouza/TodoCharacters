import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { TasksPage } from './pages/TasksPage';
import { CharactersPage } from './pages/CharactersPage';
import { CharacterDetailPage } from './pages/CharacterDetailPage';
import './App.css';

const queryClient = new QueryClient();

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Navigate to="/tarefas" replace />} />
          <Route path="/tarefas" element={<TasksPage />} />
          <Route path="/personagens" element={<CharactersPage />} />
          <Route path="/personagens/:id" element={<CharacterDetailPage />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
