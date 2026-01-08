import { useEffect } from 'react'; // Removed unused useCallback import
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useSnackbar } from 'notistack';

// Material UI Imports
import { Container, Box, Typography, CircularProgress } from '@mui/material';

const AuthCallbackPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { login } = useAuth();
  const { enqueueSnackbar } = useSnackbar();

  useEffect(() => {
    const token = searchParams.get('token');

    if (token) {
      login(token);
      navigate('/my-teams');
    } else {
      enqueueSnackbar("Authentication callback error: No token received.", { variant: 'error' });
      navigate('/login');
    }
  }, [navigate, searchParams, login, enqueueSnackbar]);

  return (
    <Container maxWidth="xs">
      <Box
        sx={{
          marginTop: 8,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: 2,
        }}
      >
        <CircularProgress />
        <Typography variant="body1">
          Please wait, authenticating...
        </Typography>
      </Box>
    </Container>
  );
};

export default AuthCallbackPage;
