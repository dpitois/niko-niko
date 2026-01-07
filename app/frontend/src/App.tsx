import { Routes, Route, Navigate, useLocation } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import AdminDashboardPage from './pages/AdminDashboardPage';
import CreateSprintPage from './pages/CreateSprintPage';
import MoodEntryStandalonePage from './pages/MoodEntryStandalonePage';
import AcceptInvitationPage from './pages/AcceptInvitationPage'; // Import AcceptInvitationPage
import AuthCallbackPage from './pages/AuthCallbackPage';
import Header from './components/Header';
import ProtectedRoute from './components/ProtectedRoute';
import NotificationListener from './components/NotificationListener'; // Import NotificationListener
import './App.css';

// Material UI Imports
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { Box } from '@mui/material';

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
  const location = useLocation();
  const noHeaderPaths = ['/login']; // Paths where the header should not be displayed
  const shouldShowHeader = !noHeaderPaths.includes(location.pathname);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <SnackbarProvider maxSnack={3}> {/* Wrap with SnackbarProvider */}
        <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
          <NotificationListener /> {/* Render NotificationListener */}
          {shouldShowHeader && <Header />}
          <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
            <Routes>
              <Route path="/" element={<Navigate to="/my-teams" />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/auth/callback" element={<AuthCallbackPage />} />
              <Route path="/accept-invitation/:token" element={<AcceptInvitationPage />} />

              <Route
                path="/my-teams" // Nouvelle route pour les équipes de l'utilisateur
                element={
                  <ProtectedRoute>
                    <DashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/dashboard" // Conserver l'ancienne route /dashboard pour compatibilité ou supprimer si inutile.
                element={
                  <ProtectedRoute>
                    <DashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin"
                element={
                  <ProtectedRoute requiredSuperAdmin={true}>
                    <AdminDashboardPage />
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
          </Box>
        </Box>
      </SnackbarProvider>
    </ThemeProvider>
  );
}

export default App;
