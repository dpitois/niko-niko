import { useState } from 'react';
import { Container, Box, Typography, Button } from '@mui/material';

const LoginPage = () => {
  const [githubLoginHref] = useState(() => {
    const invitationToken = localStorage.getItem('invitationToken');
    return invitationToken ? `/api/auth/login-github?invitationToken=${invitationToken}` : '/api/auth/login-github';
  });
  const [googleLoginHref] = useState(() => {
    const invitationToken = localStorage.getItem('invitationToken');
    return invitationToken ? `/api/auth/login-google?invitationToken=${invitationToken}` : '/api/auth/login-google';
  });

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
        <Typography component="h1" variant="h5">
          Niko Niko Calendar
        </Typography>
        <Typography variant="body1">
          Please sign in to continue
        </Typography>
        <Button
          fullWidth
          variant="contained"
          color="primary"
          href={githubLoginHref}
        >
          Sign in with GitHub
        </Button>
        <Button
          fullWidth
          variant="contained"
          color="error"
          href={googleLoginHref}
        >
          Sign in with Google
        </Button>
        {/* Microsoft is currently disabled, but we keep the placeholders */}
        <Button
          fullWidth
          variant="contained"
          color="secondary"
          disabled // Disable Microsoft button
          // href="/api/auth/login-microsoft"
        >
          Sign in with Microsoft
        </Button>
      </Box>
    </Container>
  );
};

export default LoginPage;
