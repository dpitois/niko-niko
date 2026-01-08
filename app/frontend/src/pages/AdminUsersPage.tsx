import React from 'react';
import { Typography, Box } from '@mui/material';

const AdminUsersPage: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Admin Users
      </Typography>
      <Typography variant="body1">
        This page will allow super admins to manage users.
      </Typography>
    </Box>
  );
};

export default AdminUsersPage;
