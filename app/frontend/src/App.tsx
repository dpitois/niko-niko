import { Routes, Route, Navigate, Outlet } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import CreateSprintPage from './pages/CreateSprintPage';
import MoodEntryStandalonePage from './pages/MoodEntryStandalonePage';
import AcceptInvitationPage from './pages/AcceptInvitationPage';
import AuthCallbackPage from './pages/AuthCallbackPage';
import ProtectedRoute from './components/ProtectedRoute';
import AppLayout from './components/layout/AppLayout';

// New placeholder pages
import PastSprintsPage from './pages/PastSprintsPage';
import AdminTeamsPage from './pages/AdminTeamsPage';
import AdminUsersPage from './pages/AdminUsersPage';
import AdminSprintsPage from './pages/AdminSprintsPage';

import './App.css';

// Material UI Imports
import CssBaseline from '@mui/material/CssBaseline';

// Context Imports
import { ColorModeProvider } from './context/ColorModeContext';

// notistack imports
import { SnackbarProvider } from 'notistack';

const ProtectedLayout = () => (
  <AppLayout>
    <Outlet />
  </AppLayout>
);


function App() {
  return (
    <ColorModeProvider>
      <CssBaseline />
      <SnackbarProvider maxSnack={3}>
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
                  <PastSprintsPage /> {/* To be created */}
                </ProtectedRoute>
              }
            />

            {/* Admin Dashboard */}
            <Route
              path="/admin"
              element={
                <ProtectedRoute requiredSuperAdmin={true}>
                  <Navigate to="/admin/teams" />
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/teams"
              element={
                <ProtectedRoute requiredSuperAdmin={true}>
                  <AdminTeamsPage /> {/* To be created */}
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/users"
              element={
                <ProtectedRoute requiredSuperAdmin={true}>
                  <AdminUsersPage /> {/* To be created */}
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/sprints"
              element={
                <ProtectedRoute requiredSuperAdmin={true}>
                  <AdminSprintsPage /> {/* To be created */}
                </ProtectedRoute>
              }
            />
            
            <Route
              path="/sprint/create/:teamId?"
              element={
                <ProtectedRoute>
                  <CreateSprintPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/moodentry/:sprintId"
              element={
                <ProtectedRoute>
                  <MoodEntryStandalonePage />
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