import { useState } from 'react';
import dayjs, { Dayjs } from 'dayjs';

import { useMoods } from '@/hooks/useMoods'; // To access the cache key logic if needed, or re-export types
import type { Mood } from '@/models/Mood';
import type { MoodType } from '@/models/MoodType';
import { createMoodEntry, updateMoodEntry } from '@/services/moodService';

export const useMoodMutation = (sprintId: string) => {
  const [isUpdating, setIsUpdating] = useState(false);
  const { moods, mutateMoods } = useMoods(sprintId);

  const saveMood = async (date: Dayjs, moodType: MoodType, userId: string) => {
    if (isUpdating || !moods) return;

    setIsUpdating(true);

    // Safe Date logic: Force 12:00 UTC to avoid timezone shifts on backend
    const safeDate = new Date(Date.UTC(date.year(), date.month(), date.date(), 12, 0, 0));
    const safeDateStr = safeDate.toISOString();

    // Find existing entry for this specific day
    // We compare using the same logic: startOf('day') in local time or just use the backend date string if available
    // Here we use the backend date check logic we established:
    const existingMood = moods.find(
      (m) => m.userId === userId && dayjs(m.date).startOf('day').isSame(date.startOf('day'))
    );

    // 1. Optimistic Data Construction
    const optimisticMoodEntry: Mood = {
      id: existingMood?.id ?? `opt-${Date.now()}`,
      userId,
      sprintId,
      date: safeDateStr,
      mood: moodType,
    };

    const optimisticMoodsList = existingMood
      ? moods.map((m) => (m.id === existingMood.id ? optimisticMoodEntry : m))
      : [...moods, optimisticMoodEntry];

    // 2. Apply Optimistic Update
    await mutateMoods(optimisticMoodsList, { revalidate: false });

    try {
      if (existingMood) {
        await updateMoodEntry({
          mood: moodType,
          date: safeDateStr,
          sprintId,
          userId,
        });
      } else {
        await createMoodEntry({
          mood: moodType,
          date: safeDateStr,
          sprintId,
          userId,
        });
      }
      
      // 3. Revalidate
      await mutateMoods();
      return true; // Success
    } catch (error) {
      console.error(error);
      // Revert is handled by revalidation
      await mutateMoods();
      throw error;
    } finally {
      setIsUpdating(false);
    }
  };

  return {
    saveMood,
    isUpdating,
  };
};
