import React from 'react';
import FaceIcon from '@mui/icons-material/Face';
// Material UI Imports
import { Box, Card, CardContent, Chip,CircularProgress, Typography } from '@mui/material';

import useSprints from '@/hooks/useSprints';
import useTeams from '@/hooks/useTeams';
import type { Sprint } from '@/models/Sprint';
import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';

import SprintMoodGrid from '@/components/sprints/SprintMoodGrid';

const DashboardPage: React.FC = () => {
  const { teams, isLoading: isLoadingTeams, isError: isErrorTeams } = useTeams();

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
    <Box sx={{ mt: 2 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Home
      </Typography>

      {teams && teams.length > 0 ? (
        teams.map((team: TeamWithMembersAndSprints) => (
          <TeamDashboardSection key={team.id} team={team} />
        ))
      ) : (
        <Typography variant="body1">You don't belong to any teams yet.</Typography>
      )}
    </Box>
  );
};

interface TeamDashboardSectionProps {
  team: TeamWithMembersAndSprints;
}

const TeamDashboardSection: React.FC<TeamDashboardSectionProps> = ({ team }) => {
  const { sprints, isLoading: isLoadingSprints, isError: isErrorSprints } = useSprints(team.id);

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

  const currentSprint = sprints?.find((sprint: Sprint) => {
    const today = new Date();
    const startDate = new Date(sprint.startDate);
    const endDate = new Date(sprint.endDate);
    return today >= startDate && today <= endDate;
  });

  return (
    <Card sx={{ mb: 4, p: 2, boxShadow: 3 }}>
      <CardContent>
        <Box
          sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}
        >
          <Typography variant="h5" component="h2" gutterBottom>
            {team.name} - Current Sprint
          </Typography>
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
    </Card>
  );
};

export default DashboardPage;
