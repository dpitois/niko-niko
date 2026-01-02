import { Container, Box, Typography, Button } from '@mui/material';

const LoginPage = () => {
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
          href="/api/auth/login-github"
        >
          Sign in with GitHub
        </Button>
        {/* Google and Microsoft are currently disabled, but we keep the placeholders */}
        <Button
          fullWidth
          variant="contained"
          color="secondary"
          disabled // Disable Google button
          // href="/api/auth/login-google"
        >
          Sign in with Google
        </Button>
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
