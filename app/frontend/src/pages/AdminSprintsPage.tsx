import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
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
import AdminEditSprintDialog from '@/components/AdminEditSprintDialog';
import PageContainer from '@/components/layout/PageContainer';

const AdminSprintsPage: React.FC = () => {
  const { t } = useTranslation();
  const { teams, isLoading: isLoadingTeams } = useTeams();
  const { sprints, isLoading: isLoadingSprints, mutate: mutateSprints } = useSprints();
  const { enqueueSnackbar } = useSnackbar();

  const [sprintToDelete, setSprintToDelete] = useState<Sprint | null>(null);
  const [sprintToEdit, setSprintToEdit] = useState<Sprint | null>(null);

  const handleSprintCreated = () => {
    enqueueSnackbar(t('adminSprints.createForm.success'), { variant: 'success' });
    mutateSprints();
  };

  const handleSprintUpdated = () => {
    enqueueSnackbar(t('adminSprints.editDialog.success'), { variant: 'success' });
    mutateSprints();
  };

  const handleDeleteClick = (sprint: Sprint) => {
    setSprintToDelete(sprint);
  };

  const handleEditClick = (sprint: Sprint) => {
    setSprintToEdit(sprint);
  };

  const handleCloseDeleteDialog = () => {
    setSprintToDelete(null);
  };

  const handleCloseEditDialog = () => {
    setSprintToEdit(null);
  };

  const handleConfirmDelete = async () => {
    if (!sprintToDelete) return;

    try {
      await deleteSprint(sprintToDelete.id);
      enqueueSnackbar(t('adminSprints.deleteDialog.success'), { variant: 'success' });
      mutateSprints();
    } catch {
      enqueueSnackbar(t('adminSprints.deleteDialog.fail'), { variant: 'error' });
    } finally {
      handleCloseDeleteDialog();
    }
  };

  const getTeamName = (teamId: string): string => {
    return teams?.find((t) => t.id === teamId)?.name ?? 'Unknown Team';
  };

  return (
    <PageContainer title={t('adminSprints.title')} icon={<TimelineIcon />}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h5" component="h2" gutterBottom>
          {t('adminSprints.createNew')}
        </Typography>
        {isLoadingTeams || !teams ? (
          <CircularProgress />
        ) : (
          <AdminCreateSprintForm teams={teams} onSprintCreated={handleSprintCreated} />
        )}
      </Box>

      <Divider sx={{ my: 4 }} />

      <Typography variant="h5" component="h2" gutterBottom>
        {t('adminSprints.manageAll')}
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

                  {/* Actions */}
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <IconButton
                      onClick={() => handleEditClick(sprint)}
                      color="primary"
                      size="small"
                      aria-label="edit"
                    >
                      <EditIcon />
                    </IconButton>
                    <IconButton
                      onClick={() => handleDeleteClick(sprint)}
                      color="error"
                      size="small"
                      aria-label="delete"
                    >
                      <DeleteIcon />
                    </IconButton>
                  </Box>
                </Box>
              </ListItem>
            ))
          ) : (
            <Typography variant="body1">{t('adminSprints.noSprints')}</Typography>
          )}
        </List>
      )}

      {/* Edit Dialog */}
      <AdminEditSprintDialog
        open={sprintToEdit !== null}
        sprint={sprintToEdit}
        onClose={handleCloseEditDialog}
        onSprintUpdated={handleSprintUpdated}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={sprintToDelete !== null} onClose={handleCloseDeleteDialog}>
        <DialogTitle>{t('adminSprints.deleteDialog.title')}</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {t('adminSprints.deleteDialog.content', { name: sprintToDelete?.name })}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDeleteDialog}>{t('common.cancel')}</Button>
          <Button onClick={handleConfirmDelete} color="error">
            {t('common.delete')}
          </Button>
        </DialogActions>
      </Dialog>
    </PageContainer>
  );
};

export default AdminSprintsPage;
