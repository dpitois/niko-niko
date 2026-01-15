import React from 'react';
import MoodBad from '@mui/icons-material/MoodBad';
import SentimentDissatisfied from '@mui/icons-material/SentimentDissatisfied';
import SentimentNeutral from '@mui/icons-material/SentimentNeutral';
import SentimentSatisfiedAlt from '@mui/icons-material/SentimentSatisfiedAlt';
import { Avatar, Box, Paper, Typography, useTheme } from '@mui/material';
import dayjs from 'dayjs';
import isSameOrAfter from 'dayjs/plugin/isSameOrAfter';
import { useSnackbar } from 'notistack';

import { useAuth } from '@/context/AuthContext';
import { useMoodMutation } from '@/hooks/useMoodMutation';
import { useMoods } from '@/hooks/useMoods';
import type { MoodType } from '@/models/MoodType';
import { MoodValues } from '@/models/MoodType';

dayjs.extend(isSameOrAfter);

interface SprintMoodGridProps {
  sprintId: string;
  sprintStartDate: Date;
  sprintEndDate: Date;
  teamMembers: { id: string; name: string; email: string; avatarUrl?: string }[];
}

const getMoodColor = (moodType: MoodType) => {
  switch (moodType) {
    case MoodValues.Sad:
      return 'red';
    case MoodValues.Neutral:
      return 'orange';
    case MoodValues.Happy:
      return 'green';
    default:
      return 'grey';
  }
};

const getMoodIcon = (moodType: MoodType) => {
  switch (moodType) {
    case MoodValues.Sad:
      return <SentimentDissatisfied sx={{ color: 'white' }} />;
    case MoodValues.Neutral:
      return <SentimentNeutral sx={{ color: 'white' }} />;
    case MoodValues.Happy:
      return <SentimentSatisfiedAlt sx={{ color: 'white' }} />;
    default:
      return <MoodBad sx={{ color: 'white' }} />; // Default for "None" or unselected
  }
};

const SprintMoodGrid: React.FC<SprintMoodGridProps> = ({
  sprintId,
  sprintStartDate,
  sprintEndDate,
  teamMembers,
}) => {
  const { user } = useAuth(); // Current logged-in user
  const { moods } = useMoods(sprintId); // Fetch moods for this sprint
  const { saveMood } = useMoodMutation(sprintId);
  const theme = useTheme();
  const { enqueueSnackbar } = useSnackbar();
  const today = dayjs().startOf('day');

  // Generate an array of dates for the sprint
  const sprintDates: Date[] = [];
  // eslint-disable-next-line prefer-const
  let day = new Date(sprintStartDate);
  while (day <= sprintEndDate) {
    sprintDates.push(new Date(day)); // Push a new instance of date
    day.setDate(day.getDate() + 1);
  }

  const handleMoodClick = async (date: Date, nextMood: MoodType) => {
    if (!user) return;
    try {
      await saveMood(dayjs(date), nextMood, user.sub);
    } catch {
      enqueueSnackbar('Failed to save mood entry.', { variant: 'error' });
    }
  };

  return (
    <Box sx={{ overflowX: 'auto', maxWidth: '100%', pb: 1 }}>
      <Box sx={{ minWidth: 'max-content', display: 'flex', flexDirection: 'column' }}>
        {/* Header Row: Dates */}
        <Box sx={{ display: 'flex' }}>
          <Box
            sx={{
              width: 150,
              flexShrink: 0,
              p: 1,
              position: 'sticky',
              left: 0,
              zIndex: 2,
              backgroundColor:
                theme.palette.mode === 'dark'
                  ? theme.palette.background.paper
                  : theme.palette.grey[200], // Match the general header background
              borderBottom: `1px solid ${theme.palette.divider}`, // Add a subtle border to match mood cells
            }}
          />{' '}
          {/* Spacer for member names */}
          {sprintDates.map((date, index) => {
            const isToday = dayjs(date).isSame(today, 'day');
            return (
              <Paper
                key={index}
                sx={{
                  width: 40,
                  height: 40,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  backgroundColor: isToday
                    ? theme.palette.primary.main
                    : theme.palette.mode === 'dark'
                      ? theme.palette.grey[800]
                      : theme.palette.grey[200],
                  color: isToday ? theme.palette.primary.contrastText : 'inherit',
                  fontWeight: 'bold',
                  fontSize: '0.75rem',
                  flexShrink: 0,
                  p: 0.5,
                }}
              >
                {date.getDate()}
              </Paper>
            );
          })}
        </Box>

        {/* Mood Rows: Per Member */}
        {teamMembers.map((member) => (
          <Box key={member.id} sx={{ display: 'flex', mt: 0.5 }}>
            <Box
              sx={{
                width: 150,
                flexShrink: 0,
                p: 1,
                display: 'flex',
                alignItems: 'center',
                border: `1px solid ${theme.palette.divider}`,
                backgroundColor:
                  theme.palette.mode === 'dark'
                    ? theme.palette.background.paper
                    : theme.palette.grey[100],
                borderRadius: '4px 0 0 4px',
                position: 'sticky',
                left: 0,
                zIndex: 1,
              }}
            >
              <Avatar
                src={member.avatarUrl}
                alt={member.name}
                sx={{
                  width: 24,
                  height: 24,
                  mr: 1,
                  bgcolor: theme.palette.primary.main,
                  fontSize: '0.75rem',
                }}
              >
                {member.name ? member.name[0].toUpperCase() : '?'}
              </Avatar>
              <Typography variant="body2" noWrap>
                {member.name}
              </Typography>
            </Box>
            {sprintDates.map((date, dateIndex) => {
              // Determine display data: Using SWR cache (which is optimistically updated by the hook)
              // We need to match the date logic used in the hook (startOf('day'))
              const cellDate = dayjs(date);
              
              const displayMoodEntry = moods?.find(
                (m) => m.userId === member.id && dayjs(m.date).startOf('day').isSame(cellDate.startOf('day'))
              );

              const isFuture = cellDate.isAfter(today);
              const isToday = cellDate.isSame(today, 'day');
              const canEdit = user && user.sub === member.id && !isFuture;

              return (
                <Paper
                  key={dateIndex}
                  sx={{
                    width: 40,
                    height: 40,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    backgroundColor: displayMoodEntry
                      ? getMoodColor(displayMoodEntry.mood)
                      : theme.palette.mode === 'dark'
                        ? theme.palette.grey[700]
                        : theme.palette.grey[300],
                    color: 'white',
                    cursor: canEdit ? 'pointer' : 'default',
                    opacity: canEdit ? 1 : 0.7,
                    transition: 'background-color 0.3s',
                    flexShrink: 0,
                    p: 0.5,
                    border: isToday ? `2px solid ${theme.palette.primary.main}` : 'none',
                    '&:hover': canEdit
                      ? {
                          backgroundColor: displayMoodEntry
                            ? getMoodColor(displayMoodEntry.mood)
                            : theme.palette.mode === 'dark'
                              ? theme.palette.grey[600]
                              : theme.palette.grey[400],
                        }
                      : {},
                  }}
                  onClick={() => {
                    if (!canEdit) return;
                    // Cycle logic based on currently DISPLAYED mood
                    const currentMoodValue = displayMoodEntry ? displayMoodEntry.mood : null;
                    let nextMood: MoodType;

                    if (currentMoodValue === null) {
                      nextMood = MoodValues.Happy;
                    } else {
                      nextMood = ((currentMoodValue + 1) %
                        Object.keys(MoodValues).length) as MoodType;
                    }

                    handleMoodClick(date, nextMood);
                  }}
                >
                  {displayMoodEntry ? getMoodIcon(displayMoodEntry.mood) : null}
                  {isFuture && (
                    <Typography
                      variant="caption"
                      sx={{
                        color: 'rgba(255,255,255,0.7)',
                        position: displayMoodEntry ? 'absolute' : 'static',
                        fontSize: displayMoodEntry ? '0.6rem' : '0.75rem',
                      }}
                    >
                      {date.getDate()}
                    </Typography>
                  )}
                </Paper>
              );
            })}
          </Box>
        ))}
      </Box>
    </Box>
  );
};
export default SprintMoodGrid;
