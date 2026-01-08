import React, { useState } from 'react';
import { Button, TextField, Box, Typography, Alert } from '@mui/material';
import { useAuth } from '../context/AuthContext';
import { teamInvitationService } from '../services/teamInvitationService';
import type { TeamInvitation } from '../models/Team/Invitation/TeamInvitation';
import axios from 'axios';

interface CreateTeamInvitationFormProps {
  teamId: string;
  onInvitationCreated?: (invitation: TeamInvitation) => void;
}

const CreateTeamInvitationForm: React.FC<CreateTeamInvitationFormProps> = ({ teamId, onInvitationCreated }) => {
  const { user } = useAuth();
  const [expirationInDays, setExpirationInDays] = useState<number>(7);
  const [invitationLink, setInvitationLink] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    setInvitationLink(null);

    if (!user) {
      setError('You must be logged in to create an invitation.');
      return;
    }

    try {
      const newInvitation = await teamInvitationService.createTeamInvitation({ teamId, expirationInDays });
      const link = `${window.location.origin}/accept-invitation/${newInvitation.token}`;
      setInvitationLink(link);
      setSuccess('Invitation created successfully!');
      if (onInvitationCreated) {
        onInvitationCreated(newInvitation);
      }
    } catch (err: unknown) {
      let errorMessage = 'Failed to create invitation.';
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        errorMessage = err.response.data.message;
      } else if (err instanceof Error) {
        errorMessage = err.message;
      }
      setError(errorMessage);
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ mt: 3, p: 2, border: '1px solid #ccc', borderRadius: '8px' }}>
      <Typography variant="h6" gutterBottom>
        Create Team Invitation
      </Typography>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      <TextField
        label="Expiration in Days"
        type="number"
        value={expirationInDays}
        onChange={(e) => setExpirationInDays(Number(e.target.value))}
        fullWidth
        margin="normal"
        inputProps={{ min: 1 }}
      />
      <Button type="submit" variant="contained" color="primary" sx={{ mt: 2 }}>
        Generate Invitation Link
      </Button>

      {invitationLink && (
        <Box sx={{ mt: 3, p: 2, bgcolor: '#f0f0f0', borderRadius: '4px' }}>
          <Typography variant="subtitle1">Invitation Link:</Typography>
          <TextField
            fullWidth
            value={invitationLink}
            InputProps={{
              readOnly: true,
            }}
            variant="outlined"
            size="small"
            sx={{ mt: 1 }}
          />
          <Button
            variant="outlined"
            size="small"
            onClick={() => navigator.clipboard.writeText(invitationLink)}
            sx={{ mt: 1 }}
          >
            Copy Link
          </Button>
        </Box>
      )}
    </Box>
  );
};

export default CreateTeamInvitationForm;
