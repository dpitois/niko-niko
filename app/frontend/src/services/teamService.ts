import type { CreateTeam } from '@/models/CreateTeam';
import type { TeamDto } from '@/models/Team';
import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';

import api from './api';

export const getTeams = async (): Promise<TeamWithMembersAndSprints[]> => {
  // Changed return type
  const { data } = await api.get('/teams');
  return data;
};

export const createTeam = async (team: CreateTeam): Promise<TeamDto> => {
  const { data } = await api.post('/teams', team);
  return data;
};

export const deleteTeam = async (teamId: string): Promise<void> => {
  await api.delete(`/teams/${teamId}`);
};

export const updateTeam = async (teamId: string, name: string): Promise<void> => {
  await api.put(`/teams/${teamId}`, { name });
};
