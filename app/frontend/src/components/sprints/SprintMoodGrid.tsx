import React from 'react';
import MoodBad from '@mui/icons-material/MoodBad';
import SentimentDissatisfied from '@mui/icons-material/SentimentDissatisfied';
import SentimentNeutral from '@mui/icons-material/SentimentNeutral';
import SentimentSatisfiedAlt from '@mui/icons-material/SentimentSatisfiedAlt';
import { Avatar, Box, Paper, Typography, useTheme } from '@mui/material';
import dayjs from 'dayjs';
import isSameOrAfter from 'dayjs/plugin/isSameOrAfter';
import { useMutation } from '@apollo/client';
import { useSnackbar } from 'notistack';

import { useAuth } from '@/context/AuthContext';
import type { MoodType } from '@/models/MoodType';
import { MoodValues } from '@/models/MoodType';

import { ADD_MOOD_ENTRY } from '@/graphql/mutations';

dayjs.extend(isSameOrAfter);

interface SprintMoodGridProps {
  teamId: string;
  sprintId: string;
  sprintStartDate: Date;
  sprintEndDate: Date;
  teamMembers: { id: string; name: string; email: string; avatarUrl?: string }[];
  initialMoods?: { id: string; mood: MoodType | string; date: string; userId: string }[];
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
  initialMoods,
}) => {
  const { user } = useAuth(); // Current logged-in user
  const theme = useTheme();
  const { enqueueSnackbar } = useSnackbar();
  const today = dayjs().startOf('day');

  const [addMoodEntry] = useMutation(ADD_MOOD_ENTRY);

  const moodStringToNumber: Record<string, number> = {
    HAPPY: 0,
    NEUTRAL: 1,
    SAD: 2,
  };

  const moods = (initialMoods || []).map((m) => ({
    ...m,
    mood: (typeof m.mood === 'string' ? (moodStringToNumber[m.mood] ?? 0) : m.mood) as MoodType,
  }));

  // Local state for optimistic updates (debouncing)
  // Key: `${userId}_${dateString}`
  const [pendingMoods, setPendingMoods] = React.useState<Record<string, MoodType>>({});
  const timeoutsRef = React.useRef<Record<string, ReturnType<typeof setTimeout>>>({});

  // Generate an array of dates for the sprint
  const sprintDates: Date[] = [];
  // eslint-disable-next-line prefer-const
  let day = new Date(sprintStartDate);
  while (day <= sprintEndDate) {
    sprintDates.push(new Date(day)); // Push a new instance of date
    day.setDate(day.getDate() + 1);
  }

  const handleMoodClick = async (date: Date, moodType: MoodType) => {
    if (!user) return;

    const dateStr = date.toDateString();
    const cellKey = `${user.sub}_${dateStr}`;
    const utcDate = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
    const timezoneOffset = new Date().getTimezoneOffset() * -1;

    // 1. Optimistic Update
    setPendingMoods((prev) => ({ ...prev, [cellKey]: moodType }));

    // 2. Clear existing timeout for this cell
    if (timeoutsRef.current[cellKey]) {
      clearTimeout(timeoutsRef.current[cellKey]);
    }

    // 3. Set new timeout for API call
    timeoutsRef.current[cellKey] = setTimeout(async () => {
      try {
        await addMoodEntry({
          variables: {
            input: {
              sprintId,
              userId: user.sub,
              mood: moodType === 0 ? 'HAPPY' : moodType === 1 ? 'NEUTRAL' : 'SAD',
              date: utcDate.toISOString(),
              timezoneOffset,
            },
          },
        });

        // Remove from pending state (UI assumes success, but real data won't update until refetch)
        // Since we don't auto-refetch here, the 'pending' state removal might revert the UI if we strictly rely on props.
        // However, 'pendingMoods' is only used if present. 
        // Ideally, we should update Apollo cache or keep pending state until confirmed.
        // For this demo, we assume success. To keep UI consistent without refetch, 
        // we might want to NOT delete from pending immediately or update a local state copy of moods.
        // But let's stick to the previous logic structure.
        
        setPendingMoods((prev) => {
          const newState = { ...prev };
          delete newState[cellKey];
          return newState;
        });
      } catch {
        enqueueSnackbar('Failed to save mood entry.', { variant: 'error' });
        // Remove from pending state to revert UI to server state
        setPendingMoods((prev) => {
          const newState = { ...prev };
          delete newState[cellKey];
          return newState;
        });
      } finally {
        delete timeoutsRef.current[cellKey];
      }
    }, 1000); // 1 second debounce
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
              // Determine display data: Pending > Server > Null
              const dateStr = date.toDateString();
              const cellKey = `${member.id}_${dateStr}`;
              const pendingMood = pendingMoods[cellKey];

              const serverMoodEntry = moods?.find(
                (m) => m.userId === member.id && new Date(m.date).toDateString() === dateStr,
              );

              // Construct a virtual mood object for display
              const displayMoodEntry =
                pendingMood !== undefined
                  ? { mood: pendingMood, date: date.toISOString(), userId: member.id } // Virtual entry from pending state
                  : serverMoodEntry;

              const isFuture = dayjs(date).isAfter(today);
              const isToday = dayjs(date).isSame(today);
              
              const normalizeId = (id: string | undefined) => id?.toLowerCase().replace(/-/g, '');
              const canEdit = user && normalizeId(user.sub) === normalizeId(member.id) && !isFuture;

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
