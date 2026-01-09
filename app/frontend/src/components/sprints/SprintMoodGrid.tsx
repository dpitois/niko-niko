import React from 'react';
import { Box, Typography, Paper, Avatar, useTheme } from '@mui/material';
import { useSnackbar } from 'notistack';
import { useSWRConfig } from 'swr';
import type { MoodType } from '../../models/MoodType';
import { MoodValues } from '../../models/MoodType';
import { useMoods } from '../../hooks/useMoods';
import { useAuth } from '../../context/AuthContext'; // To get current user
import { createMoodEntry, updateMoodEntry } from '../../services/moodService'; // API services
import { MoodBad, SentimentDissatisfied, SentimentNeutral, SentimentSatisfiedAlt } from '@mui/icons-material';
import dayjs from 'dayjs';
import isSameOrAfter from 'dayjs/plugin/isSameOrAfter';

dayjs.extend(isSameOrAfter);

interface SprintMoodGridProps {
  teamId: string;
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
  teamId,
  sprintId,
  sprintStartDate,
  sprintEndDate,
  teamMembers,
}) => {
  const { user } = useAuth(); // Current logged-in user
  const { moods, mutateMoods } = useMoods(sprintId); // Fetch moods for this sprint
  const { mutate } = useSWRConfig();
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

  const handleMoodClick = async (date: Date, moodType: MoodType) => {
    if (!user) return;

    const existingMood = moods?.find(
      (m) =>
        m.userId === user.sub &&
        new Date(m.date).toDateString() === date.toDateString()
    );

    const utcDate = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));

    try {
      if (existingMood) {
        // Update existing mood
        await updateMoodEntry({
          mood: moodType,
          date: utcDate.toISOString(),
          sprintId: sprintId,
          userId: user.sub,
        });
      } else {
        // Create new mood
        await createMoodEntry({
          mood: moodType,
          date: utcDate.toISOString(),
          sprintId: sprintId,
          userId: user.sub,
        });
      }
      mutateMoods(); // Revalidate moods for this sprint
      mutate(`/teams/${teamId}/sprints`); // Revalidate sprints to potentially update averages
    } catch (error) {
      enqueueSnackbar('Failed to save mood entry.', { variant: 'error' });
    }
  };

    return (
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
              backgroundColor: theme.palette.grey[200], // Match the general header background
              borderBottom: '1px solid #eee' // Add a subtle border to match mood cells
            }}
          /> {/* Spacer for member names */}
          {sprintDates.map((date, index) => (
            <Paper
              key={index}
              sx={{
                width: 40,
                height: 40,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                backgroundColor: theme.palette.grey[200],
                fontWeight: 'bold',
                fontSize: '0.75rem',
                flexShrink: 0,
                p: 0.5,
              }}
            >
              {date.getDate()}
            </Paper>
          ))}
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
                border: '1px solid #eee',
                backgroundColor: theme.palette.grey[100],
                borderRadius: '4px 0 0 4px',
                position: 'sticky',
                left: 0,
                zIndex: 1,
              }}
            >
              <Avatar 
                src={member.avatarUrl}
                alt={member.name}
                sx={{ width: 24, height: 24, mr: 1, bgcolor: theme.palette.primary.main, fontSize: '0.75rem' }}
              >
                {member.name ? member.name[0].toUpperCase() : '?'}
              </Avatar>
              <Typography variant="body2" noWrap>
                {member.name}
              </Typography>
            </Box>
            {sprintDates.map((date, dateIndex) => {
              const moodEntry = moods?.find(
                (m) =>
                  m.userId === member.id &&
                  new Date(m.date).toDateString() === date.toDateString()
              );
              const isTodayOrFuture = dayjs(date).isSameOrAfter(today);
              const canEdit = user && user.sub === member.id && !isTodayOrFuture;
  
              return (
                <Paper
                  key={dateIndex}
                  sx={{
                    width: 40,
                    height: 40,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    backgroundColor: moodEntry ? getMoodColor(moodEntry.mood) : theme.palette.grey[300],
                    color: 'white',
                    cursor: canEdit ? 'pointer' : 'default',
                    opacity: canEdit ? 1 : 0.7,
                    transition: 'background-color 0.3s',
                    flexShrink: 0,
                    p: 0.5,
                    '&:hover': canEdit
                      ? {
                          backgroundColor: moodEntry
                            ? getMoodColor(moodEntry.mood)
                            : theme.palette.grey[400],
                        }
                      : {},
                  }}
                  onClick={() => canEdit && handleMoodClick(date, (moodEntry ? (moodEntry.mood + 1) % Object.keys(MoodValues).length : MoodValues.Happy) as MoodType)} // Cycle through mood types, starting with Happy if no mood exists
                >
                  {moodEntry ? getMoodIcon(moodEntry.mood) : null}
                  {isTodayOrFuture && (
                    <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.7)' }}>
                      {date.getDate()}
                    </Typography>
                  )}
                </Paper>
              );
            })}
          </Box>
        ))}
      </Box>
    );
  };
export default SprintMoodGrid;
