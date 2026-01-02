import api from './api';
import type { TeamDto } from '../models/Team';
import type { CreateTeam } from '../models/CreateTeam';
import type { TeamWithSprints } from '../models/TeamWithSprints'; // Import new DTO

export const getTeams = async (): Promise<TeamWithSprints[]> => { // Changed return type
    const { data } = await api.get('/teams');
    return data;
};

export const createTeam = async (team: CreateTeam): Promise<TeamDto> => {
    const { data } = await api.post('/teams', team);
    return data;
};

