import React, { useState } from 'react';
import { createSprint } from '../services/sprintService';
import type { CreateSprint } from '../models/CreateSprint';
import { Box, TextField, Button, Typography, Stack } from '@mui/material';

interface CreateSprintFormProps {
  teamId: string;
  onSprintCreated: () => void;
}

const CreateSprintForm: React.FC<CreateSprintFormProps> = ({ teamId, onSprintCreated }) => {
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name || !startDate || !endDate) {
      setError('Please fill in all fields.');
      return;
    }

    if (new Date(startDate) >= new Date(endDate)) {
      setError('End date must be after start date.');
      return;
    }

    const newSprint: CreateSprint = {
      name,
      startDate,
      endDate,
      teamId,
    };

    try {
      await createSprint(newSprint);
      onSprintCreated();
      setName('');
      setStartDate('');
      setEndDate('');
    } catch {
      setError('Failed to create sprint. Please try again.');
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ mt: 3, maxWidth: 400, mx: 'auto', p: 3, border: '1px solid #ccc', borderRadius: '8px' }}>
      <Typography variant="h5" component="h3" gutterBottom>
        Create New Sprint
      </Typography>
      {error && (
        <Typography color="error" variant="body2" sx={{ mb: 2 }}>
          {error}
        </Typography>
      )}
      <Stack spacing={2}>
        <TextField
          id="sprint-name"
          label="Sprint Name"
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          fullWidth
          required
        />
        <TextField
          id="start-date"
          label="Start Date"
          type="date"
          value={startDate}
          onChange={(e) => setStartDate(e.target.value)}
          InputLabelProps={{
            shrink: true,
          }}
          fullWidth
          required
        />
        <TextField
          id="end-date"
          label="End Date"
          type="date"
          value={endDate}
          onChange={(e) => setEndDate(e.target.value)}
          InputLabelProps={{
            shrink: true,
          }}
          fullWidth
          required
        />
        <Button type="submit" variant="contained" color="primary" fullWidth>
          Create Sprint
        </Button>
      </Stack>
    </Box>
  );
};

export default CreateSprintForm;
