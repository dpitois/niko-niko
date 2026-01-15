import React, { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Box,
  Card,
  CardContent,
  CircularProgress,
  Typography,
  useTheme,
} from '@mui/material';
import dayjs from 'dayjs';
import isSameOrBefore from 'dayjs/plugin/isSameOrBefore';
import {
  Area,
  AreaChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

dayjs.extend(isSameOrBefore);

import { useMoods } from '@/hooks/useMoods';
import { MoodValues } from '@/models/MoodType';

interface TeamMoodTrendWidgetProps {
  sprintId: string;
  sprintStartDate: string;
  sprintEndDate: string;
}

const TeamMoodTrendWidget: React.FC<TeamMoodTrendWidgetProps> = ({
  sprintId,
  sprintStartDate,
  sprintEndDate,
}) => {
  const { t } = useTranslation();
  const theme = useTheme();
  const { moods, isLoading } = useMoods(sprintId);

  const chartData = useMemo(() => {
    if (!moods) return [];

    const moodsByDate: Record<string, { totalScore: number; count: number }> = {};
    
    // Initialize dates in sprint up to today
    const start = dayjs(sprintStartDate);
    const end = dayjs(sprintEndDate);
    const today = dayjs().startOf('day');
    const lastDate = end.isBefore(today) ? end : today;

    for (let d = start; d.isSameOrBefore(lastDate); d = d.add(1, 'day')) {
      moodsByDate[d.format('YYYY-MM-DD')] = { totalScore: 0, count: 0 };
    }

    moods.forEach((m) => {
      const dateStr = dayjs(m.date).format('YYYY-MM-DD');
      if (moodsByDate[dateStr]) {
        // Map: Happy(0) -> 3, Neutral(1) -> 2, Sad(2) -> 1
        let score = 0;
        if (m.mood === MoodValues.Happy) score = 3;
        else if (m.mood === MoodValues.Neutral) score = 2;
        else if (m.mood === MoodValues.Sad) score = 1;
        
        moodsByDate[dateStr].totalScore += score;
        moodsByDate[dateStr].count += 1;
      }
    });

    return Object.entries(moodsByDate)
      .map(([date, data]) => ({
        date,
        displayDate: dayjs(date).format('DD/MM'),
        average: data.count > 0 ? parseFloat((data.totalScore / data.count).toFixed(2)) : null,
      }))
      .sort((a, b) => a.date.localeCompare(b.date));
  }, [moods, sprintStartDate, sprintEndDate]);

  if (isLoading) {
    return (
      <Card sx={{ height: '100%', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
        <CircularProgress />
      </Card>
    );
  }

  return (
    <Card sx={{ height: '100%', boxShadow: 3, borderRadius: 2 }}>
      <CardContent sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
        <Typography variant="h6" gutterBottom align="center">
          {t('dashboard.teamTrend', 'Tendance de l\'équipe')}
        </Typography>
        
        <Box sx={{ flexGrow: 1, minHeight: 200, mt: 2 }}>
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={chartData}>
              <defs>
                <linearGradient id="colorAverage" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor={theme.palette.primary.main} stopOpacity={0.8} />
                  <stop offset="95%" stopColor={theme.palette.primary.main} stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis 
                dataKey="displayDate" 
                tick={{ fontSize: 12 }}
                tickLine={false}
                axisLine={false}
              />
              <YAxis 
                domain={[1, 3]} 
                ticks={[1, 2, 3]}
                tickFormatter={(value) => {
                  if (value === 3) return '😊';
                  if (value === 2) return '😐';
                  if (value === 1) return '☹️';
                  return '';
                }}
                tick={{ fontSize: 16 }}
                axisLine={false}
                tickLine={false}
              />
              <Tooltip 
                formatter={(value: number | undefined) => [value ?? 0, t('dashboard.averageMood', 'Humeur moyenne')]}
                labelFormatter={(label) => `${t('common.date', 'Date')}: ${label}`}
                contentStyle={{ 
                  backgroundColor: theme.palette.background.paper, 
                  color: theme.palette.text.primary,
                  border: `1px solid ${theme.palette.divider}`,
                  borderRadius: '4px'
                }}
              />
              <Area
                type="monotone"
                dataKey="average"
                stroke={theme.palette.primary.main}
                fillOpacity={1}
                fill="url(#colorAverage)"
                connectNulls
              />
            </AreaChart>
          </ResponsiveContainer>
        </Box>
      </CardContent>
    </Card>
  );
};

export default TeamMoodTrendWidget;
