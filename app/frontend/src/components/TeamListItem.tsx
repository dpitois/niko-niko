import React from 'react';
import type { TeamWithSprints } from '../models/TeamWithSprints';
import MoodEntryForm from './MoodEntryForm';
import { Link } from 'react-router-dom';

// Material UI Imports
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Box from '@mui/material/Box'; // For layout

interface TeamListItemProps {
  team: TeamWithSprints;
}

const TeamListItem: React.FC<TeamListItemProps> = ({ team }) => {
  const now = new Date();

  const currentSprint = team.sprints.find(sprint => {
    const startDate = new Date(sprint.startDate);
    const endDate = new Date(sprint.endDate);
    const utcStartDate = new Date(Date.UTC(startDate.getFullYear(), startDate.getMonth(), startDate.getDate()));
    const utcEndDate = new Date(Date.UTC(endDate.getFullYear(), endDate.getMonth(), endDate.getDate(), 23, 59, 59, 999));
    const utcNow = new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate()));
    
    return utcStartDate <= utcNow && utcEndDate >= utcNow;
  });

  const handleMoodEntered = () => {
    console.log(`Mood submitted for sprint ${currentSprint?.id}`);
  };

  return (
    <Card variant="outlined">
      <CardContent>
        <Typography variant="h5" component="h3" gutterBottom>
          {team.name}
        </Typography>

        {currentSprint ? (
          <Box>
            <Typography variant="body1">Current Sprint: <strong>{currentSprint.name}</strong></Typography>
            <Typography variant="body2">
              ({new Date(currentSprint.startDate).toLocaleDateString()} - {new Date(currentSprint.endDate).toLocaleDateString()})
            </Typography>
            <MoodEntryForm sprintId={currentSprint.id} onMoodEntered={handleMoodEntered} />
          </Box>
        ) : (
          <Typography variant="body1">No active sprint for this team.</Typography>
        )}

        <Box sx={{ mt: 2 }}>
          <Button
            component={Link}
            to={`/sprint/create/${team.id}`}
            variant="contained"
            color="primary"
            fullWidth
          >
            Create New Sprint
          </Button>
        </Box>
      </CardContent>
    </Card>
  );
};

export default TeamListItem;
