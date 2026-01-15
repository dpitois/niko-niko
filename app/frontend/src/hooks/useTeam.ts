import useSWR from 'swr';

import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';
import { getTeamById } from '@/services/teamService';

export const useTeam = (teamId: string | undefined) => {
  const { data, error, mutate, isLoading } = useSWR<TeamWithMembersAndSprints>(
    teamId ? [`/teams/${teamId}`] : null,
    () => getTeamById(teamId!),
  );

  return {
    team: data,
    isLoading,
    isError: error,
    mutate,
  };
};
