import React from 'react';
import { useTeams } from '../hooks/useTeams';
import TeamListItem from '../components/TeamListItem';

// Material UI Imports
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Grid from '@mui/material/Grid';
// import Box from '@mui/material/Box'; // Removed as it's not directly used here

const DashboardPage: React.FC = () => {
  const { teams, isLoading, isError } = useTeams();

  if (isLoading) return <Typography>Loading teams...</Typography>;
  if (isError) return <Typography color="error">Failed to load teams. Make sure you are logged in.</Typography>;

  return (
    <Container maxWidth="lg" sx={{ mt: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        My Teams
      </Typography>

      {teams && teams.length > 0 ? (
        <Grid container spacing={3}>
          {teams.map(team => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={team.id}> {/* Removed item prop and component="div", used size prop */}
              <TeamListItem team={team} />
            </Grid>
          ))}
        </Grid>
      ) : (
        <Typography variant="body1">You don't belong to any teams yet.</Typography>
      )}
    </Container>
  );
};

export default DashboardPage;

