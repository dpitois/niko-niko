import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Container,
  FormControlLabel,
  Grid,
  Typography,
} from '@mui/material';
import { useSnackbar } from 'notistack';

import { useAuth } from '@/context/AuthContext';
import api from '@/services/api';

const OnboardingPage = () => {
  const { t } = useTranslation();
  const { login } = useAuth();
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();
  const [agreed, setAgreed] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleConsent = async () => {
    if (!agreed) return;

    setIsSubmitting(true);
    try {
      const response = await api.post('/users/consent', {
        consentVersion: '1.0', // Version hardcoded for now, could be config
      });
      const { token } = response.data;
      login(token); // Update context with new token (isOnboarded: true)
      enqueueSnackbar(t('onboarding.success'), { variant: 'success' });
      navigate('/my-teams');
    } catch (error) {
      console.error('Consent failed:', error);
      enqueueSnackbar(t('onboarding.error'), { variant: 'error' });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Container maxWidth="sm" sx={{ mt: 8 }}>
      <Card elevation={3}>
        <CardContent sx={{ p: 4 }}>
          <Box sx={{ mb: 4, textAlign: 'center' }}>
            <Typography variant="h4" component="h1" gutterBottom>
              {t('onboarding.title')}
            </Typography>
            <Typography variant="body1" color="text.secondary">
              {t('onboarding.description')}
            </Typography>
          </Box>

          <Box sx={{ mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              {t('onboarding.termsTitle')}
            </Typography>
            <Typography variant="body2" paragraph id="terms-content">
              {t('onboarding.termsText')}
            </Typography>
            {/* TODO: Add real links to Terms and Privacy Policy when available
            <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
              <Link href="#terms-content" underline="hover">
                {t('onboarding.termsLink')}
              </Link>
              <Link href="#terms-content" underline="hover">
                {t('onboarding.privacyLink')}
              </Link>
            </Box>
            */}
          </Box>

          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={agreed}
                    onChange={(e) => setAgreed(e.target.checked)}
                    color="primary"
                  />
                }
                label={t('onboarding.agreeLabel')}
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Button
                variant="contained"
                color="primary"
                fullWidth
                size="large"
                disabled={!agreed || isSubmitting}
                onClick={handleConsent}
              >
                {t('onboarding.submitButton')}
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
    </Container>
  );
};

export default OnboardingPage;
