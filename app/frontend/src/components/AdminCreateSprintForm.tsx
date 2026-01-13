import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Button, Grid, MenuItem, TextField, Typography } from '@mui/material';

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
  const { t } = useTranslation();
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [selectedTeamId, setSelectedTeamId] = useState('');
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name || !startDate || !endDate || !selectedTeamId) {
      setError(t('adminSprints.createForm.errorFill'));
      return;
    }

    if (new Date(startDate) >= new Date(endDate)) {
      setError(t('adminSprints.createForm.errorDate'));
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
      setError(t('adminSprints.createForm.errorFail'));
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
          <TextField
            select
            id="team-select"
            label={t('adminSprints.createForm.teamLabel')}
            value={selectedTeamId}
            onChange={(e) => setSelectedTeamId(e.target.value)}
            fullWidth
            required
          >
            {teams.map((team) => (
              <MenuItem key={team.id} value={team.id}>
                {team.name}
              </MenuItem>
            ))}
          </TextField>
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <TextField
            id="sprint-name"
            label={t('adminSprints.createForm.nameLabel')}
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
            label={t('adminSprints.createForm.startDateLabel')}
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
            label={t('adminSprints.createForm.endDateLabel')}
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
            {t('adminSprints.createForm.createButton')}
          </Button>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AdminCreateSprintForm;
