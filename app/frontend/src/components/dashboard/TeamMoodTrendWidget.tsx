import React, { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Card,
  CardContent,
  CircularProgress,
  Typography,
} from '@mui/material';

import { useAuth } from '@/context/AuthContext';
import { useMoods } from '@/hooks/useMoods';
import { calculateMoodTrend } from '@/utils/moodTrendUtils';

import MoodTrendChart from './MoodTrendChart';

interface TeamMoodTrendWidgetProps {
  sprintId: string;
  sprintStartDate: string;
  sprintEndDate: string;
  showUserTrend?: boolean;
}

const TeamMoodTrendWidget: React.FC<TeamMoodTrendWidgetProps> = ({
  sprintId,
  sprintStartDate,
  sprintEndDate,
  showUserTrend = true,
}) => {
  const { t } = useTranslation();
  const { user } = useAuth();
  const { moods, isLoading } = useMoods(sprintId);

  const chartData = useMemo(() => {
    return calculateMoodTrend(moods, sprintStartDate, sprintEndDate, user?.sub);
  }, [moods, sprintStartDate, sprintEndDate, user?.sub]);

  if (isLoading) {
    return (
      <Card
        sx={{ height: '100%', display: 'flex', justifyContent: 'center', alignItems: 'center' }}
      >
        <CircularProgress />
      </Card>
    );
  }

  return (
    <Card sx={{ height: '100%', boxShadow: 3, borderRadius: 2 }}>
      <CardContent sx={{ height: '100%', display: 'flex', flexDirection: 'column', pb: '16px !important' }}>
        <Typography variant="h6" gutterBottom align="center" sx={{ fontSize: '1rem' }}>
          {t('dashboard.teamTrend', "Tendance de l'équipe")}
        </Typography>

        <MoodTrendChart chartData={chartData} showUserTrend={showUserTrend} />
      </CardContent>
    </Card>
  );
};

export default TeamMoodTrendWidget;
