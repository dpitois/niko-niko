import React from 'react';
import { useTranslation } from 'react-i18next';
import HistoryIcon from '@mui/icons-material/History';
import {
  Alert,
  Box,
  Card,
  CardContent,
  CircularProgress,
  List,
  ListItem,
  ListItemText,
  Typography,
} from '@mui/material';

import useSprints from '@/hooks/useSprints';
import useTeams from '@/hooks/useTeams';
import type { Sprint } from '@/models/Sprint';

import PageContainer from '@/components/layout/PageContainer';

const PastSprintsPage: React.FC = () => {
  const { t } = useTranslation();
  const { sprints, isLoading: isLoadingSprints, isError: isErrorSprints } = useSprints();
  const { teams, isLoading: isLoadingTeams, isError: isErrorTeams } = useTeams();

  if (isLoadingSprints || isLoadingTeams) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isErrorSprints || isErrorTeams) {
    return <Alert severity="error">{t('pastSprints.failedLoad')}</Alert>;
  }

  if (!sprints || !teams) {
    return <Alert severity="info">{t('pastSprints.noData')}</Alert>;
  }

  const today = new Date();
  const pastSprints = sprints.filter((sprint: Sprint) => new Date(sprint.endDate) < today);

  // Helper to get team name
  const getTeamName = (teamId: string): string => {
    return teams.find((team) => team.id === teamId)?.name || 'Unknown Team';
  };

  return (
    <PageContainer title={t('pastSprints.title')} icon={<HistoryIcon />}>
      {pastSprints.length > 0 ? (
        <List>
          {pastSprints.map((sprint: Sprint) => (
            <Card key={sprint.id} variant="outlined" sx={{ mb: 2 }}>
              <CardContent>
                <ListItem disablePadding>
                  <ListItemText
                    primary={<Typography variant="h6">{sprint.name}</Typography>}
                    secondary={
                      <React.Fragment>
                        <Typography
                          sx={{ display: 'inline' }}
                          component="span"
                          variant="body2"
                          color="text.primary"
                        >
                          {t('pastSprints.team', { name: getTeamName(sprint.teamId) })}
                        </Typography>
                        {` — ${t('pastSprints.from', { start: new Date(sprint.startDate).toLocaleDateString(), end: new Date(sprint.endDate).toLocaleDateString() })}`}
                      </React.Fragment>
                    }
                  />
                </ListItem>
              </CardContent>
            </Card>
          ))}
        </List>
      ) : (
        <Typography variant="body1">{t('pastSprints.noSprintsFound')}</Typography>
      )}
    </PageContainer>
  );
};

export default PastSprintsPage;
