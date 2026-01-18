import type { CreateTeam } from '@/models/CreateTeam';
import type { TeamDto } from '@/models/Team';
import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';

import api from './api';

export const getTeams = async (): Promise<TeamWithMembersAndSprints[]> => {
  // Changed return type
  const { data } = await api.get('/teams');
  return data;
};

export const getTeamById = async (teamId: string): Promise<TeamWithMembersAndSprints> => {
  const { data } = await api.get(`/teams/${teamId}`);
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

export const transferTeamAdmin = async (teamId: string, newAdminId: string): Promise<void> => {
  await api.put(`/teams/${teamId}/admin`, { newAdminId });
};

export const removeUserFromTeam = async (teamId: string, userId: string): Promise<void> => {
  await api.delete(`/teams/${teamId}/users/${userId}`);
};

