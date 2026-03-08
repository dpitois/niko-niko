// React Imports
import { useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Navigate, Outlet, Route, Routes } from 'react-router-dom';
import CloseIcon from '@mui/icons-material/Close';
import { IconButton } from '@mui/material';
// Material UI Imports
import CssBaseline from '@mui/material/CssBaseline';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs from 'dayjs';
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
// New placeholder pages
import CurrentSprintsPage from '@/pages/CurrentSprintsPage';
import DashboardPage from '@/pages/DashboardPage';
import LoginPage from '@/pages/LoginPage';
import OnboardingPage from '@/pages/OnboardingPage';
import PastSprintsPage from '@/pages/PastSprintsPage';
import ProfilePage from '@/pages/ProfilePage';
import SprintDetailsPage from '@/pages/SprintDetailsPage';

import './App.css';

const ProtectedLayout = () => (
  <ProtectedRoute>
    <AppLayout>
      <Outlet />
    </AppLayout>
  </ProtectedRoute>
);

function App() {
  const notistackRef = useRef<SnackbarProvider>(null);
  const { i18n } = useTranslation();

  useEffect(() => {
    dayjs.locale(i18n.language);
  }, [i18n.language]);

  const onClickDismiss = (key: string | number) => () => {
    notistackRef.current?.closeSnackbar(key);
  };

  return (
    <ColorModeProvider>
      <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale={i18n.language}>
        <CssBaseline />
        <SnackbarProvider
          ref={notistackRef}
          maxSnack={5}
          preventDuplicate
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
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
            <Route
              path="/onboarding"
              element={
                <ProtectedRoute skipOnboardingCheck={true}>
                  <OnboardingPage />
                </ProtectedRoute>
              }
            />

            {/* Protected routes within the layout */}
            <Route element={<ProtectedLayout />}>
              <Route path="/" element={<Navigate to="/my-teams" />} />

              {/* User Dashboard */}
              <Route path="/my-teams" element={<DashboardPage />} />
              <Route path="/profile" element={<ProfilePage />} />
              <Route path="/teams/:teamId/sprints/:sprintId" element={<SprintDetailsPage />} />
              <Route path="/current-sprints" element={<CurrentSprintsPage />} />
              <Route path="/past-sprints" element={<PastSprintsPage />} />

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
                  <ProtectedRoute requiredAnyAdmin={true}>
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

            {/* Catch-all route: redirect unknown paths to home */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </SnackbarProvider>
      </LocalizationProvider>
    </ColorModeProvider>
  );
}

export default App;
