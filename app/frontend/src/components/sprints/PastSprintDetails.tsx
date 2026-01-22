import React, { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Divider, Grid, Typography } from '@mui/material';
import dayjs from 'dayjs';

import { useMoods } from '@/hooks/useMoods';
import type { User } from '@/models/User';
import { calculateMoodTrend } from '@/utils/moodTrendUtils';

import MoodTrendChart from '@/components/dashboard/MoodTrendChart';
import MoodGridDisplay from './MoodGridDisplay';

interface PastSprintDetailsProps {
  sprintId: string;
  sprintStartDate: string;
  sprintEndDate: string;
  teamMembers: User[];
}

const PastSprintDetails: React.FC<PastSprintDetailsProps> = ({
  sprintId,
  sprintStartDate,
  sprintEndDate,
  teamMembers,
}) => {
  const { t } = useTranslation();
  const { moods, isLoading } = useMoods(sprintId);

  const chartData = useMemo(() => {
    return calculateMoodTrend(moods, sprintStartDate, sprintEndDate);
  }, [moods, sprintStartDate, sprintEndDate]);

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

  if (isLoading) {
    return <Typography sx={{ py: 2 }}>{t('common.loading')}</Typography>;
  }

  return (
    <Box sx={{ mt: 2 }}>
      <Grid container spacing={4}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Typography variant="subtitle1" gutterBottom fontWeight="bold">
            {t('dashboard.teamTrend')}
          </Typography>
          <Box sx={{ height: 150 }}>
            <MoodTrendChart chartData={chartData} showUserTrend={false} />
          </Box>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <Typography variant="subtitle1" gutterBottom fontWeight="bold">
            {t('pastSprints.moodGrid', "Grille d'humeurs")}
          </Typography>
          <MoodGridDisplay sprintDates={sprintDates} teamMembers={teamMembers} moods={moods} />
        </Grid>
      </Grid>
      <Divider sx={{ mt: 4 }} />
    </Box>
  );
};

export default PastSprintDetails;
