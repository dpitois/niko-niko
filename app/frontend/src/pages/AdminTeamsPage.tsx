import React from 'react';
import { Box, CircularProgress, Divider, Typography } from '@mui/material';
import { useSnackbar } from 'notistack';

import { useAuth } from '@/context/AuthContext';
import useTeams from '@/hooks/useTeams';
import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';
import { deleteTeam } from '@/services/teamService';

import AdminTeamListItem from '@/components/AdminTeamListItem';
import CreateTeamForm from '@/components/CreateTeamForm';

const AdminTeamsPage: React.FC = () => {
  const { isSuperAdmin } = useAuth();
  const { teams, isLoading, isError, mutate } = useTeams();
  const { enqueueSnackbar } = useSnackbar();

  const handleTeamUpdated = () => {
    mutate();
  };

  const handleDeleteTeam = async (teamId: string) => {
    try {
      await deleteTeam(teamId);
      mutate(); // Refresh the list of teams
    } catch {
      enqueueSnackbar('Failed to delete team.', { variant: 'error' });
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
          <CreateTeamForm onTeamCreated={handleTeamUpdated} />
        </Box>
      )}

      <Divider sx={{ my: 4 }} />

      <Typography variant="h5" component="h2" gutterBottom>
        Manage All Teams
      </Typography>

      {isLoading && <CircularProgress />}
      {isError && <Typography color="error">Error loading teams.</Typography>}

      {teams && teams.length > 0 ? (
        teams.map((team: TeamWithMembersAndSprints) => (
          <AdminTeamListItem
            key={team.id}
            team={team}
            onDelete={handleDeleteTeam}
            onUpdate={handleTeamUpdated}
          />
        ))
      ) : (
        <Typography variant="body1">No teams found.</Typography>
      )}
    </Box>
  );
};

export default AdminTeamsPage;
