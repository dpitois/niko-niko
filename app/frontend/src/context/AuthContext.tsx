import React, { createContext, useState, useEffect, useCallback, useContext } from 'react';
import { jwtDecode } from 'jwt-decode';

interface DecodedToken {
  sub: string; // Subject (user id)
  name: string;
  email: string;
  is_super_admin?: string; // This claim might be optional
}

interface AuthContextType {
  user: DecodedToken | null;
  isSuperAdmin: boolean;
  login: (token: string) => void;
  logout: () => void;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<DecodedToken | null>(null);
  const [isSuperAdmin, setIsSuperAdmin] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  const processToken = useCallback((token: string) => {
    localStorage.setItem('jwt_token', token);
    try {
      const decoded = jwtDecode<DecodedToken>(token);
      setUser(decoded);
      setIsSuperAdmin(decoded.is_super_admin === 'true');
    } catch (error) {
      console.error("Invalid token:", error);
      localStorage.removeItem('jwt_token');
      setUser(null);
      setIsSuperAdmin(false);
    }
  }, []);

  const login = useCallback((token: string) => {
    processToken(token);
  }, [processToken]);

  const logout = useCallback(() => {
    localStorage.removeItem('jwt_token');
    setUser(null);
    setIsSuperAdmin(false);
  }, []);

  useEffect(() => {
    const token = localStorage.getItem('jwt_token');
    if (token) {
      processToken(token);
    }
    setIsLoading(false);
  }, [processToken]);

  return (
    <AuthContext.Provider value={{ user, isSuperAdmin, login, logout, isLoading }}>
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
