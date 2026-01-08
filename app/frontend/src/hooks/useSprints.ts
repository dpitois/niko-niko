import useSWR from 'swr';
import { getSprints } from '../services/sprintService';

const useSprints = (teamId?: string) => {
  const { data, error, mutate } = useSWR(teamId ? `sprints-${teamId}` : 'sprints', () => getSprints(teamId));

  return {
    sprints: data,
    isLoading: !error && !data,
    isError: error,
    mutate,
  };
};

export default useSprints;