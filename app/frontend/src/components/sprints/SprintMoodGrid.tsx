import React, { useMemo } from 'react';
import dayjs from 'dayjs';
import { useSnackbar } from 'notistack';

import { useAuth } from '@/context/AuthContext';
import { useMoodMutation } from '@/hooks/useMoodMutation';
import { useMoods } from '@/hooks/useMoods';
import type { MoodType } from '@/models/MoodType';
import type { User } from '@/models/User';

import MoodGridDisplay from './MoodGridDisplay';

interface SprintMoodGridProps {
  sprintId: string;
  sprintStartDate: Date;
  sprintEndDate: Date;
  teamMembers: User[];
}

const SprintMoodGrid: React.FC<SprintMoodGridProps> = ({
  sprintId,
  sprintStartDate,
  sprintEndDate,
  teamMembers,
}) => {
  const { user } = useAuth();
  const { moods } = useMoods(sprintId);
  const { saveMood } = useMoodMutation(sprintId);
  const { enqueueSnackbar } = useSnackbar();

  const sprintDates = useMemo(() => {
    const dates: Date[] = [];
    const day = new Date(sprintStartDate);
    while (day <= sprintEndDate) {
      dates.push(new Date(day));
      day.setDate(day.getDate() + 1);
    }
    return dates;
  }, [sprintStartDate, sprintEndDate]);

  const handleMoodClick = async (date: Date, nextMood: MoodType) => {
    if (!user) return;
    try {
      await saveMood(dayjs(date), nextMood, user.sub);
    } catch {
      enqueueSnackbar('Failed to save mood entry.', { variant: 'error' });
    }
  };

  return (
    <MoodGridDisplay
      sprintDates={sprintDates}
      teamMembers={teamMembers}
      moods={moods}
      currentUserId={user?.sub}
      onMoodClick={handleMoodClick}
    />
  );
};

export default SprintMoodGrid;
