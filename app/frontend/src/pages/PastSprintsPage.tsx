import React, { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import HistoryIcon from '@mui/icons-material/History';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  CircularProgress,
  Typography,
} from '@mui/material';

import useTeams from '@/hooks/useTeams';
import type { Sprint } from '@/models/Sprint';
import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';

import PageContainer from '@/components/layout/PageContainer';
import PastSprintDetails from '@/components/sprints/PastSprintDetails';

const PastSprintsPage: React.FC = () => {
  const { t } = useTranslation();
  const { teams, isLoading: isLoadingTeams, isError: isErrorTeams } = useTeams();

  const sortedTeams = useMemo(() => {
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
      return a.name.localeCompare(b.name);
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
    return <Alert severity="error">{t('pastSprints.failedLoad')}</Alert>;
  }

  if (!teams || teams.length === 0) {
    return <Alert severity="info">{t('pastSprints.noData')}</Alert>;
  }

  const today = new Date();

  return (
    <PageContainer title={t('pastSprints.title')} icon={<HistoryIcon />}>
      {sortedTeams.map((team) => {
        const pastSprints = team.sprints
          .filter((sprint) => new Date(sprint.endDate) < today)
          .sort((a, b) => a.name.localeCompare(b.name));

        if (pastSprints.length === 0) return null;

        return (
          <Box key={team.id} sx={{ mb: 6 }}>
            <Typography variant="h5" fontWeight="bold" gutterBottom sx={{ borderBottom: 1, borderColor: 'divider', pb: 1, mb: 3 }}>
              {team.name}
            </Typography>
            
            {pastSprints.map((sprint: Sprint) => (
              <Accordion key={sprint.id} TransitionProps={{ unmountOnExit: true }} sx={{ mb: 1 }}>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Box sx={{ display: 'flex', flexDirection: 'column' }}>
                    <Typography variant="subtitle1" fontWeight="bold">
                      {sprint.name}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {new Date(sprint.startDate).toLocaleDateString()} - {new Date(sprint.endDate).toLocaleDateString()}
                    </Typography>
                  </Box>
                </AccordionSummary>
                <AccordionDetails>
                  <PastSprintDetails
                    sprintId={sprint.id}
                    sprintStartDate={sprint.startDate}
                    sprintEndDate={sprint.endDate}
                    teamMembers={team.members}
                  />
                </AccordionDetails>
              </Accordion>
            ))}
          </Box>
        );
      })}
      
      {sortedTeams.every(t => t.sprints.filter(s => new Date(s.endDate) < today).length === 0) && (
        <Typography variant="body1">{t('pastSprints.noSprintsFound')}</Typography>
      )}
    </PageContainer>
  );
};

export default PastSprintsPage;