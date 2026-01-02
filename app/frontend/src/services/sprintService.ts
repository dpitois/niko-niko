import api from './api';
import type { Sprint } from '../models/Sprint';
import type { CreateSprint } from '../models/CreateSprint';

export const getSprints = async (teamId: string): Promise<Sprint[]> => {
  const { data } = await api.get(`/sprints?teamId=${teamId}`);
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
