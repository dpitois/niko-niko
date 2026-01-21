/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useCallback, useContext, useEffect, useState } from 'react';
import { jwtDecode } from 'jwt-decode';

import type { DecodedToken, TeamRole } from '@/models/Auth';
import type { TeamWithSprintsDto } from '@/models/Team/TeamWithSprintsDto';
import type { User } from '@/models/User';
import api, { setLogoutHandler } from '@/services/api';

interface AuthContextType {
  user: DecodedToken | null;
  isSuperAdmin: boolean;
  isOnboarded: boolean;
  userTeamRoles: { [teamId: string]: TeamRole }; // Map of teamId to roles
  login: (token: string) => void;
  logout: () => void;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<DecodedToken | null>(null);
  const [isSuperAdmin, setIsSuperAdmin] = useState(false);
  const [isOnboarded, setIsOnboarded] = useState(false);
  const [userTeamRoles, setUserTeamRoles] = useState<{ [teamId: string]: TeamRole }>({});
  const [isLoading, setIsLoading] = useState(true);

  const fetchUserTeamRoles = useCallback(async (userId: string) => {
    try {
      // Token is handled by api interceptor
      const response = await api.get<TeamWithSprintsDto[]>('/teams');

      const roles: { [teamId: string]: TeamRole } = {};
      response.data.forEach((team) => {
        const isAdmin = team.adminId === userId;
        const isMember = team.members.some((member: User) => member.id === userId);
        roles[team.id] = { isAdmin, isMember };
      });
      setUserTeamRoles(roles);
    } catch (error) {
      console.error('Error fetching user team roles:', error);
      setUserTeamRoles({});
    }
  }, []);

  const login = useCallback(
    async (token: string) => {
      localStorage.setItem('jwt_token', token);
      setIsLoading(true);
      try {
        const decoded = jwtDecode<DecodedToken>(token);
        setUser(decoded);
        setIsSuperAdmin(decoded.is_super_admin === 'true');
        setIsOnboarded(decoded.is_onboarded === 'true');
        await fetchUserTeamRoles(decoded.sub);
      } catch (error) {
        console.error('Invalid token during login:', error);
        localStorage.removeItem('jwt_token');
        setUser(null);
        setIsSuperAdmin(false);
        setIsOnboarded(false);
        setUserTeamRoles({});
      } finally {
        setIsLoading(false);
      }
    },
    [fetchUserTeamRoles],
  );

  const logout = useCallback(async () => {
    try {
      await api.post('/auth/logout');
    } catch (error) {
      console.error('Logout failed on server:', error);
    }
    localStorage.removeItem('jwt_token');
    setUser(null);
    setIsSuperAdmin(false);
    setIsOnboarded(false);
    setUserTeamRoles({}); // Clear roles on logout
  }, []);

  useEffect(() => {
    setLogoutHandler(logout);
    const token = localStorage.getItem('jwt_token');
    const loadAuthData = async () => {
      setIsLoading(true);
      if (token) {
        try {
          const decoded = jwtDecode<DecodedToken>(token);
          setUser(decoded);
          setIsSuperAdmin(decoded.is_super_admin === 'true');
          setIsOnboarded(decoded.is_onboarded === 'true');
          await fetchUserTeamRoles(decoded.sub); // Wait for roles to be fetched
        } catch (error) {
          console.error('Invalid token during useEffect init:', error);
          localStorage.removeItem('jwt_token');
          setUser(null);
          setIsSuperAdmin(false);
          setIsOnboarded(false);
          setUserTeamRoles({});
        }
      }
      setIsLoading(false);
    };

    loadAuthData();
  }, [fetchUserTeamRoles, logout]);

  return (
    <AuthContext.Provider
      value={{ user, isSuperAdmin, isOnboarded, userTeamRoles, login, logout, isLoading }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
