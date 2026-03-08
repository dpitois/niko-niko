import React from 'react';
import { useTranslation } from 'react-i18next';
import GroupWorkIcon from '@mui/icons-material/GroupWork';
import { Alert, Box, CircularProgress, Divider, Typography } from '@mui/material';
import { useSnackbar } from 'notistack';

import { useAuth } from '@/context/AuthContext';
import useTeams from '@/hooks/useTeams';
import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';
import { deleteTeam } from '@/services/teamService';

import AdminTeamListItem from '@/components/AdminTeamListItem';
import CreateTeamForm from '@/components/CreateTeamForm';
import PageContainer from '@/components/layout/PageContainer';

const AdminTeamsPage: React.FC = () => {
  const { t } = useTranslation();
  const { user, isSuperAdmin } = useAuth();
  const { teams, isLoading, isError, mutate } = useTeams();
  const { enqueueSnackbar } = useSnackbar();

  const handleTeamUpdated = () => {
    mutate();
  };

  const adminedTeamsCount = teams?.filter((t) => t.adminId === user?.sub).length || 0;
  const canCreateTeam = isSuperAdmin || adminedTeamsCount < 2;

  const handleDeleteTeam = async (teamId: string) => {
    try {
      await deleteTeam(teamId);
      mutate(); // Refresh the list of teams
    } catch {
      enqueueSnackbar(t('adminTeams.deleteFailed'), { variant: 'error' });
    }
  };

  return (
    <PageContainer title={t('adminTeams.title')} icon={<GroupWorkIcon />}>
      {canCreateTeam ? (
        <Box sx={{ mb: 4 }}>
          <Typography variant="h5" component="h2" gutterBottom>
            {t('adminTeams.createNew')}
          </Typography>
          <CreateTeamForm onTeamCreated={handleTeamUpdated} />
        </Box>
      ) : (
        <Alert severity="info" sx={{ mb: 4 }}>
          {t('adminTeams.quotaReached')}
        </Alert>
      )}

      <Divider sx={{ my: 4 }} />

      <Typography variant="h5" component="h2" gutterBottom>
        {t('adminTeams.manageAll')}
      </Typography>

      {isLoading && <CircularProgress />}
      {isError && <Typography color="error">{t('common.error')}</Typography>}

      {teams && teams.length > 0 ? (
        teams.map((team: TeamWithMembersAndSprints) => (
          <AdminTeamListItem
            key={team.id}
            team={team}
            onDelete={handleDeleteTeam}
            onUpdate={handleTeamUpdated}
          />
        ))
      ) : (
        <Typography variant="body1">{t('adminTeams.noTeamsFound')}</Typography>
      )}
    </PageContainer>
  );
};

export default AdminTeamsPage;
