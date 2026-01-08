import { Routes, Route, Navigate } from 'react-router-dom';
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
import TeamInvitationsPage from './pages/TeamInvitationsPage';
import AdminTeamsPage from './pages/AdminTeamsPage';
import AdminUsersPage from './pages/AdminUsersPage';
import AdminSprintsPage from './pages/AdminSprintsPage';

import './App.css';

// Material UI Imports
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';

// notistack imports
import { SnackbarProvider } from 'notistack'; // Import SnackbarProvider

const theme = createTheme({
  palette: {
    primary: {
      main: '#007bff',
    },
    secondary: {
      main: '#6c757d',
    },
  },
  typography: {
    fontFamily: 'Arial, Helvetica, sans-serif',
    fontSize: 14,
    h1: {
      fontSize: '2rem',
      fontWeight: 500,
      color: '#007bff',
      marginBottom: '1rem',
    },
    h2: {
      fontSize: '1.5rem',
      fontWeight: 500,
      color: '#0056b3',
      marginBottom: '1rem',
    },
    h3: {
      fontSize: '1.2rem',
      fontWeight: 500,
      color: '#004085',
      marginBottom: '0.8rem',
    },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          borderRadius: 4,
          padding: '0.8em 1.2em',
        },
      },
    },
    MuiTextField: {
      defaultProps: {
        variant: 'outlined',
        size: 'small',
      },
    },
    MuiSelect: {
      defaultProps: {
        variant: 'outlined',
        size: 'small',
      },
    },
    MuiInputLabel: {
      styleOverrides: {
        root: {
          fontWeight: 'bold',
        },
      },
    },
  },
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <SnackbarProvider maxSnack={3}>
        <AppLayout>
          <Routes>
            <Route path="/" element={<Navigate to="/my-teams" />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/auth/callback" element={<AuthCallbackPage />} />
            <Route path="/accept-invitation/:token" element={<AcceptInvitationPage />} />

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
            <Route
              path="/team-invitations"
              element={
                <ProtectedRoute>
                  <TeamInvitationsPage /> {/* To be created */}
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
          </Routes>
        </AppLayout>
      </SnackbarProvider>
    </ThemeProvider>
  );
}

export default App;
