import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
// Material UI Imports
import { Box, CircularProgress, Container, Typography } from '@mui/material';
import { useSnackbar } from 'notistack';

import { useAuth } from '@/context/AuthContext';

const AuthCallbackPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { login } = useAuth();
  const { enqueueSnackbar } = useSnackbar();

  useEffect(() => {
    const token = searchParams.get('token');

    if (token) {
      login(token);
      localStorage.removeItem('invitationToken'); // Clean up invitation token after successful login
      navigate('/my-teams');
    } else {
      enqueueSnackbar('Authentication callback error: No token received.', { variant: 'error' });
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
        <Typography variant="body1">Please wait, authenticating...</Typography>
      </Box>
    </Container>
  );
};

export default AuthCallbackPage;
