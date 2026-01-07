import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext'; // Update import path
import { CircularProgress, Box } from '@mui/material'; // Import MUI components for loading indicator

interface ProtectedRouteProps {
  children: React.ReactNode;
  adminOnly?: boolean;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, adminOnly = false }) => {
  const { user, isLoading, isSuperAdmin } = useAuth(); // Get isLoading and isSuperAdmin from useAuth

  if (isLoading) {
    // While authentication status is being checked, display a loading indicator
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!user) {
    // User is not authenticated after loading, redirect to login page
    return <Navigate to="/login" replace />;
  }

  if (adminOnly && !isSuperAdmin) {
    // User is not a super admin, redirect to a safe page
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
};

export default ProtectedRoute;

