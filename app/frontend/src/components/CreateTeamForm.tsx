import React, { useState } from 'react';
import { createTeam } from '../services/teamService';
import type { CreateTeam } from '../models/CreateTeam';
import { useAuth } from '../context/AuthContext'; // Update import path

// Material UI Imports
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Stack from '@mui/material/Stack'; // Import Stack

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
      console.error(err);
    }
  };

  return (
    <Box
      component="form"
      onSubmit={handleSubmit}
      sx={{ mt: 3, maxWidth: 400, mx: 'auto', p: 3, border: '1px solid #ccc', borderRadius: '8px' }} // Added styling for consistency
    >
      <Stack spacing={2}> {/* Use Stack for consistent vertical spacing */}
        <Typography variant="h5" component="h3" gutterBottom> {/* Changed variant for consistency */}
          Create New Team
        </Typography>
        {error && (
          <Typography color="error" variant="body2">
            {error}
          </Typography>
        )}
        <TextField
          label="Team Name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          fullWidth
          required
        />
        <Button type="submit" variant="contained" color="primary" fullWidth> {/* Added fullWidth */}
          Create Team
        </Button>
      </Stack>
    </Box>
  );
};

export default CreateTeamForm;