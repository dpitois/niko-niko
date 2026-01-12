import api from './api';
import type { TeamInvitation } from '../models/Team/Invitation/TeamInvitation';
import type { CreateTeamInvitation } from '../models/Team/Invitation/CreateTeamInvitation';

const BASE_URL = '/teamInvitations';
const TEAMS_BASE_URL = '/teams'; // Correction ici

export const teamInvitationService = {
  createTeamInvitation: async (createData: CreateTeamInvitation): Promise<TeamInvitation> => {
    const response = await api.post<TeamInvitation>(BASE_URL, createData);
    return response.data;
  },

  acceptTeamInvitation: async (token: string): Promise<TeamInvitation> => {
    const response = await api.post<TeamInvitation>(`${BASE_URL}/${token}/accept`);
    return response.data;
  },

  getTeamInvitations: async (teamId: string): Promise<TeamInvitation[]> => {
    const response = await api.get<TeamInvitation[]>(`${TEAMS_BASE_URL}/${teamId}/invitations`);
    return response.data;
  },

  deleteTeamInvitation: async (invitationId: string): Promise<void> => {
    await api.delete(`${BASE_URL}/${invitationId}`);
  },
};
