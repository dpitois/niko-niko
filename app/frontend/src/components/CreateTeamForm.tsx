import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
// Material UI Imports
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import axios from 'axios';

import { useAuth } from '@/context/AuthContext';
import type { CreateTeam } from '@/models/CreateTeam';
import { createTeam } from '@/services/teamService';

interface CreateTeamFormProps {
  onTeamCreated: () => void;
}

const CreateTeamForm: React.FC<CreateTeamFormProps> = ({ onTeamCreated }) => {
  const { t } = useTranslation();
  const { user } = useAuth();
  const [name, setName] = useState('');
  const [defaultSprintDuration, setDefaultSprintDuration] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [serverErrors, setServerErrors] = useState<Record<string, string[]>>({});

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setServerErrors({});

    if (!name) {
      setError(t('adminTeams.createForm.errorFill'));
      return;
    }

    if (!user) {
      setError(t('adminTeams.createForm.errorLogin'));
      return;
    }

    const newTeam: CreateTeam = {
      name,
      adminId: user.sub,
      defaultSprintDuration: defaultSprintDuration
        ? parseInt(defaultSprintDuration, 10)
        : undefined,
    };

    try {
      await createTeam(newTeam);
      onTeamCreated();
      setName('');
      setDefaultSprintDuration('');
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.status === 400 && err.response.data.errors) {
        setServerErrors(err.response.data.errors);
      } else {
        setError(t('adminTeams.createForm.errorFail'));
      }
    }
  };

  return (
    <Box>
      <Box
        component="form"
        onSubmit={handleSubmit}
        sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}
      >
        <TextField
          label={t('adminTeams.createForm.labelName')}
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          variant="outlined"
          size="small"
          sx={{ flexGrow: 1 }}
          error={!!serverErrors['Name']}
          helperText={serverErrors['Name'] ? t(serverErrors['Name'][0]) : ''}
        />
        <TextField
          label={t('adminTeams.editDialog.labelDefaultSprintDuration')}
          type="number"
          value={defaultSprintDuration}
          onChange={(e) => setDefaultSprintDuration(e.target.value)}
          variant="outlined"
          size="small"
          slotProps={{ htmlInput: { min: 1 } }}
          sx={{ width: '200px' }}
          error={!!serverErrors['DefaultSprintDuration']}
          helperText={
            serverErrors['DefaultSprintDuration'] ? t(serverErrors['DefaultSprintDuration'][0]) : ''
          }
        />
        <Button type="submit" variant="contained" color="primary" sx={{ height: '40px' }}>
          {t('adminTeams.createForm.createButton')}
        </Button>
      </Box>
      {error && (
        <Typography color="error" variant="body2" sx={{ mt: 1 }}>
          {error}
        </Typography>
      )}
    </Box>
  );
};

export default CreateTeamForm;
