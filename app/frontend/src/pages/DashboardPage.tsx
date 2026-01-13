import React, { useState } from 'react';
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
import { updateTeam } from '@/services/teamService';

import EditTeamDialog from '@/components/EditTeamDialog';
import PageContainer from '@/components/layout/PageContainer';
import SprintMoodGrid from '@/components/sprints/SprintMoodGrid';

const DashboardPage: React.FC = () => {
  const { teams, isLoading: isLoadingTeams, isError: isErrorTeams, mutate } = useTeams();

  if (isLoadingTeams) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isErrorTeams) {
    return (
      <Typography color="error">Failed to load teams. Make sure you are logged in.</Typography>
    );
  }

  return (
    <PageContainer title="Home" icon={<DashboardIcon />}>
      {teams && teams.length > 0 ? (
        teams.map((team: TeamWithMembersAndSprints) => (
          <TeamDashboardSection key={team.id} team={team} onUpdate={() => mutate()} />
        ))
      ) : (
        <Typography variant="body1">You don't belong to any teams yet.</Typography>
      )}
    </PageContainer>
  );
};

interface TeamDashboardSectionProps {
  team: TeamWithMembersAndSprints;
  onUpdate: () => void;
}

const TeamDashboardSection: React.FC<TeamDashboardSectionProps> = ({ team, onUpdate }) => {
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
    return <Typography color="error">Failed to load sprints for {team.name}.</Typography>;
  }

  const handleUpdateName = async (newName: string) => {
    try {
      await updateTeam(team.id, newName);
      enqueueSnackbar('Team name updated successfully.', { variant: 'success' });
      onUpdate();
    } catch {
      enqueueSnackbar('Failed to update team name.', { variant: 'error' });
      throw new Error('Update failed');
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
              {team.name} - Current Sprint
            </Typography>
            {isTeamAdmin && (
              <IconButton
                size="small"
                onClick={() => setIsEditDialogOpen(true)}
                title="Edit team name"
                sx={{ color: 'text.secondary' }}
              >
                <EditIcon fontSize="small" />
              </IconButton>
            )}
          </Box>
          <Chip
            icon={<FaceIcon />}
            label={`Owner: ${team.adminName}`}
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
          <Typography variant="body2">No active sprint found for this team.</Typography>
        )}
      </CardContent>

      <EditTeamDialog
        open={isEditDialogOpen}
        onClose={() => setIsEditDialogOpen(false)}
        onUpdate={handleUpdateName}
        currentName={team.name}
      />
    </Card>
  );
};

export default DashboardPage;
