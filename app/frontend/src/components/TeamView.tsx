import React from 'react';
import type { TeamDto } from '../models/Team';
import { useSprints } from '../hooks/useSprints';
import MoodEntryForm from './MoodEntryForm';
import { Link } from 'react-router-dom';

// Material UI Imports
import { Box, Typography, Button, List, ListItem, Grid, Card, CardContent } from '@mui/material';
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';

interface TeamViewProps {
  team: TeamDto;
}

const TeamView: React.FC<TeamViewProps> = ({ team }) => {
  const { sprints, isLoading, isError, mutateSprints } = useSprints(team.id);

  const handleDataMutation = () => {
    mutateSprints();
  };

  return (
    <Card sx={{ mb: 4, p: 2, boxShadow: 3 }}>
      <CardContent>
        <Typography variant="h5" component="h3" gutterBottom>
          {team.name}
        </Typography>

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

        {isLoading && <Typography>Loading sprints...</Typography>}
        {isError && <Typography color="error">Error loading sprints.</Typography>}

        {sprints && sprints.length > 0 ? (
          <List>
            {sprints.map(sprint => (
              <ListItem key={sprint.id} divider sx={{ flexDirection: 'column', alignItems: 'flex-start', py: 2 }}>
                <Grid container spacing={2} sx={{ width: '100%' }}>
                  <Grid size={{ xs: 12, sm: 6 }}> {/* Removed item prop, used size prop */}
                    <Typography variant="subtitle1">
                      <strong>{sprint.name}</strong>
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      ({new Date(sprint.startDate).toLocaleDateString()} - {new Date(sprint.endDate).toLocaleDateString()})
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}> {/* Removed item prop, used size prop */}
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
