import useSWR from 'swr';

import type { Sprint } from '@/models/Sprint';
import { getSprintById } from '@/services/sprintService';

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
