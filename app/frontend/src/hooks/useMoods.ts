import useSWR from 'swr';

import type { Mood } from '@/models/Mood';
import { getMoodEntriesBySprint } from '@/services/moodService';

export const useMoods = (sprintId: string | null, userId?: string | null, date?: string | null) => {
  const swrKey = sprintId ? ['/moods', sprintId, userId, date] : null;

  const { data, error, isLoading, mutate } = useSWR<Mood[]>(swrKey, () =>
    getMoodEntriesBySprint(sprintId!, userId ?? undefined, date ?? undefined),
  );

  return {
    moods: data,
    isLoading,
    isError: error,
    mutateMoods: mutate,
  };
};
