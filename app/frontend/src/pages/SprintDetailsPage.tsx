import React from 'react';
import { useTranslation } from 'react-i18next';
import { useParams } from 'react-router-dom';
import { Link as RouterLink } from 'react-router-dom';
import DashboardIcon from '@mui/icons-material/Dashboard';
import { Box, Breadcrumbs, CircularProgress, Link, Typography } from '@mui/material';

import { useSprint } from '@/hooks/useSprint';
import { useTeam } from '@/hooks/useTeam';

import PageContainer from '@/components/layout/PageContainer';
import SprintMoodGrid from '@/components/sprints/SprintMoodGrid';

const SprintDetailsPage: React.FC = () => {
  const { t } = useTranslation();
  const { teamId, sprintId } = useParams<{ teamId: string; sprintId: string }>();

  const { team, isLoading: isLoadingTeam, isError: isErrorTeam } = useTeam(teamId);
  const {
    sprint,
    isLoading: isLoadingSprint,
    isError: isErrorSprint,
  } = useSprint(sprintId ?? null);

  if (isLoadingTeam || isLoadingSprint) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isErrorTeam || isErrorSprint || !team || !sprint) {
    return (
      <PageContainer title={t('common.error')}>
        <Typography color="error">{t('dashboard.failedLoadTeams')}</Typography>
      </PageContainer>
    );
  }

  return (
    <PageContainer title={`${team.name} - ${sprint.name}`} icon={<DashboardIcon />}>
      <Breadcrumbs aria-label="breadcrumb" sx={{ mb: 3 }}>
        <Link component={RouterLink} underline="hover" color="inherit" to="/my-teams">
          {t('dashboard.title')}
        </Link>
        <Typography color="text.primary">{sprint.name}</Typography>
      </Breadcrumbs>

      <Box sx={{ mt: 2 }}>
        <Typography variant="h6" gutterBottom>
          {new Date(sprint.startDate).toLocaleDateString()} -{' '}
          {new Date(sprint.endDate).toLocaleDateString()}
        </Typography>
        <Box sx={{ overflowX: 'auto', pb: 2, mt: 3 }}>
          <SprintMoodGrid
            sprintId={sprint.id}
            sprintStartDate={new Date(sprint.startDate)}
            sprintEndDate={new Date(sprint.endDate)}
            teamMembers={team.members}
          />
        </Box>
      </Box>
    </PageContainer>
  );
};

export default SprintDetailsPage;
