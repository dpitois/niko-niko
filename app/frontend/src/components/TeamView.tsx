import React from 'react'; // Retirer useState, useEffect
import type { TeamWithMembersAndSprints } from '../models/Team/TeamWithMembersAndSprints';
import { useSprints } from '../hooks/useSprints';
import MoodEntryForm from './MoodEntryForm';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import CreateTeamInvitationForm from './CreateTeamInvitationForm';
import { teamInvitationService } from '../services/teamInvitationService';
// import type { TeamInvitation } from '../models/Team/Invitation/TeamInvitation'; // Plus nécessaire directement ici

// Material UI Imports
import { Box, Typography, Button, List, ListItem, Grid, Card, CardContent, Divider, Alert, IconButton } from '@mui/material'; // Ajout de IconButton
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import DeleteIcon from '@mui/icons-material/Delete'; // Ajout de DeleteIcon
import { useSnackbar } from 'notistack'; // Ajout de useSnackbar
import useSWR from 'swr'; // Ajout de useSWR

interface TeamViewProps {
  team: TeamWithMembersAndSprints;
}

const TeamView: React.FC<TeamViewProps> = ({ team }) => {
  const { user } = useAuth();
  const { sprints, isLoading, isError, mutateSprints } = useSprints(team.id); // Correction ici: utiliser mutateSprints directement
  const { enqueueSnackbar } = useSnackbar();

  // Utiliser useSWR pour les invitations
  const { data: invitations, isLoading: loadingInvitations, error: invitationsError, mutate: mutateTeamInvitations } = useSWR( // Renommer mutate en mutateTeamInvitations
    user && team.adminId === user.sub ? `/teams/${team.id}/invitations` : null, // La clé SWR est null si l'utilisateur n'est pas admin, donc ne déclenche pas la récupération
    () => teamInvitationService.getTeamInvitations(team.id)
  );

  const isTeamAdmin = user && team.adminId === user.sub;

  const handleInvitationCreated = () => {
    mutateTeamInvitations(); // Rafraîchir la liste des invitations après création
  };

  const handleDeleteInvitation = async (invitationId: string) => {
    try {
      await teamInvitationService.deleteTeamInvitation(invitationId);
      enqueueSnackbar('Invitation deleted successfully!', { variant: 'success' });
      mutateTeamInvitations(); // Rafraîchir la liste après suppression
    } catch (error: any) {
      enqueueSnackbar(error.response?.data?.message || 'Failed to delete invitation.', { variant: 'error' });
    }
  };

  const handleDataMutation = () => {
    mutateSprints();
  };

  return (
    <Card sx={{ mb: 4, p: 2, boxShadow: 3 }}>
      <CardContent>
        <Typography variant="h5" component="h3" gutterBottom>
          {team.name}
        </Typography>

        {isTeamAdmin && (
          <Box sx={{ mb: 3 }}>
            <CreateTeamInvitationForm teamId={team.id} onInvitationCreated={handleInvitationCreated} />

            <Divider sx={{ my: 3 }} />

            <Typography variant="h6" gutterBottom>
              Team Invitations
            </Typography>
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
                        <Typography variant="body2" color="text.secondary">
                          Created by: {invitation.creatorUserName}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Expires: {new Date(invitation.expirationDate).toLocaleDateString()}
                        </Typography>
                      </Grid>
                      <Grid size={{ xs: 12, sm: 6 }}>
                        <Typography variant="body2" color={invitation.status === 'Accepted' ? 'success.main' : invitation.status === 'Expired' ? 'error.main' : 'info.main'}>
                          Status: {invitation.status}
                        </Typography>
                        <Button
                          variant="outlined"
                          size="small"
                          onClick={() => navigator.clipboard.writeText(`${window.location.origin}/accept-invitation/${invitation.token}`)}
                          sx={{ mt: 1 }}
                        >
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
          <Typography variant="h6" component="h4" sx={{ mr: 1 }}>
            Sprints
          </Typography>
          <Button
            component={Link}
            to={`/sprint/create/${team.id}`}
            variant="outlined"
            size="small"
            startIcon={<AddCircleOutlineIcon />}
          >
            Add Sprint
          </Button>
        </Box>

        {team.members && team.members.length > 0 && (
          <Box sx={{ mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Team Members
            </Typography>
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
                    <Typography variant="subtitle1">
                      <strong>{sprint.name}</strong>
                    </Typography>
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
