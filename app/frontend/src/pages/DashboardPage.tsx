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
import { useQuery } from '@apollo/client';
import { useSnackbar } from 'notistack';

import { useAuth } from '@/context/AuthContext';
import { GET_MY_TEAMS_DASHBOARD } from '@/graphql/queries';
import type { MoodType } from '@/models/MoodType';
import type { User } from '@/models/User';
import { updateTeam } from '@/services/teamService';

import EditTeamDialog from '@/components/EditTeamDialog';
import PageContainer from '@/components/layout/PageContainer';
import SprintMoodGrid from '@/components/sprints/SprintMoodGrid';

// Define GraphQL Result Types locally for now, or move to models
interface GraphQLMoodEntry {
  id: string;
  mood: MoodType | string; // Can be string from GraphQL or MoodType after normalization
  date: string;
  userId: string;
}

interface GraphQLSprint {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  moodEntries: GraphQLMoodEntry[];
}

interface GraphQLTeam {
  id: string;
  name: string;
  adminId: string;
  admin: {
    id: string;
    name: string;
  };
  members: User[];
  sprints: GraphQLSprint[];
}

const DashboardPage: React.FC = () => {
  const { t } = useTranslation();
  
  const { data, loading, error, refetch } = useQuery<{ myTeams: GraphQLTeam[] }>(GET_MY_TEAMS_DASHBOARD);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Typography color="error">{t('dashboard.failedLoadTeams')}</Typography>;
  }

  const teams = data?.myTeams;

  return (
    <PageContainer title={t('dashboard.title')} icon={<DashboardIcon />}>
      {teams && teams.length > 0 ? (
        teams.map((team) => (
          <TeamDashboardSection key={team.id} team={team} onUpdate={() => refetch()} />
        ))
      ) : (
        <Typography variant="body1">{t('dashboard.noTeams')}</Typography>
      )}
    </PageContainer>
  );
};

interface TeamDashboardSectionProps {
  team: GraphQLTeam;
  onUpdate: () => void;
}

const TeamDashboardSection: React.FC<TeamDashboardSectionProps> = ({ team, onUpdate }) => {
  const { t } = useTranslation();
  const { isSuperAdmin, userTeamRoles } = useAuth();
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const { enqueueSnackbar } = useSnackbar();

  const isTeamAdmin = isSuperAdmin || userTeamRoles[team.id]?.isAdmin;

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

  const sprints = team.sprints;
  
  const currentSprint = sprints?.find((sprint) => {
    const today = new Date();
    // Normalize today to avoid time issues
    today.setHours(0, 0, 0, 0);
    
    const startDate = new Date(sprint.startDate);
    const endDate = new Date(sprint.endDate);
    
    // Normalize sprint dates if needed, usually they come as midnight UTC
    startDate.setHours(0, 0, 0, 0);
    endDate.setHours(23, 59, 59, 999);

    return today >= startDate && today <= endDate;
  });

  // Map GraphQL Sprint to Model Sprint if necessary, or ensure SprintMoodGrid accepts the structure.
  // SprintMoodGrid expects: teamId, sprintId, sprintStartDate, sprintEndDate, teamMembers.
  // It fetches mood entries internally? Let's check SprintMoodGrid.
  
  // Note: If SprintMoodGrid fetches its own data using REST (useMoodEntries), we are only halfway there.
  // But passing 'currentSprint' dates is correct.
  // Ideally SprintMoodGrid should also accept 'initialMoodEntries' or similar, but for now we keep it hybrid 
  // or check if we need to refactor it too. 
  // Wait, the GraphQL query fetches moodEntries! 
  // Let's see if we can pass them to SprintMoodGrid. 

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
            label={t('dashboard.owner', { name: team.admin.name })}
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
                initialMoods={currentSprint.moodEntries}
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
      />
    </Card>
  );
};

export default DashboardPage;
