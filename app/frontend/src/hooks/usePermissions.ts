import { useAuth } from '@/context/AuthContext';

export const usePermissions = () => {
  const { isSuperAdmin, userTeamRoles } = useAuth();

  const isTeamAdmin = (teamId: string): boolean => {
    return userTeamRoles[teamId]?.isAdmin || false;
  };

  const isTeamMember = (teamId: string): boolean => {
    return userTeamRoles[teamId]?.isMember || false;
  };

  const canCreateTeam = (): boolean => {
    // Only super admins can create teams based on backend policy
    return isSuperAdmin;
  };

  const canManageTeam = (teamId: string): boolean => {
    // Only team admins or super admins can manage a specific team
    return isSuperAdmin || isTeamAdmin(teamId);
  };

  const canDeleteTeam = (teamId: string): boolean => {
    return isSuperAdmin || isTeamAdmin(teamId);
  };

  const canManageTeamInvitations = (teamId: string): boolean => {
    return isSuperAdmin || isTeamAdmin(teamId);
  };

  const canManageTeamMembers = (teamId: string): boolean => {
    return isSuperAdmin || isTeamAdmin(teamId);
  };

  const canViewTeam = (teamId: string): boolean => {
    return isSuperAdmin || isTeamAdmin(teamId) || isTeamMember(teamId);
  };

  return {
    isSuperAdmin,
    isTeamAdmin,
    isTeamMember,
    canCreateTeam,
    canManageTeam,
    canDeleteTeam,
    canManageTeamInvitations,
    canManageTeamMembers,
    canViewTeam,
  };
};
