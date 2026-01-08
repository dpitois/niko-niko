import React from 'react';
import { Typography, Box } from '@mui/material';

const AdminSprintsPage: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Admin Sprints
      </Typography>
      <Typography variant="body1">
        This page will allow super admins to manage sprints.
      </Typography>
    </Box>
  );
};

export default AdminSprintsPage;
