import React from 'react';
import { useParams } from 'react-router-dom';
import MoodEntryForm from '../components/MoodEntryForm';

// Material UI Imports
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box'; // For spacing

const MoodEntryStandalonePage: React.FC = () => {
  const { sprintId } = useParams<{ sprintId: string }>();

  if (!sprintId) {
    return (
      <Container maxWidth="sm" sx={{ mt: 4 }}>
        <Typography color="error" variant="h6">Error: Sprint ID is missing.</Typography>
      </Container>
    );
  }

  const handleMoodEntered = () => {
    console.log(`Mood submitted for sprint ${sprintId}`);
    // Potentially show a toast notification here
  };

  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Mood Entry for Sprint ID: {sprintId}
      </Typography>
      <Box sx={{ mt: 3 }}>
        <MoodEntryForm sprintId={sprintId} onMoodEntered={handleMoodEntered} />
      </Box>
    </Container>
  );
};

export default MoodEntryStandalonePage;
