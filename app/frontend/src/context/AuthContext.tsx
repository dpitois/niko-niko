import React, { createContext, useState, useEffect, useCallback, useContext } from 'react';

interface DecodedToken {
  sub: string; // Subject (user id)
  name: string;
  email: string;
  // ... any other fields in your token
}

interface AuthContextType {
  user: DecodedToken | null;
  login: (token: string) => void;
  logout: () => void;
  isLoading: boolean; // To indicate if initial auth check is ongoing
}

// Manual JWT decode for prototype purposes to bypass TS2307 error with jwt-decode library
const manualJwtDecode = <T extends object>(token: string): T | null => { // Explicitly return T | null
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(function (c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        })
        .join('')
    );

    return JSON.parse(jsonPayload) as T; // Cast to T
  } catch (e) {
    console.error("Error decoding JWT manually:", e);
    return null; // Return null on error
  }
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<DecodedToken | null>(null);
  const [isLoading, setIsLoading] = useState(true); // Start loading

  const login = useCallback((token: string) => {
    localStorage.setItem('jwt_token', token);
    const decoded = manualJwtDecode<DecodedToken>(token);
    if (decoded) {
      setUser(decoded);
    } else {
      setUser(null); // Clear user if token is invalid
      localStorage.removeItem('jwt_token');
    }
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('jwt_token');
    setUser(null);
  }, []);

  useEffect(() => {
    const token = localStorage.getItem('jwt_token');
    if (token) {
      const decoded = manualJwtDecode<DecodedToken>(token);
      if (decoded) {
        setUser(decoded);
      } else {
        localStorage.removeItem('jwt_token'); // Remove invalid token
      }
    }
    setIsLoading(false); // Finished initial loading
  }, []);

  return (
    <AuthContext.Provider value={{ user, login, logout, isLoading }}>
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
