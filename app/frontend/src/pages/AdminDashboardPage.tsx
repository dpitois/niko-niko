import React from 'react';
import { Typography, Box } from '@mui/material';

const AdminDashboardPage: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Administration
      </Typography>
      <Typography variant="body1">
        Please select an administration section from the sidebar.
      </Typography>
    </Box>
  );
};

export default AdminDashboardPage;
