import React, { useEffect } from 'react';
import CreateSprintForm from '../components/CreateSprintForm';
import { useParams, useNavigate } from 'react-router-dom';
import useSprints from '../hooks/useSprints';
import useTeams from '../hooks/useTeams';
import type { TeamWithMembersAndSprints } from '../models/Team/TeamWithMembersAndSprints';

// Material UI Imports
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import type { SelectChangeEvent } from '@mui/material/Select';
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import Stack from '@mui/material/Stack';

const CreateSprintPage: React.FC = () => {
  const { teamId: urlTeamId } = useParams<{ teamId: string }>();
  const navigate = useNavigate();
  const { teams, isLoading: teamsLoading, isError: teamsError } = useTeams();
  const selectedTeam = urlTeamId || '';

  // Effect to redirect to first team if no team in URL
  useEffect(() => {
    if (!urlTeamId && teams && teams.length > 0) {
      navigate(`/sprint/create/${teams[0].id}`, { replace: true });
    }
  }, [urlTeamId, teams, navigate]);

  const { mutate } = useSprints(selectedTeam || undefined);

  const handleSprintCreated = () => {
    mutate();
    navigate(`/my-teams`); // Navigate back to my-teams after sprint creation
  };

  const handleTeamSelectChange = (event: SelectChangeEvent<string>) => {
    const newTeamId = event.target.value;
    // Update URL if team selection changes from dropdown
    if (newTeamId) {
      navigate(`/sprint/create/${newTeamId}`);
    } else {
      navigate(`/sprint/create`);
    }
  };

  if (teamsLoading) return <Typography>Loading teams...</Typography>;
  if (teamsError) return <Typography color="error">Error loading teams.</Typography>;
  if (!teams) return <Typography>No teams found.</Typography>;

  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Create Sprint
      </Typography>

      <Stack spacing={3}>
        {teams.length === 0 ? (
          <Typography variant="body1" color="textSecondary">
            No teams available. Please create a team first from the Admin Dashboard.
          </Typography>
        ) : (
          <Box>
            <FormControl fullWidth>
              <InputLabel id="team-select-label">Select a Team</InputLabel>
              <Select
                labelId="team-select-label"
                value={selectedTeam}
                label="Select a Team"
                onChange={handleTeamSelectChange}
                size="small"
              >
                <MenuItem value="">
                  <em>-- Select a Team --</em>
                </MenuItem>
                {teams.map((team: TeamWithMembersAndSprints) => (
                  <MenuItem key={team.id} value={team.id}>{team.name}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>
        )}

        {selectedTeam && (
          <CreateSprintForm teamId={selectedTeam} onSprintCreated={handleSprintCreated} />
        )}
      </Stack>
    </Container>
  );
};

export default CreateSprintPage;