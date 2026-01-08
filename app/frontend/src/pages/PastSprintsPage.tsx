import React from 'react';
import { Typography, Box } from '@mui/material';

const PastSprintsPage: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Past Sprints
      </Typography>
      <Typography variant="body1">
        This page will display a history of past sprints for all your teams.
      </Typography>
    </Box>
  );
};

export default PastSprintsPage;
