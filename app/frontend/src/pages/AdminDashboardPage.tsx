import React from 'react';
import CreateTeamForm from '../components/CreateTeamForm';
import { useTeams } from '../hooks/useTeams'; // To re-fetch teams after creation

// Material UI Imports
import { Container, Typography } from '@mui/material';

const AdminDashboardPage: React.FC = () => {
  const { mutateTeams } = useTeams(); // Function to re-fetch teams

  const handleTeamCreated = () => {
    mutateTeams(); // Invalidate SWR cache to re-fetch teams
  };

  return (
    <Container maxWidth="md" sx={{ mt: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Admin Dashboard
      </Typography>
      <CreateTeamForm onTeamCreated={handleTeamCreated} />
      {/* Other admin functionalities can be added here later */}
    </Container>
  );
};

export default AdminDashboardPage;
