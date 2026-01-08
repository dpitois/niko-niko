import React, { useState } from 'react';
import {
  Container,
  Grid,
  Paper,
  Typography,
  List,
  ListItem,
  ListItemText,
  IconButton,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Button
} from '@mui/material';
import { Delete as DeleteIcon } from '@mui/icons-material';
import { useSnackbar } from 'notistack';
import useSprints from '../hooks/useSprints';
import useTeams from '../hooks/useTeams';
import AdminCreateSprintForm from '../components/AdminCreateSprintForm';
import { deleteSprint } from '../services/sprintService';
import type { Sprint } from '../models/Sprint';

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
    } catch (error) {
      enqueueSnackbar('Failed to delete sprint.', { variant: 'error' });
    } finally {
      handleCloseDeleteDialog();
    }
  };

  const getTeamName = (teamId: string): string => {
    return teams?.find(t => t.id === teamId)?.name ?? 'Unknown Team';
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4 }}>
      <Typography variant="h4" gutterBottom>
        Admin Sprints Management
      </Typography>
      <Grid container spacing={3}>
        {/* Create Sprint Form */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Create New Sprint
            </Typography>
            {isLoadingTeams || !teams ? (
              <CircularProgress />
            ) : (
              <AdminCreateSprintForm teams={teams} onSprintCreated={handleSprintCreated} />
            )}
          </Paper>
        </Grid>

        {/* Sprints List */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Existing Sprints
            </Typography>
            {isLoadingSprints ? (
              <CircularProgress />
            ) : (
              <List>
                {sprints && sprints.length > 0 ? (
                  sprints.map((sprint) => (
                    <ListItem
                      key={sprint.id}
                      secondaryAction={
                        <IconButton edge="end" aria-label="delete" onClick={() => handleDeleteClick(sprint)}>
                          <DeleteIcon />
                        </IconButton>
                      }
                    >
                      <ListItemText
                        primary={sprint.name}
                        secondary={`${new Date(sprint.startDate).toLocaleDateString()} - ${new Date(sprint.endDate).toLocaleDateString()} | Team: ${getTeamName(sprint.teamId)}`}
                      />
                    </ListItem>
                  ))
                ) : (
                  <Typography>No sprints found.</Typography>
                )}
              </List>
            )}
          </Paper>
        </Grid>
      </Grid>
      
      {/* Delete Confirmation Dialog */}
      <Dialog
        open={sprintToDelete !== null}
        onClose={handleCloseDeleteDialog}
      >
        <DialogTitle>Delete Sprint?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the sprint "<strong>{sprintToDelete?.name}</strong>"? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDeleteDialog}>Cancel</Button>
          <Button onClick={handleConfirmDelete} color="error">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default AdminSprintsPage;