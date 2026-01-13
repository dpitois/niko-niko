// React Imports
import { useRef } from 'react';
import { Navigate, Outlet, Route, Routes } from 'react-router-dom';
import CloseIcon from '@mui/icons-material/Close';
import { IconButton } from '@mui/material';
// Material UI Imports
import CssBaseline from '@mui/material/CssBaseline';
// notistack imports
import { SnackbarProvider } from 'notistack';

// Context Imports
import { ColorModeProvider } from '@/context/ColorModeContext';

import AppLayout from '@/components/layout/AppLayout';
import ProtectedRoute from '@/components/ProtectedRoute';
import AcceptInvitationPage from '@/pages/AcceptInvitationPage';
import AdminSprintsPage from '@/pages/AdminSprintsPage';
import AdminTeamsPage from '@/pages/AdminTeamsPage';
import AdminUsersPage from '@/pages/AdminUsersPage';
import AuthCallbackPage from '@/pages/AuthCallbackPage';
import DashboardPage from '@/pages/DashboardPage';
import LoginPage from '@/pages/LoginPage';
// New placeholder pages
import PastSprintsPage from '@/pages/PastSprintsPage';

import './App.css';

const ProtectedLayout = () => (
  <AppLayout>
    <Outlet />
  </AppLayout>
);

function App() {
  const notistackRef = useRef<SnackbarProvider>(null);

  const onClickDismiss = (key: string | number) => () => {
    notistackRef.current?.closeSnackbar(key);
  };

  return (
    <ColorModeProvider>
      <CssBaseline />
      <SnackbarProvider
        ref={notistackRef}
        maxSnack={5}
        preventDuplicate
        anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
        action={(key) => (
          <IconButton onClick={onClickDismiss(key)} color="inherit" size="small">
            <CloseIcon fontSize="small" />
          </IconButton>
        )}
      >
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/auth/callback" element={<AuthCallbackPage />} />
          <Route path="/accept-invitation/:token" element={<AcceptInvitationPage />} />

          {/* Protected routes within the layout */}
          <Route element={<ProtectedLayout />}>
            <Route path="/" element={<Navigate to="/my-teams" />} />

            {/* User Dashboard */}
            <Route
              path="/my-teams"
              element={
                <ProtectedRoute>
                  <DashboardPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/past-sprints"
              element={
                <ProtectedRoute>
                  <PastSprintsPage />
                </ProtectedRoute>
              }
            />

            {/* Admin Dashboard */}
            <Route
              path="/admin"
              element={
                <ProtectedRoute>
                  <Navigate to="/admin/teams" />
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/teams"
              element={
                <ProtectedRoute requiredSuperAdmin={true}>
                  <AdminTeamsPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/users"
              element={
                <ProtectedRoute>
                  <AdminUsersPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/sprints"
              element={
                <ProtectedRoute>
                  <AdminSprintsPage />
                </ProtectedRoute>
              }
            />
          </Route>
        </Routes>
      </SnackbarProvider>
    </ColorModeProvider>
  );
}

export default App;
