import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Button, Grid, MenuItem, TextField, Typography } from '@mui/material';
import dayjs from 'dayjs';
import axios from 'axios';

import type { CreateSprint } from '@/models/CreateSprint';
import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';
import { createSprint } from '@/services/sprintService';

interface AdminCreateSprintFormProps {
  teams: TeamWithMembersAndSprints[];
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
  const [serverErrors, setServerErrors] = useState<Record<string, string[]>>({});

  const handleTeamChange = (teamId: string) => {
    setSelectedTeamId(teamId);
    if (!teamId) return;

    const team = teams.find((t) => t.id === teamId);
    if (!team) return;

    // Find last sprint
    const lastSprint = team.sprints?.length
      ? [...team.sprints].sort((a, b) => dayjs(b.endDate).valueOf() - dayjs(a.endDate).valueOf())[0]
      : null;

    const nextStart = lastSprint ? dayjs(lastSprint.endDate).add(1, 'day') : dayjs();
    setStartDate(nextStart.format('YYYY-MM-DD'));

    if (team.sprintNameTemplate) {
      let newName = team.sprintNameTemplate;
      const nextDate = nextStart;
      newName = newName.replace(/\[yyyy\]/g, nextDate.format('YYYY'));
      newName = newName.replace(/\[yy\]/g, nextDate.format('YY'));
      newName = newName.replace(/\[MM\]/g, nextDate.format('MM'));
      newName = newName.replace(/\[team\]/g, team.name);
      // Simple ID logic: count + 1
      const nextId = (team.sprints?.length || 0) + 1;
      newName = newName.replace(/\[id\]/g, nextId.toString());
      setName(newName);
    }

    if (team.defaultSprintDuration) {
      // Subtract 1 day because duration is inclusive
      setEndDate(nextStart.add(team.defaultSprintDuration - 1, 'day').format('YYYY-MM-DD'));
    } else {
      setEndDate('');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setServerErrors({});

    if (!name || !startDate || !endDate || !selectedTeamId) {
      setError(t('adminSprints.createForm.errorFill'));
      return;
    }

    if (dayjs(startDate).isSameOrAfter(dayjs(endDate), 'day')) {
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
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.status === 400 && err.response.data.errors) {
        setServerErrors(err.response.data.errors);
      } else {
        setError(t('adminSprints.createForm.errorFail'));
      }
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ mt: 1 }}>
      {error && (
        <Typography color="error" variant="body2" sx={{ mb: 2 }}>
          {error}
        </Typography>
      )}
      <Grid container spacing={2} alignItems="flex-start">
        <Grid size={{ xs: 12, md: 3 }}>
          <TextField
            select
            id="team-select"
            label={t('adminSprints.createForm.teamLabel')}
            value={selectedTeamId}
            onChange={(e) => handleTeamChange(e.target.value)}
            fullWidth
            required
            error={!!serverErrors['TeamId']}
            helperText={serverErrors['TeamId'] ? t(serverErrors['TeamId'][0]) : ''}
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
            error={!!serverErrors['Name']}
            helperText={serverErrors['Name'] ? t(serverErrors['Name'][0]) : ''}
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
            error={!!serverErrors['StartDate']}
            helperText={serverErrors['StartDate'] ? t(serverErrors['StartDate'][0]) : ''}
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
            error={!!serverErrors['EndDate']}
            helperText={serverErrors['EndDate'] ? t(serverErrors['EndDate'][0]) : ''}
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
