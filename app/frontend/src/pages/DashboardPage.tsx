import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import DashboardIcon from '@mui/icons-material/Dashboard';
import EditIcon from '@mui/icons-material/Edit';
import FaceIcon from '@mui/icons-material/Face';
// Material UI Imports
import {
  Box,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  IconButton,
  Typography,
} from '@mui/material';
import { useSnackbar } from 'notistack';

import { useAuth } from '@/context/AuthContext';
import useSprints from '@/hooks/useSprints';
import useTeams from '@/hooks/useTeams';
import type { Sprint } from '@/models/Sprint';
import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';
import { transferTeamAdmin,updateTeam } from '@/services/teamService';

import EditTeamDialog from '@/components/EditTeamDialog';
import PageContainer from '@/components/layout/PageContainer';
import SprintMoodGrid from '@/components/sprints/SprintMoodGrid';

const DashboardPage: React.FC = () => {
  const { t } = useTranslation();
  const { teams, isLoading: isLoadingTeams, isError: isErrorTeams, mutate } = useTeams();

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
      {teams && teams.length > 0 ? (
        teams.map((team: TeamWithMembersAndSprints) => (
          <TeamDashboardSection key={team.id} team={team} onUpdate={() => mutate()} />
        ))
      ) : (
        <Typography variant="body1">{t('dashboard.noTeams')}</Typography>
      )}
    </PageContainer>
  );
};

interface TeamDashboardSectionProps {
  team: TeamWithMembersAndSprints;
  onUpdate: () => void;
}

const TeamDashboardSection: React.FC<TeamDashboardSectionProps> = ({ team, onUpdate }) => {
  const { t } = useTranslation();
  const { isSuperAdmin, userTeamRoles } = useAuth();
  const { sprints, isLoading: isLoadingSprints, isError: isErrorSprints } = useSprints(team.id);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const { enqueueSnackbar } = useSnackbar();

  const isTeamAdmin = isSuperAdmin || userTeamRoles[team.id]?.isAdmin;

  if (isLoadingSprints) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isErrorSprints) {
    // Note: You might want to add a specific error key for sprints or reuse a generic one with dynamic content if supported/needed.
    // For now keeping it simple or reusing a generic error if applicable, or leaving untranslated if specific key missing.
    // Let's assume we want to translate it roughly or stick to English if key missing.
    // I will use a generic error message for now to be safe or leave hardcoded if critical.
    // Given the plan, let's use a generic error or add a specific key if I can. I added 'error' in common.
    return <Typography color="error">{t('common.error')}</Typography>;
  }

  const handleUpdateName = async (newName: string) => {
    try {
      await updateTeam(team.id, newName);
      enqueueSnackbar(t('dashboard.teamUpdated'), { variant: 'success' });
      onUpdate();
    } catch {
      enqueueSnackbar(t('dashboard.teamUpdateFailed'), { variant: 'error' });
      throw new Error('Update failed');
    }
  };

  const handleTransferAdmin = async (newAdminId: string) => {
    try {
      await transferTeamAdmin(team.id, newAdminId);
      enqueueSnackbar(t('adminTeams.teamAdminTransferred'), { variant: 'success' });
      onUpdate();
    } catch {
      enqueueSnackbar(t('adminTeams.teamAdminTransferFailed'), { variant: 'error' });
      throw new Error('Transfer failed');
    }
  };

  const currentSprint = sprints?.find((sprint: Sprint) => {
    const today = new Date();
    const startDate = new Date(sprint.startDate);
    const endDate = new Date(sprint.endDate);
    return today >= startDate && today <= endDate;
  });

  return (
    <Card key={team.id} sx={{ mb: 4, p: 2, boxShadow: 3 }}>
      <CardContent>
        <Box
          sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography variant="h5" component="h2" gutterBottom sx={{ mb: 0 }}>
              {team.name} - {t('dashboard.currentSprint')}
            </Typography>
            {isTeamAdmin && (
              <IconButton
                size="small"
                onClick={() => setIsEditDialogOpen(true)}
                title={t('dashboard.editTeamName')}
                sx={{ color: 'text.secondary' }}
              >
                <EditIcon fontSize="small" />
              </IconButton>
            )}
          </Box>
          <Chip
            icon={<FaceIcon />}
            label={t('dashboard.owner', { name: team.adminName })}
            variant="outlined"
            size="small"
            color="primary"
          />
        </Box>
        {currentSprint ? (
          <Box>
            <Typography variant="subtitle1" color="text.secondary">
              ({new Date(currentSprint.startDate).toLocaleDateString()} -{' '}
              {new Date(currentSprint.endDate).toLocaleDateString()})
            </Typography>
            <Box sx={{ overflowX: 'auto', pb: 2 }}>
              <SprintMoodGrid
                teamId={team.id}
                sprintId={currentSprint.id}
                sprintStartDate={new Date(currentSprint.startDate)}
                sprintEndDate={new Date(currentSprint.endDate)}
                teamMembers={team.members}
              />
            </Box>
          </Box>
        ) : (
          <Typography variant="body2">{t('dashboard.noActiveSprint')}</Typography>
        )}
      </CardContent>

      <EditTeamDialog
        open={isEditDialogOpen}
        onClose={() => setIsEditDialogOpen(false)}
        onUpdate={handleUpdateName}
        currentName={team.name}
        members={team.members}
        currentAdminId={team.adminId}
        onTransfer={handleTransferAdmin}
      />
    </Card>
  );
};

export default DashboardPage;
