import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
// Material UI Imports
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

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
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

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
    };

    try {
      await createTeam(newTeam);
      onTeamCreated();
      setName('');
    } catch {
      setError(t('adminTeams.createForm.errorFail'));
    }
  };

  return (
    <Box>
      <Box
        component="form"
        onSubmit={handleSubmit}
        sx={{ display: 'flex', alignItems: 'center', gap: 2 }}
      >
        <TextField
          label={t('adminTeams.createForm.labelName')}
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          variant="outlined"
          size="small"
          sx={{ flexGrow: 1 }}
        />
        <Button type="submit" variant="contained" color="primary">
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
