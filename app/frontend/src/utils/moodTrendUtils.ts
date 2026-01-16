import dayjs from 'dayjs';
import isSameOrBefore from 'dayjs/plugin/isSameOrBefore';

import type { Mood } from '@/models/Mood';
import { MoodValues } from '@/models/MoodType';

dayjs.extend(isSameOrBefore);

export interface ChartDataPoint {
  date: string;
  displayDate: string;
  average: number | null;
  userAverage: number | null;
}

export const calculateMoodTrend = (
  moods: Mood[] | undefined,
  sprintStartDate: string,
  sprintEndDate: string,
  userId?: string,
): ChartDataPoint[] => {
  if (!moods) return [];

  const moodsByDate: Record<
    string,
    { totalScore: number; count: number; userScore: number; userCount: number }
  > = {};

  const start = dayjs(sprintStartDate);
  const end = dayjs(sprintEndDate);
  const today = dayjs().startOf('day');
  const lastDate = end.isBefore(today) ? end : today;

  for (let d = start; d.isSameOrBefore(lastDate); d = d.add(1, 'day')) {
    moodsByDate[d.format('YYYY-MM-DD')] = { totalScore: 0, count: 0, userScore: 0, userCount: 0 };
  }

  moods.forEach((m) => {
    const dateStr = dayjs(m.date).format('YYYY-MM-DD');
    if (moodsByDate[dateStr]) {
      let score = 0;
      if (m.mood === MoodValues.Happy) score = 3;
      else if (m.mood === MoodValues.Neutral) score = 2;
      else if (m.mood === MoodValues.Sad) score = 1;

      moodsByDate[dateStr].totalScore += score;
      moodsByDate[dateStr].count += 1;

      if (userId && m.userId === userId) {
        moodsByDate[dateStr].userScore += score;
        moodsByDate[dateStr].userCount += 1;
      }
    }
  });

  return Object.entries(moodsByDate)
    .map(([date, data]) => ({
      date,
      displayDate: dayjs(date).format('DD/MM'),
      average: data.count > 0 ? parseFloat((data.totalScore / data.count).toFixed(2)) : null,
      userAverage:
        data.userCount > 0 ? parseFloat((data.userScore / data.userCount).toFixed(2)) : null,
    }))
    .sort((a, b) => a.date.localeCompare(b.date));
};

