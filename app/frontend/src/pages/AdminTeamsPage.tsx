import React from 'react';
import CreateTeamForm from '../components/CreateTeamForm';
import { useTeams } from '../hooks/useTeams';
import { useAuth } from '../context/AuthContext';
import type { TeamWithMembersAndSprints } from '../models/Team/TeamWithMembersAndSprints';
import TeamView from '../components/TeamView';

import { Typography, Box, Divider, CircularProgress } from '@mui/material';

const AdminTeamsPage: React.FC = () => {
  const { user, isSuperAdmin } = useAuth();
  const { teams, isLoading, isError, mutateTeams } = useTeams();

  const handleTeamCreated = () => {
    mutateTeams();
  };

  const administeredTeams = teams?.filter(team => user && team.adminId === user.sub) || [];

  return (
    <Box sx={{ mt: 2 }}>
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
        Manage My Administered Teams
      </Typography>

      {isLoading && <CircularProgress />}
      {isError && <Typography color="error">Error loading teams.</Typography>}

      {administeredTeams.length > 0 ? (
        administeredTeams.map((team: TeamWithMembersAndSprints) => (
          <TeamView key={team.id} team={team} />
        ))
      ) : (
        <Typography variant="body1">You are not an administrator of any team.</Typography>
      )}
    </Box>
  );
};

export default AdminTeamsPage;