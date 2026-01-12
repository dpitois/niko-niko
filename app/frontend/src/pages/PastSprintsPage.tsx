import React from 'react';
import {
  Alert,
  Box,
  Card,
  CardContent,
  CircularProgress,
  Container,
  List,
  ListItem,
  ListItemText,
  Typography,
} from '@mui/material';

import useSprints from '@/hooks/useSprints';
import useTeams from '@/hooks/useTeams';
import type { Sprint } from '@/models/Sprint';

const PastSprintsPage: React.FC = () => {
  const { sprints, isLoading: isLoadingSprints, isError: isErrorSprints } = useSprints();
  const { teams, isLoading: isLoadingTeams, isError: isErrorTeams } = useTeams();

  if (isLoadingSprints || isLoadingTeams) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isErrorSprints || isErrorTeams) {
    return <Alert severity="error">Failed to load data. Make sure you are logged in.</Alert>;
  }

  if (!sprints || !teams) {
    return <Alert severity="info">No sprints or teams data available.</Alert>;
  }

  const today = new Date();
  const pastSprints = sprints.filter((sprint: Sprint) => new Date(sprint.endDate) < today);

  // Helper to get team name
  const getTeamName = (teamId: string): string => {
    return teams.find((team) => team.id === teamId)?.name || 'Unknown Team';
  };

  return (
    <Container maxWidth="lg" sx={{ mt: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Past Sprints
      </Typography>

      {pastSprints.length > 0 ? (
        <List>
          {pastSprints.map((sprint: Sprint) => (
            <Card key={sprint.id} variant="outlined" sx={{ mb: 2 }}>
              <CardContent>
                <ListItem disablePadding>
                  <ListItemText
                    primary={<Typography variant="h6">{sprint.name}</Typography>}
                    secondary={
                      <React.Fragment>
                        <Typography
                          sx={{ display: 'inline' }}
                          component="span"
                          variant="body2"
                          color="text.primary"
                        >
                          Team: {getTeamName(sprint.teamId)}
                        </Typography>
                        {` — From ${new Date(sprint.startDate).toLocaleDateString()} to ${new Date(sprint.endDate).toLocaleDateString()}`}
                      </React.Fragment>
                    }
                  />
                </ListItem>
              </CardContent>
            </Card>
          ))}
        </List>
      ) : (
        <Typography variant="body1">No past sprints found.</Typography>
      )}
    </Container>
  );
};

export default PastSprintsPage;
