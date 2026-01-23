import React, { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
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
  sprintStartDate: string;
  sprintEndDate: string;
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
  const { t } = useTranslation();

  const sprintDates = useMemo(() => {
    const dates: Date[] = [];
    let current = dayjs(sprintStartDate).startOf('day');
    const end = dayjs(sprintEndDate).startOf('day');

    while (current.isSameOrBefore(end)) {
      dates.push(current.toDate());
      current = current.add(1, 'day');
    }
    return dates;
  }, [sprintStartDate, sprintEndDate]);

  const displayMembers = useMemo(() => {
    const members = [...teamMembers];

    if (!moods) return members;

    // Check for moods from deleted users (userId is null)
    const hasDeletedUserMoods = moods.some((m) => m.userId === null);
    if (hasDeletedUserMoods) {
      members.push({
        id: 'null', // Use string 'null' as key for pseudo-member
        name: t('common.deletedUser', 'Deleted User'),
        avatarUrl: undefined,
      } as User);
    }

    // Check for moods from users who left the team (userId exists but not in teamMembers)
    const otherUserIds = new Set(
      moods
        .filter((m) => m.userId !== null && !teamMembers.some((tm) => tm.id === m.userId))
        .map((m) => m.userId as string),
    );

    otherUserIds.forEach((id) => {
      members.push({
        id,
        name: t('common.formerMember', 'Former Member'),
        avatarUrl: undefined,
      } as User);
    });

    return members;
  }, [teamMembers, moods, t]);

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
      teamMembers={displayMembers}
      moods={moods}
      currentUserId={user?.sub}
      onMoodClick={handleMoodClick}
    />
  );
};

export default SprintMoodGrid;
