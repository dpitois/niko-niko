import React from 'react';
import { useTeams } from '../hooks/useTeams';
import { useAuth } from '../context/AuthContext';
import CreateTeamInvitationForm from '../components/CreateTeamInvitationForm';
import { teamInvitationService } from '../services/teamInvitationService';

import {
  Typography,
  Box,
  Divider,
  List,
  ListItem,
  Grid,
  Alert,
  IconButton,
  Button,
  CircularProgress
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import { useSnackbar } from 'notistack';
import useSWR from 'swr';
import type { TeamInvitation } from '../models/Team/Invitation/TeamInvitation';
import axios from 'axios';


const TeamInvitationsPage: React.FC = () => {
  const { user, isSuperAdmin } = useAuth();
  const { teams, isLoading: isLoadingTeams, isError: isErrorTeams } = useTeams();
  const { enqueueSnackbar } = useSnackbar();

  const [selectedTeamId, setSelectedTeamId] = React.useState<string | null>(null);

  React.useEffect(() => {
    if (teams && teams.length > 0 && !selectedTeamId) {
      // Set the first team as selected by default if none is selected
      setSelectedTeamId(teams[0].id);
    }
  }, [teams, selectedTeamId, teams?.length]); // Added teams?.length to dependencies


  const canManageTeam = (teamId: string) => {
    const team = teams?.find(t => t.id === teamId);
    return (user && team?.adminId === user.sub) || isSuperAdmin;
  };

  const { data: invitations, isLoading: loadingInvitations, error: invitationsError, mutate: mutateTeamInvitations } = useSWR<TeamInvitation[]>(
    selectedTeamId && canManageTeam(selectedTeamId) ? `/teams/${selectedTeamId}/invitations` : null,
    () => teamInvitationService.getTeamInvitations(selectedTeamId!)
  );

  const handleInvitationCreated = () => {
    mutateTeamInvitations();
  };

  const handleDeleteInvitation = async (invitationId: string) => {
    try {
      await teamInvitationService.deleteTeamInvitation(invitationId);
      enqueueSnackbar('Invitation deleted successfully!', { variant: 'success' });
      mutateTeamInvitations();
    } catch (error: unknown) {
      let errorMessage = 'Failed to delete invitation.';
      if (axios.isAxiosError(error) && error.response?.data?.message) {
        errorMessage = error.response.data.message;
      } else if (error instanceof Error) {
        errorMessage = error.message;
      }
      enqueueSnackbar(errorMessage, { variant: 'error' });
    }
  };

  if (isLoadingTeams) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isErrorTeams) {
    return <Typography color="error">Failed to load teams. Make sure you are logged in.</Typography>;
  }

  const administeredTeams = teams?.filter(team => user && team.adminId === user.sub) || [];

  if (administeredTeams.length === 0) {
    return (
      <Box sx={{ mt: 2 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Team Invitations
        </Typography>
        <Typography variant="body1">
          You are not an administrator of any team to manage invitations.
        </Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ mt: 2 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Team Invitations
      </Typography>

      <Box sx={{ mb: 3 }}>
        <Typography variant="h5" component="h2" gutterBottom>
          Select a Team
        </Typography>
        <select
          value={selectedTeamId || ''}
          onChange={(e) => setSelectedTeamId(e.target.value)}
          style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ccc', marginBottom: '16px' }}
        >
          <option value="">Select a team</option>
          {administeredTeams.map((team) => (
            <option key={team.id} value={team.id}>{team.name}</option>
          ))}
        </select>
      </Box>

      {selectedTeamId && canManageTeam(selectedTeamId) && (
        <>
          <Box sx={{ mb: 3 }}>
            <CreateTeamInvitationForm teamId={selectedTeamId} onInvitationCreated={handleInvitationCreated} />
          </Box>
          <Divider sx={{ my: 3 }} />
          <Typography variant="h6" gutterBottom>Team Invitations</Typography>
          {loadingInvitations && <CircularProgress />}
          {invitationsError && <Alert severity="error">{invitationsError?.message || 'Failed to load invitations.'}</Alert>}
          {!loadingInvitations && invitations && invitations.length > 0 ? (
            <List>
              {invitations.map((invitation) => (
                <ListItem key={invitation.id} secondaryAction={
                  <IconButton edge="end" aria-label="delete" onClick={() => handleDeleteInvitation(invitation.id)}>
                    <DeleteIcon />
                  </IconButton>
                }>
                  <Grid container spacing={2} alignItems="center">
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <Typography variant="body1">Token: {invitation.token.substring(0, 10)}...</Typography>
                      <Typography variant="body2" color="text.secondary">Expires: {new Date(invitation.expirationDate).toLocaleDateString()}</Typography>
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <Typography variant="body2" color={invitation.status === 'Accepted' ? 'success.main' : invitation.status === 'Expired' ? 'error.main' : 'info.main'}>
                        Status: {invitation.status}
                      </Typography>
                      <Button variant="outlined" size="small" onClick={() => navigator.clipboard.writeText(`${window.location.origin}/accept-invitation/${invitation.token}`)} sx={{ mt: 1 }}>
                        Copy Link
                      </Button>
                    </Grid>
                  </Grid>
                </ListItem>
              ))}
            </List>
          ) : (!loadingInvitations && !invitationsError && (
            <Typography>No invitations found for this team.</Typography>
          ))}
        </>
      )}
    </Box>
  );
};

export default TeamInvitationsPage;