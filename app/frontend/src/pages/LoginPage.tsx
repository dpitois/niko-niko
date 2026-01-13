import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import GitHubIcon from '@mui/icons-material/GitHub';
import GoogleIcon from '@mui/icons-material/Google';
import { Box, Button, Container, Typography } from '@mui/material';

const LoginPage = () => {
  const { t } = useTranslation();
  const [githubLoginHref] = useState(() => {
    const invitationToken = localStorage.getItem('invitationToken');
    return invitationToken
      ? `/api/auth/login-github?invitationToken=${invitationToken}`
      : '/api/auth/login-github';
  });
  const [googleLoginHref] = useState(() => {
    const invitationToken = localStorage.getItem('invitationToken');
    return invitationToken
      ? `/api/auth/login-google?invitationToken=${invitationToken}`
      : '/api/auth/login-google';
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
          {t('common.appName')}
        </Typography>
        <Typography variant="body1">{t('login.subtitle')}</Typography>
        <Button
          fullWidth
          variant="contained"
          color="primary"
          href={githubLoginHref}
          startIcon={<GitHubIcon />}
        >
          {t('login.signInGithub')}
        </Button>
        <Button
          fullWidth
          variant="contained"
          color="error"
          href={googleLoginHref}
          startIcon={<GoogleIcon />}
        >
          {t('login.signInGoogle')}
        </Button>
        {/* Microsoft is currently disabled, but we keep the placeholders */}
        <Button
          fullWidth
          variant="contained"
          color="secondary"
          disabled // Disable Microsoft button
          // href="/api/auth/login-microsoft"
        >
          {t('login.signInMicrosoft')}
        </Button>
      </Box>
    </Container>
  );
};

export default LoginPage;
