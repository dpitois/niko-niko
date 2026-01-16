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
import { transferTeamAdmin, updateTeam } from '@/services/teamService';

import EditTeamDialog from '@/components/EditTeamDialog';
import PageContainer from '@/components/layout/PageContainer';
import SprintMoodGrid from '@/components/sprints/SprintMoodGrid';

const CurrentSprintsPage: React.FC = () => {
  const { t } = useTranslation();
  const { teams, isLoading: isLoadingTeams, isError: isErrorTeams, mutate } = useTeams();

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
    <PageContainer title={t('dashboard.currentSprint')} icon={<DashboardIcon />}>
      {sortedTeams && sortedTeams.length > 0 ? (
        sortedTeams.map((team: TeamWithMembersAndSprints) => (
          <TeamCurrentSprintSection key={team.id} team={team} onUpdate={() => mutate()} />
        ))
      ) : (
        <Typography variant="body1">{t('dashboard.noTeams')}</Typography>
      )}
    </PageContainer>
  );
};

interface TeamCurrentSprintSectionProps {
  team: TeamWithMembersAndSprints;
  onUpdate: () => void;
}

const TeamCurrentSprintSection: React.FC<TeamCurrentSprintSectionProps> = ({ team, onUpdate }) => {
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
              {team.name}
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
              {currentSprint.name} ({new Date(currentSprint.startDate).toLocaleDateString()} -{' '}
              {new Date(currentSprint.endDate).toLocaleDateString()})
            </Typography>
            <Box sx={{ overflowX: 'auto', pb: 2, mt: 2 }}>
              <SprintMoodGrid
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

export default CurrentSprintsPage;
