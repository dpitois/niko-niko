import React from 'react';
import { Navigate } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';

import { useAuth } from '@/context/AuthContext';
import { usePermissions } from '@/hooks/usePermissions';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredSuperAdmin?: boolean; // Replaces adminOnly
  requiredAnyAdmin?: boolean; // Requires the user to be a Super Admin or a Team Admin
  requiredTeamAdminOf?: string; // Requires the user to be admin of this teamId
  requiredTeamMemberOf?: string; // Requires the user to be a member of this teamId
  skipOnboardingCheck?: boolean; // New prop to skip onboarding check
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requiredSuperAdmin = false,
  requiredAnyAdmin = false,
  requiredTeamAdminOf,
  requiredTeamMemberOf,
  skipOnboardingCheck = false,
}) => {
  const { user, isLoading, isOnboarded } = useAuth();
  const { isSuperAdmin, isAnyTeamAdmin, isTeamAdmin, isTeamMember } = usePermissions();

  if (isLoading) {
    return (
      <Box
        sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}
      >
        <CircularProgress />
      </Box>
    );
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // Check Onboarding requirement
  if (!isOnboarded && !skipOnboardingCheck) {
    return <Navigate to="/onboarding" replace />;
  }

  // Check Super Admin requirement
  if (requiredSuperAdmin && !isSuperAdmin) {
    return <Navigate to="/my-teams" replace />; // Redirect if not super admin
  }

  // Check Any Admin requirement (Super Admin or any Team Admin)
  if (requiredAnyAdmin && !isSuperAdmin && !isAnyTeamAdmin()) {
    return <Navigate to="/my-teams" replace />;
  }

  // Check Team Admin requirement
  if (requiredTeamAdminOf && !isTeamAdmin(requiredTeamAdminOf)) {
    return <Navigate to="/my-teams" replace />; // Redirect if not team admin
  }

  // Check Team Member requirement
  if (requiredTeamMemberOf && !isTeamMember(requiredTeamMemberOf)) {
    return <Navigate to="/my-teams" replace />; // Redirect if not team member
  }

  return <>{children}</>;
};

export default ProtectedRoute;
