import React from 'react';
import type { TeamWithMembersAndSprints } from '../models/Team/TeamWithMembersAndSprints';
import { useSprints } from '../hooks/useSprints';
import MoodEntryForm from './MoodEntryForm';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import CreateTeamInvitationForm from './CreateTeamInvitationForm';
import { teamInvitationService } from '../services/teamInvitationService';
import { deleteTeam } from '../services/teamService'; // Import deleteTeam
import { Box, Typography, Button, List, ListItem, Grid, Card, CardContent, Divider, Alert, IconButton } from '@mui/material';
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import DeleteIcon from '@mui/icons-material/Delete';
import { useSnackbar } from 'notistack';
import useSWR from 'swr';

interface TeamViewProps {
  team: TeamWithMembersAndSprints;
}

const TeamView: React.FC<TeamViewProps> = ({ team }) => {
  const { user, isSuperAdmin } = useAuth();
  const { sprints, isLoading, isError, mutateSprints } = useSprints(team.id);
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();

  const canManageTeam = (user && team.adminId === user.sub) || isSuperAdmin;

  const { data: invitations, isLoading: loadingInvitations, error: invitationsError, mutate: mutateTeamInvitations } = useSWR(
    canManageTeam ? `/teams/${team.id}/invitations` : null,
    () => teamInvitationService.getTeamInvitations(team.id)
  );

  const handleInvitationCreated = () => {
    mutateTeamInvitations();
  };

  const handleDeleteInvitation = async (invitationId: string) => {
    try {
      await teamInvitationService.deleteTeamInvitation(invitationId);
      enqueueSnackbar('Invitation deleted successfully!', { variant: 'success' });
      mutateTeamInvitations();
    } catch (error: any) {
      enqueueSnackbar(error.response?.data?.message || 'Failed to delete invitation.', { variant: 'error' });
    }
  };
  
  const handleDeleteTeam = async () => {
    if (window.confirm(`Are you sure you want to delete the team "${team.name}"? This action cannot be undone.`)) {
      try {
        await deleteTeam(team.id);
        enqueueSnackbar('Team deleted successfully!', { variant: 'success' });
        // After deletion, you might want to redirect or refresh the list of teams
        navigate('/dashboard'); // Redirect to dashboard
        window.location.reload(); // Force a reload to refresh team list on dashboard
      } catch (error: any) {
        enqueueSnackbar(error.response?.data?.message || 'Failed to delete the team.', { variant: 'error' });
      }
    }
  };

  const handleDataMutation = () => {
    mutateSprints();
  };

  return (
    <Card sx={{ mb: 4, p: 2, boxShadow: 3 }}>
      <CardContent>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="h5" component="h3" gutterBottom>
            {team.name}
          </Typography>
          {canManageTeam && (
            <Button
              variant="outlined"
              color="error"
              size="small"
              startIcon={<DeleteIcon />}
              onClick={handleDeleteTeam}
            >
              Delete Team
            </Button>
          )}
        </Box>

        {canManageTeam && (
          <Box sx={{ mb: 3 }}>
            <CreateTeamInvitationForm teamId={team.id} onInvitationCreated={handleInvitationCreated} />
            <Divider sx={{ my: 3 }} />
            <Typography variant="h6" gutterBottom>Team Invitations</Typography>
            {loadingInvitations && <Typography>Loading invitations...</Typography>}
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
          </Box>
        )}

        <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
          <Typography variant="h6" component="h4" sx={{ mr: 1 }}>Sprints</Typography>
          {canManageTeam && (
            <Button component={Link} to={`/sprint/create/${team.id}`} variant="outlined" size="small" startIcon={<AddCircleOutlineIcon />}>
              Add Sprint
            </Button>
          )}
        </Box>

        {team.members && team.members.length > 0 && (
          <Box sx={{ mb: 3 }}>
            <Typography variant="h6" gutterBottom>Team Members</Typography>
            <List>
              {team.members.map((member) => (
                <ListItem key={member.id} divider>
                  <Typography variant="body1">{member.name} ({member.email})</Typography>
                </ListItem>
              ))}
            </List>
          </Box>
        )}

        {isLoading && <Typography>Loading sprints...</Typography>}
        {isError && <Typography color="error">Error loading sprints.</Typography>}

        {sprints && sprints.length > 0 ? (
          <List>
            {sprints.map(sprint => (
              <ListItem key={sprint.id} divider sx={{ flexDirection: 'column', alignItems: 'flex-start', py: 2 }}>
                <Grid container spacing={2} sx={{ width: '100%' }}>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="subtitle1"><strong>{sprint.name}</strong></Typography>
                    <Typography variant="body2" color="text.secondary">
                      ({new Date(sprint.startDate).toLocaleDateString()} - {new Date(sprint.endDate).toLocaleDateString()})
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <MoodEntryForm sprintId={sprint.id} onMoodEntered={handleDataMutation} />
                  </Grid>
                </Grid>
              </ListItem>
            ))}
          </List>
        ) : (
          <Typography>No sprints found for this team.</Typography>
        )}
      </CardContent>
    </Card>
  );
};

export default TeamView;
