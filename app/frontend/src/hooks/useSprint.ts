import useSWR from 'swr';
import { getSprintById } from '../services/sprintService';
import type { Sprint } from '../models/Sprint';

export const useSprint = (sprintId: string | null) => {
  const { data, error, isLoading, mutate } = useSWR<Sprint>(
    sprintId ? ['/sprint', sprintId] : null,
    () => getSprintById(sprintId!),
  );

  return {
    sprint: data,
    isLoading,
    isError: error,
    mutateSprint: mutate,
  };
};
