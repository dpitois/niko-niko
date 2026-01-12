import React, { useState } from 'react';
import {
  Box,
  Button,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material';

import type { CreateSprint } from '@/models/CreateSprint';
import type { TeamDto } from '@/models/Team';
import { createSprint } from '@/services/sprintService';

interface AdminCreateSprintFormProps {
  teams: TeamDto[];
  onSprintCreated: () => void;
}

const AdminCreateSprintForm: React.FC<AdminCreateSprintFormProps> = ({
  teams,
  onSprintCreated,
}) => {
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [selectedTeamId, setSelectedTeamId] = useState('');
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name || !startDate || !endDate || !selectedTeamId) {
      setError('Please fill in all fields and select a team.');
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
      teamId: selectedTeamId,
    };

    try {
      await createSprint(newSprint);
      onSprintCreated();
      // Reset form
      setName('');
      setStartDate('');
      setEndDate('');
      setSelectedTeamId('');
    } catch {
      setError('Failed to create sprint. Please try again.');
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ mt: 1 }}>
      {error && (
        <Typography color="error" variant="body2" sx={{ mb: 2 }}>
          {error}
        </Typography>
      )}
      <Grid container spacing={2} alignItems="center">
        <Grid size={{ xs: 12, md: 3 }}>
          <FormControl fullWidth required>
            <InputLabel id="team-select-label">Team</InputLabel>
            <Select
              labelId="team-select-label"
              id="team-select"
              value={selectedTeamId}
              label="Team"
              onChange={(e) => setSelectedTeamId(e.target.value)}
            >
              {teams.map((team) => (
                <MenuItem key={team.id} value={team.id}>
                  {team.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <TextField
            id="sprint-name"
            label="Sprint Name"
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            fullWidth
            required
          />
        </Grid>
        <Grid size={{ xs: 6, md: 2 }}>
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
        </Grid>
        <Grid size={{ xs: 6, md: 2 }}>
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
        </Grid>
        <Grid size={{ xs: 12, md: 2 }}>
          <Button
            type="submit"
            variant="contained"
            color="primary"
            fullWidth
            sx={{ height: '56px' }}
          >
            Create
          </Button>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AdminCreateSprintForm;
