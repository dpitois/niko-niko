import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useSearchParams } from 'react-router-dom';
import GitHubIcon from '@mui/icons-material/GitHub';
import GoogleIcon from '@mui/icons-material/Google';
import { Alert, Box, Button, Container, Typography } from '@mui/material';

import DiscordIcon from '@/components/icons/DiscordIcon';

const LoginPage = () => {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const error = searchParams.get('error');

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
  const [discordLoginHref] = useState(() => {
    const invitationToken = localStorage.getItem('invitationToken');
    return invitationToken
      ? `/api/auth/login-discord?invitationToken=${invitationToken}`
      : '/api/auth/login-discord';
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

        {error && (
          <Alert severity="error" sx={{ width: '100%', mb: 2 }}>
            {t('login.authenticationFailed')}
          </Alert>
        )}

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
        <Button
          fullWidth
          variant="contained"
          sx={{
            backgroundColor: '#5865F2',
            '&:hover': {
              backgroundColor: '#4752C4',
            },
          }}
          href={discordLoginHref}
          startIcon={<DiscordIcon />}
        >
          {t('login.signInDiscord')}
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