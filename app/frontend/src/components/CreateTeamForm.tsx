import React, { useState } from 'react';
import { createTeam } from '../services/teamService';
import type { CreateTeam } from '../models/CreateTeam';
import { useAuth } from '../context/AuthContext';

// Material UI Imports
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';

interface CreateTeamFormProps {
  onTeamCreated: () => void;
}

const CreateTeamForm: React.FC<CreateTeamFormProps> = ({ onTeamCreated }) => {
  const { user } = useAuth();
  const [name, setName] = useState('');
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name) {
      setError('Please fill in the team name.');
      return;
    }

    if (!user) {
      setError('You must be logged in to create a team.');
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
    } catch (err) {
      setError('Failed to create team. Please try again.');
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
          label="New Team Name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          variant="outlined"
          size="small"
          sx={{ flexGrow: 1 }}
        />
        <Button type="submit" variant="contained" color="primary">
          Create Team
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