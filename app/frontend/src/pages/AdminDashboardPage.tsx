import React from 'react';
import CreateTeamForm from '../components/CreateTeamForm';
import { useTeams } from '../hooks/useTeams';
import { useAuth } from '../context/AuthContext'; // Import useAuth
import type { TeamWithMembersAndSprints } from '../models/Team/TeamWithMembersAndSprints'; // Import the team type
import TeamView from '../components/TeamView'; // Import TeamView

// Material UI Imports
import { Container, Typography, Box, Divider } from '@mui/material';

const AdminDashboardPage: React.FC = () => {
  const { user, isSuperAdmin } = useAuth(); // Get current user and super admin status
  const { teams, isLoading, isError, mutateTeams } = useTeams();

  const handleTeamCreated = () => {
    mutateTeams(); // Invalidate SWR cache to re-fetch teams
  };

  const administeredTeams = teams?.filter(team => user && team.adminId === user.sub) || [];

  return (
    <Container maxWidth="lg" sx={{ mt: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Admin Dashboard
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

      {isLoading && <Typography>Loading teams...</Typography>}
      {isError && <Typography color="error">Error loading teams.</Typography>}

      {administeredTeams.length > 0 ? (
        administeredTeams.map((team: TeamWithMembersAndSprints) => (
          <TeamView key={team.id} team={team} />
        ))
      ) : (
        <Typography variant="body1">You are not an administrator of any team.</Typography>
      )}

    </Container>
  );
};

export default AdminDashboardPage;
