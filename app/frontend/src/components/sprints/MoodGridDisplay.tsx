import React from 'react';
import MoodBad from '@mui/icons-material/MoodBad';
import SentimentDissatisfied from '@mui/icons-material/SentimentDissatisfied';
import SentimentNeutral from '@mui/icons-material/SentimentNeutral';
import SentimentSatisfiedAlt from '@mui/icons-material/SentimentSatisfiedAlt';
import { Avatar, Box, Paper, Typography, useTheme } from '@mui/material';
import dayjs from 'dayjs';

import type { Mood } from '@/models/Mood';
import type { MoodType } from '@/models/MoodType';
import { MoodValues } from '@/models/MoodType';
import type { User } from '@/models/User';

interface MoodGridDisplayProps {
  sprintDates: Date[];
  teamMembers: User[];
  moods?: Mood[];
  currentUserId?: string;
  onMoodClick?: (date: Date, nextMood: MoodType) => void;
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
      return <MoodBad sx={{ color: 'white' }} />;
  }
};

const MoodGridDisplay: React.FC<MoodGridDisplayProps> = ({
  sprintDates,
  teamMembers,
  moods,
  currentUserId,
  onMoodClick,
}) => {
  const theme = useTheme();
  const today = dayjs().startOf('day');

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
                  : theme.palette.grey[200],
              borderBottom: `1px solid ${theme.palette.divider}`,
            }}
          />
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
              const cellDate = dayjs(date);
              
              const displayMoodEntry = moods?.find(
                (m) => m.userId === member.id && dayjs(m.date).startOf('day').isSame(cellDate.startOf('day'))
              );

              const isFuture = cellDate.isAfter(today);
              const isToday = cellDate.isSame(today, 'day');
              const canEdit = onMoodClick && currentUserId && currentUserId === member.id && !isFuture;

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
                    if (!canEdit || !onMoodClick) return;
                    const currentMoodValue = displayMoodEntry ? displayMoodEntry.mood : null;
                    let nextMood: MoodType;

                    if (currentMoodValue === null) {
                      nextMood = MoodValues.Happy;
                    } else {
                      nextMood = ((currentMoodValue + 1) %
                        Object.keys(MoodValues).length) as MoodType;
                    }

                    onMoodClick(date, nextMood);
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

export default MoodGridDisplay;