import useSWR from 'swr';
import { getSprints } from '../services/sprintService';

export const useSprints = (teamId: string | null) => {
  // The key is an array, so SWR re-fetches when teamId changes.
  const { data, error, isLoading, mutate } = useSWR(teamId ? ['/sprints', teamId] : null, () => getSprints(teamId!));

  return {
    sprints: data,
    isLoading,
    isError: error,
    mutateSprints: mutate,
  };
};
