import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { usePermissions } from '../hooks/usePermissions'; // Import the new hook
import { CircularProgress, Box } from '@mui/material';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredSuperAdmin?: boolean; // Replaces adminOnly
  requiredTeamAdminOf?: string; // Requires the user to be admin of this teamId
  requiredTeamMemberOf?: string; // Requires the user to be a member of this teamId
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requiredSuperAdmin = false,
  requiredTeamAdminOf,
  requiredTeamMemberOf,
}) => {
  const { user, isLoading } = useAuth();
  const { isSuperAdmin, isTeamAdmin, isTeamMember } = usePermissions();

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // Check Super Admin requirement
  if (requiredSuperAdmin && !isSuperAdmin) {
    return <Navigate to="/dashboard" replace />; // Redirect if not super admin
  }

  // Check Team Admin requirement
  if (requiredTeamAdminOf && !isTeamAdmin(requiredTeamAdminOf)) {
    return <Navigate to="/dashboard" replace />; // Redirect if not team admin
  }

  // Check Team Member requirement
  if (requiredTeamMemberOf && !isTeamMember(requiredTeamMemberOf)) {
    return <Navigate to="/dashboard" replace />; // Redirect if not team member
  }

  return <>{children}</>;
};

export default ProtectedRoute;

