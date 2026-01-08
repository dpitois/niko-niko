import React from 'react';
import type { TeamWithMembersAndSprints } from '../models/Team/TeamWithMembersAndSprints';
import { useSprints } from '../hooks/useSprints';
import MoodEntryForm from './MoodEntryForm';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { deleteTeam } from '../services/teamService'; // Import deleteTeam
import { Box, Typography, Button, List, ListItem, Grid, Card, CardContent, CircularProgress } from '@mui/material';
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import DeleteIcon from '@mui/icons-material/Delete';
import { useSnackbar } from 'notistack';
import axios from 'axios';

interface TeamViewProps {
  team: TeamWithMembersAndSprints;
}

const TeamView: React.FC<TeamViewProps> = ({ team }) => {
  const { user, isSuperAdmin } = useAuth();
  const { sprints, isLoading, isError, mutateSprints } = useSprints(team.id);
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();

  const canManageTeam = (user && team.adminId === user.sub) || isSuperAdmin;
  
  const handleDeleteTeam = async () => {
    if (window.confirm(`Are you sure you want to delete the team "${team.name}"? This action cannot be undone.`)) {
      try {
        await deleteTeam(team.id);
        enqueueSnackbar('Team deleted successfully!', { variant: 'success' });
        navigate('/admin/teams'); // Redirect to admin teams page
      } catch (error: unknown) {
        let errorMessage = 'Failed to delete the team.';
        if (axios.isAxiosError(error) && error.response?.data?.message) {
          errorMessage = error.response.data.message;
        } else if (error instanceof Error) {
          errorMessage = error.message;
        }
        enqueueSnackbar(errorMessage, { variant: 'error' });
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

        {isLoading && <CircularProgress />}
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
