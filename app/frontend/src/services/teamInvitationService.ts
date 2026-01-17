import type { CreateTeamInvitation } from '@/models/Team/Invitation/CreateTeamInvitation';
import type { TeamInvitation } from '@/models/Team/Invitation/TeamInvitation';

import api from './api';

const BASE_URL = '/teamInvitations';
const TEAMS_BASE_URL = '/teams'; // Correction ici

export const teamInvitationService = {
  createTeamInvitation: async (createData: CreateTeamInvitation): Promise<TeamInvitation> => {
    const response = await api.post<TeamInvitation>(
      `${TEAMS_BASE_URL}/${createData.teamId}/invitations`,
      createData
    );
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

  deleteTeamInvitation: async (teamId: string, invitationId: string): Promise<void> => {
    await api.delete(`${TEAMS_BASE_URL}/${teamId}/invitations/${invitationId}`);
  },
};
