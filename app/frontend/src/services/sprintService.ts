import type { CreateSprint } from '@/models/CreateSprint';
import type { Sprint } from '@/models/Sprint';

import api from './api';

export const getSprints = async (teamId?: string): Promise<Sprint[]> => {
  const url = teamId ? `/sprints?teamId=${teamId}` : '/sprints';
  const { data } = await api.get(url);
  return data;
};

export const getSprintById = async (sprintId: string): Promise<Sprint> => {
  const { data } = await api.get(`/sprints/${sprintId}`);
  return data;
};

export const createSprint = async (sprint: CreateSprint): Promise<Sprint> => {
  const { data } = await api.post('/sprints', sprint);
  return data;
};

export const deleteSprint = async (sprintId: string): Promise<void> => {
  await api.delete(`/sprints/${sprintId}`);
};
