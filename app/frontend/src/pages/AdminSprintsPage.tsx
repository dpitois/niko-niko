import React, { useState } from 'react';
import DeleteIcon from '@mui/icons-material/Delete';
import TimelineIcon from '@mui/icons-material/Timeline';
import {
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Divider,
  IconButton,
  List,
  ListItem,
  Typography,
} from '@mui/material';
import { useSnackbar } from 'notistack';

import useSprints from '@/hooks/useSprints';
import useTeams from '@/hooks/useTeams';
import type { Sprint } from '@/models/Sprint';
import { deleteSprint } from '@/services/sprintService';

import AdminCreateSprintForm from '@/components/AdminCreateSprintForm';
import PageContainer from '@/components/layout/PageContainer';

const AdminSprintsPage: React.FC = () => {
  const { teams, isLoading: isLoadingTeams } = useTeams();
  const { sprints, isLoading: isLoadingSprints, mutate: mutateSprints } = useSprints();
  const { enqueueSnackbar } = useSnackbar();

  const [sprintToDelete, setSprintToDelete] = useState<Sprint | null>(null);

  const handleSprintCreated = () => {
    enqueueSnackbar('Sprint created successfully!', { variant: 'success' });
    mutateSprints();
  };

  const handleDeleteClick = (sprint: Sprint) => {
    setSprintToDelete(sprint);
  };

  const handleCloseDeleteDialog = () => {
    setSprintToDelete(null);
  };

  const handleConfirmDelete = async () => {
    if (!sprintToDelete) return;

    try {
      await deleteSprint(sprintToDelete.id);
      enqueueSnackbar('Sprint deleted successfully!', { variant: 'success' });
      mutateSprints();
    } catch {
      enqueueSnackbar('Failed to delete sprint.', { variant: 'error' });
    } finally {
      handleCloseDeleteDialog();
    }
  };

  const getTeamName = (teamId: string): string => {
    return teams?.find((t) => t.id === teamId)?.name ?? 'Unknown Team';
  };

  return (
    <PageContainer title="Admin Sprints Management" icon={<TimelineIcon />}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h5" component="h2" gutterBottom>
          Create New Sprint
        </Typography>
        {isLoadingTeams || !teams ? (
          <CircularProgress />
        ) : (
          <AdminCreateSprintForm teams={teams} onSprintCreated={handleSprintCreated} />
        )}
      </Box>

      <Divider sx={{ my: 4 }} />

      <Typography variant="h5" component="h2" gutterBottom>
        Manage All Sprints
      </Typography>

      {isLoadingSprints ? (
        <CircularProgress />
      ) : (
        <List>
          {sprints && sprints.length > 0 ? (
            sprints.map((sprint) => (
              <ListItem key={sprint.id} divider sx={{ pr: 12 }}>
                {' '}
                {/* Add padding-right for absolute positioned button if needed, but here we use flex */}
                <Box sx={{ display: 'flex', width: '100%', alignItems: 'center', gap: 2 }}>
                  {/* Sprint Name */}
                  <Typography
                    variant="subtitle1"
                    component="span"
                    sx={{ fontWeight: 'bold', width: '25%', minWidth: '150px' }}
                    noWrap
                  >
                    {sprint.name}
                  </Typography>

                  {/* Team Name */}
                  <Chip label={getTeamName(sprint.teamId)} size="small" variant="outlined" />

                  {/* Dates (Flexible space) */}
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    sx={{ flexGrow: 1, textAlign: 'center', display: { xs: 'none', sm: 'block' } }}
                  >
                    {new Date(sprint.startDate).toLocaleDateString()} —{' '}
                    {new Date(sprint.endDate).toLocaleDateString()}
                  </Typography>

                  {/* Delete Button */}
                  <IconButton
                    onClick={() => handleDeleteClick(sprint)}
                    color="error"
                    size="small"
                    aria-label="delete"
                  >
                    <DeleteIcon />
                  </IconButton>
                </Box>
              </ListItem>
            ))
          ) : (
            <Typography variant="body1">No sprints found.</Typography>
          )}
        </List>
      )}

      {/* Delete Confirmation Dialog */}
      <Dialog open={sprintToDelete !== null} onClose={handleCloseDeleteDialog}>
        <DialogTitle>Delete Sprint?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the sprint "<strong>{sprintToDelete?.name}</strong>"?
            This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDeleteDialog}>Cancel</Button>
          <Button onClick={handleConfirmDelete} color="error">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </PageContainer>
  );
};

export default AdminSprintsPage;
