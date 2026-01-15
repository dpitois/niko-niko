import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Alert, Box, Button, TextField, Typography } from '@mui/material';
import axios from 'axios';

import { useAuth } from '@/context/AuthContext';
import type { TeamInvitation } from '@/models/Team/Invitation/TeamInvitation';
import { teamInvitationService } from '@/services/teamInvitationService';

interface CreateTeamInvitationFormProps {
  teamId: string;
  onInvitationCreated?: (invitation: TeamInvitation) => void;
}

const CreateTeamInvitationForm: React.FC<CreateTeamInvitationFormProps> = ({
  teamId,
  onInvitationCreated,
}) => {
  const { t } = useTranslation();
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
      setError(t('adminUsers.invitations.createForm.errorLogin'));
      return;
    }

    try {
      const newInvitation = await teamInvitationService.createTeamInvitation({
        teamId,
        expirationInDays,
      });
      const link = `${window.location.origin}/accept-invitation/${newInvitation.token}`;
      setInvitationLink(link);
      setSuccess(t('adminUsers.invitations.createForm.success'));
      if (onInvitationCreated) {
        onInvitationCreated(newInvitation);
      }
    } catch (err: unknown) {
      let errorMessage = t('adminUsers.invitations.createForm.fail');
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        errorMessage = err.response.data.message;
      } else if (err instanceof Error) {
        errorMessage = err.message;
      }
      setError(errorMessage);
    }
  };

  return (
    <Box
      component="form"
      onSubmit={handleSubmit}
      sx={{ mt: 3, p: 2, border: '1px solid #ccc', borderRadius: '8px' }}
    >
      <Typography variant="h6" gutterBottom>
        {t('adminUsers.invitations.createForm.title')}
      </Typography>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      <TextField
        label={t('adminUsers.invitations.createForm.expirationLabel')}
        type="number"
        value={expirationInDays}
        onChange={(e) => setExpirationInDays(Number(e.target.value))}
        fullWidth
        margin="normal"
        inputProps={{ min: 1 }}
      />
      <Button type="submit" variant="contained" color="primary" sx={{ mt: 2 }}>
        {t('adminUsers.invitations.createForm.generateButton')}
      </Button>

      {invitationLink && (
        <Box sx={{ mt: 3, p: 2, bgcolor: 'background.paper', borderRadius: '4px', border: '1px solid', borderColor: 'divider' }}>
          <Typography variant="subtitle1">
            {t('adminUsers.invitations.createForm.linkLabel')}
          </Typography>
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
            {t('common.copyLink')}
          </Button>
        </Box>
      )}
    </Box>
  );
};

export default CreateTeamInvitationForm;
