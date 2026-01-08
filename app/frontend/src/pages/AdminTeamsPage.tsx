import React from 'react';
import CreateTeamForm from '../components/CreateTeamForm';
import { useTeams } from '../hooks/useTeams';
import { useAuth } from '../context/AuthContext';
import AdminTeamListItem from '../components/AdminTeamListItem'; // Use the new component
import { deleteTeam } from '../services/teamService'; // Import deleteTeam service

import { Typography, Box, Divider, CircularProgress } from '@mui/material';

const AdminTeamsPage: React.FC = () => {
  const { isSuperAdmin } = useAuth();
  const { teams, isLoading, isError, mutateTeams } = useTeams();

  const handleTeamCreated = () => {
    mutateTeams();
  };

  const handleDeleteTeam = async (teamId: string) => {
    try {
      await deleteTeam(teamId);
      mutateTeams(); // Refresh the list of teams
    } catch (error) {
      console.error('Failed to delete team:', error);
      // TODO: Show an error message to the user
    }
  };

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Admin Teams
      </Typography>

      {isSuperAdmin && (
        <Box sx={{ mb: 4 }}>
          <Typography variant="h5" component="h2" gutterBottom>
            Create New Team
          </Typography>
          <CreateTeamForm onTeamCreated={handleTeamCreated} />
        </Box>
      )}

      <Divider sx={{ my: 4 }} />

      <Typography variant="h5" component="h2" gutterBottom>
        Manage All Teams
      </Typography>

      {isLoading && <CircularProgress />}
      {isError && <Typography color="error">Error loading teams.</Typography>}

      {teams && teams.length > 0 ? (
        teams.map((team) => (
          <AdminTeamListItem key={team.id} team={team} onDelete={handleDeleteTeam} />
        ))
      ) : (
        <Typography variant="body1">No teams found.</Typography>
      )}
    </Box>
  );
};

export default AdminTeamsPage;