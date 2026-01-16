import React from 'react';
import { useTranslation } from 'react-i18next';
import { Link as RouterLink } from 'react-router-dom';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import DashboardIcon from '@mui/icons-material/Dashboard';
// Material UI Imports
import {
  Box,
  Button,
  CircularProgress,
  Grid,
  Typography,
} from '@mui/material';

import useSprints from '@/hooks/useSprints';
import useTeams from '@/hooks/useTeams';
import type { Sprint } from '@/models/Sprint';
import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';

import DailyMoodWidget from '@/components/dashboard/DailyMoodWidget';
import TeamMoodTrendWidget from '@/components/dashboard/TeamMoodTrendWidget';
import PageContainer from '@/components/layout/PageContainer';

const DashboardPage: React.FC = () => {
  const { t } = useTranslation();
  const { teams, isLoading: isLoadingTeams, isError: isErrorTeams } = useTeams();

  const sortedTeams = React.useMemo(() => {
    if (!teams) return [];
    const today = new Date();

    const getActiveSprint = (team: TeamWithMembersAndSprints) => {
      return team.sprints?.find((s) => {
        const start = new Date(s.startDate);
        const end = new Date(s.endDate);
        return today >= start && today <= end;
      });
    };

    return [...teams].sort((a, b) => {
      const aSprint = getActiveSprint(a);
      const bSprint = getActiveSprint(b);

      // 1. Active Sprint > No Active Sprint
      if (aSprint && !bSprint) return -1;
      if (!aSprint && bSprint) return 1;

      // 2. Team Name
      const nameCompare = a.name.localeCompare(b.name);
      if (nameCompare !== 0) return nameCompare;

      // 3. Sprint Name
      if (aSprint && bSprint) {
        return aSprint.name.localeCompare(bSprint.name);
      }

      return 0;
    });
  }, [teams]);

  if (isLoadingTeams) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isErrorTeams) {
    return <Typography color="error">{t('dashboard.failedLoadTeams')}</Typography>;
  }

  return (
    <PageContainer title={t('dashboard.title')} icon={<DashboardIcon />}>
      {sortedTeams && sortedTeams.length > 0 ? (
        sortedTeams.map((team: TeamWithMembersAndSprints) => (
          <TeamDashboardSection key={team.id} team={team} />
        ))
      ) : (
        <Typography variant="body1">{t('dashboard.noTeams')}</Typography>
      )}
    </PageContainer>
  );
};

interface TeamDashboardSectionProps {
  team: TeamWithMembersAndSprints;
}

const TeamDashboardSection: React.FC<TeamDashboardSectionProps> = ({ team }) => {
  const { t } = useTranslation();
  const { sprints, isLoading: isLoadingSprints, isError: isErrorSprints } = useSprints(team.id);

  if (isLoadingSprints) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isErrorSprints) {
    return <Typography color="error">{t('common.error')}</Typography>;
  }

  const currentSprint = sprints?.find((sprint: Sprint) => {
    const today = new Date();
    const startDate = new Date(sprint.startDate);
    const endDate = new Date(sprint.endDate);
    return today >= startDate && today <= endDate;
  });

  return (
    <Box sx={{ mb: 6 }}>
      {/* Unified Header */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'baseline',
          justifyContent: 'space-between',
          borderBottom: 1,
          borderColor: 'divider',
          pb: 1,
          mb: 3,
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 2, flexWrap: 'wrap' }}>
          <Typography variant="h5" fontWeight="bold">
            {team.name}
          </Typography>
          {currentSprint && (
            <Typography variant="subtitle1" color="text.secondary">
              {currentSprint.name}
              <Typography component="span" variant="caption" sx={{ ml: 1 }}>
                ({new Date(currentSprint.startDate).toLocaleDateString()} -{' '}
                {new Date(currentSprint.endDate).toLocaleDateString()})
              </Typography>
            </Typography>
          )}
        </Box>
        {currentSprint && (
          <Button
            component={RouterLink}
            to={`/teams/${team.id}/sprints/${currentSprint.id}`}
            endIcon={<ArrowForwardIcon />}
            size="small"
            color="inherit"
            sx={{ textTransform: 'none' }}
          >
            {t('dashboard.viewFullBoard', 'Voir le tableau complet')}
          </Button>
        )}
      </Box>

      {/* Widgets Content */}
      {currentSprint ? (
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 5 }}>
            <DailyMoodWidget
              sprintId={currentSprint.id}
              sprintStartDate={currentSprint.startDate}
              sprintEndDate={currentSprint.endDate}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 7 }}>
            <TeamMoodTrendWidget
              sprintId={currentSprint.id}
              sprintStartDate={currentSprint.startDate}
              sprintEndDate={currentSprint.endDate}
            />
          </Grid>
        </Grid>
      ) : (
        <Typography variant="body1" sx={{ py: 4, textAlign: 'center', color: 'text.secondary' }}>
          {t('dashboard.noActiveSprint')}
        </Typography>
      )}
    </Box>
  );
};

export default DashboardPage;