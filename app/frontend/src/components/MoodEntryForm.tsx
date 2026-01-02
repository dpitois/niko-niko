import React, { useState } from 'react';
import { createMoodEntry } from '../services/moodService';
import type { CreateMood } from '../models/CreateMood';
import type { MoodType } from '../models/MoodType';
import { MoodValues } from '../models/MoodType';
import { useAuth } from '../context/AuthContext';
import { useMoods } from '../hooks/useMoods';
import { useSprint } from '../hooks/useSprint'; // Import useSprint hook

// Material UI Imports
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Stack from '@mui/material/Stack';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import { DatePicker } from '@mui/x-date-pickers/DatePicker'; // Import DatePicker
import dayjs, { Dayjs } from 'dayjs'; // Import dayjs and Dayjs type

interface MoodEntryFormProps {
  sprintId: string;
  onMoodEntered: () => void;
}

const MoodEntryForm: React.FC<MoodEntryFormProps> = ({ sprintId, onMoodEntered }) => {
  const { user } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const [selectedDate, setSelectedDate] = useState<Dayjs | null>(dayjs()); // State for the selected date

  const { sprint, isLoading: sprintLoading, isError: sprintError } = useSprint(sprintId);

  const { moods, isLoading: moodsLoading, isError: moodsError, mutateMoods } = useMoods(
    user ? sprintId : null,
    user ? user.sub : null,
    selectedDate ? selectedDate.toISOString().slice(0, 10) : '' // Use selectedDate for fetching moods
  );

  const currentMoodEntry = moods && moods.length > 0 ? moods[0] : null;
  const [selectedMoodValue, setSelectedMoodValue] = useState<MoodType | null>(currentMoodEntry ? currentMoodEntry.mood : null);

  React.useEffect(() => {
    if (currentMoodEntry) {
      setSelectedMoodValue(currentMoodEntry.mood);
    }
  }, [currentMoodEntry]);


  const handleMoodSelect = async (_event: React.MouseEvent<HTMLElement>, newMood: MoodType | null) => {
    setError(null);

    if (!user) {
      setError('You must be logged in to submit your mood.');
      return;
    }

    if (newMood === null) return;

    if (!selectedDate) {
      setError('Please select a date for your mood entry.');
      return;
    }

    if (sprintLoading) {
      setError('Sprint data is still loading. Please try again.');
      return;
    }

    if (sprintError || !sprint) {
      setError('Failed to load sprint data. Cannot submit mood.');
      return;
    }

    const moodDate = selectedDate.startOf('day');
    const sprintStartDate = dayjs(sprint.startDate).startOf('day');
    const sprintEndDate = dayjs(sprint.endDate).startOf('day');
    const today = dayjs().startOf('day');

    if (moodDate.isAfter(today)) {
      setError('Mood entry date cannot be in the future.');
      return;
    }

    if (moodDate.isBefore(sprintStartDate)) {
      setError('Mood entry date cannot be before the sprint start date.');
      return;
    }

    if (moodDate.isAfter(sprintEndDate)) {
      setError('Mood entry date cannot be after the sprint end date.');
      return;
    }

    setSelectedMoodValue(newMood);

    const newMoodEntry: CreateMood = {
      userId: user.sub,
      sprintId,
      mood: newMood,
      date: selectedDate.toISOString(), // Include the selected date
    };

    try {
      await createMoodEntry(newMoodEntry);
      mutateMoods();
      onMoodEntered();
    } catch (err) {
      setError('Failed to save mood. Please try again.');
      console.error(err);
    }
  };

  const getMoodEmoji = (moodValue: MoodType | null) => {
    switch (moodValue) {
      case MoodValues.Happy: return '😊';
      case MoodValues.Neutral: return '😐';
      case MoodValues.Sad: return '😞';
      default: return '❓';
    }
  };

  return (
    <Box sx={{
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 1,
      p: 1.5,
      borderLeft: 3,
      borderColor: 'primary.main',
      borderRadius: 1,
      mt: 2,
    }}>
      <Stack spacing={1} alignItems="center">
        <Typography variant="h6" component="h4">How are you feeling?</Typography>
        {error && <Typography color="error" variant="body2">{error}</Typography>}
        {moodsLoading && <Typography variant="body2">Loading mood...</Typography>}
        {moodsError && <Typography color="error" variant="body2">Error loading mood.</Typography>}
        {sprintLoading && <Typography variant="body2">Loading sprint details...</Typography>}
        {sprintError && <Typography color="error" variant="body2">Error loading sprint details.</Typography>}

        <DatePicker
          label="Select Date"
          value={selectedDate}
          onChange={(newValue) => setSelectedDate(newValue as Dayjs | null)}
          disableFuture
          minDate={sprint ? dayjs(sprint.startDate) : undefined}
          maxDate={sprint ? dayjs(sprint.endDate) : undefined}
        />

        <Stack direction="row" alignItems="center" spacing={1}>
          <Typography variant="body1">Your mood for the selected date:</Typography>
          <Typography variant="h3" sx={{ fontSize: '3rem' }}>
            {currentMoodEntry ? getMoodEmoji(currentMoodEntry.mood) : '—'}
          </Typography>
        </Stack>

        <ToggleButtonGroup
          value={selectedMoodValue}
          exclusive
          onChange={handleMoodSelect}
          aria-label="mood selection"
          disabled={!user || !sprint || selectedDate === null}
        >
          <ToggleButton value={MoodValues.Happy} aria-label="happy">
            😊
          </ToggleButton>
          <ToggleButton value={MoodValues.Neutral} aria-label="neutral">
            😐
          </ToggleButton>
          <ToggleButton value={MoodValues.Sad} aria-label="sad">
            😞
          </ToggleButton>
        </ToggleButtonGroup>
      </Stack>
    </Box>
  );
};

export default MoodEntryForm;
