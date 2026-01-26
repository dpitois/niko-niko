import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material';

import type { User } from '@/models/User';

interface EditTeamDialogProps {
  open: boolean;
  onClose: () => void;
  onUpdate: (
    newName: string,
    newDefaultSprintDuration?: number,
    newSprintNameTemplate?: string,
  ) => Promise<void>;
  currentName: string;
  currentDefaultSprintDuration?: number;
  currentSprintNameTemplate?: string;
  members?: User[];
  currentAdminId?: string;
  onTransfer?: (newAdminId: string) => Promise<void>;
}

const EditTeamDialog: React.FC<EditTeamDialogProps> = ({
  open,
  onClose,
  onUpdate,
  currentName,
  currentDefaultSprintDuration,
  currentSprintNameTemplate,
  members,
  currentAdminId,
  onTransfer,
}) => {
  const { t } = useTranslation();
  const [name, setName] = useState(currentName);
  const [defaultSprintDuration, setDefaultSprintDuration] = useState<string>(
    currentDefaultSprintDuration?.toString() || '',
  );
  const [sprintNameTemplate, setSprintNameTemplate] = useState(currentSprintNameTemplate || '');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [selectedAdminId, setSelectedAdminId] = useState<string>('');
  const [transferError, setTransferError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setName(currentName);
      setDefaultSprintDuration(currentDefaultSprintDuration?.toString() || '');
      setSprintNameTemplate(currentSprintNameTemplate || '');
    }
  }, [open, currentName, currentDefaultSprintDuration, currentSprintNameTemplate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const duration = defaultSprintDuration ? parseInt(defaultSprintDuration, 10) : undefined;
    const nameChanged = name.trim() !== currentName;
    const durationChanged = duration !== currentDefaultSprintDuration;
    const templateChanged = sprintNameTemplate !== currentSprintNameTemplate;

    if (!name.trim() || (!nameChanged && !durationChanged && !templateChanged)) {
      onClose();
      return;
    }

    setIsSubmitting(true);
    try {
      await onUpdate(name, duration, sprintNameTemplate);
      onClose();
    } catch (error) {
      console.error('Failed to update team name:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleTransfer = async () => {
    if (!onTransfer || !selectedAdminId) return;

    setIsSubmitting(true);
    setTransferError(null);
    try {
      await onTransfer(selectedAdminId);
      onClose();
    } catch (error) {
      console.error('Failed to transfer team admin:', error);
      setTransferError(t('adminTeams.editDialog.transferError'));
      setIsSubmitting(false);
    }
  };

  const availableMembers = members?.filter((m) => m.id !== currentAdminId) || [];

  const durationInt = defaultSprintDuration ? parseInt(defaultSprintDuration, 10) : undefined;
  const hasChanges =
    name.trim() !== currentName ||
    durationInt !== currentDefaultSprintDuration ||
    sprintNameTemplate !== currentSprintNameTemplate;

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>{t('adminTeams.editDialog.title')}</DialogTitle>
      <form onSubmit={handleSubmit}>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label={t('adminTeams.editDialog.labelName')}
            type="text"
            fullWidth
            variant="outlined"
            value={name}
            onChange={(e) => setName(e.target.value)}
            disabled={isSubmitting}
            required
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense"
            label={t('adminTeams.editDialog.labelDefaultSprintDuration')}
            type="number"
            fullWidth
            variant="outlined"
            value={defaultSprintDuration}
            onChange={(e) => setDefaultSprintDuration(e.target.value)}
            disabled={isSubmitting}
            slotProps={{ htmlInput: { min: 1 } }}
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense"
            label={t('adminTeams.editDialog.labelSprintNameTemplate')}
            type="text"
            fullWidth
            variant="outlined"
            value={sprintNameTemplate}
            onChange={(e) => setSprintNameTemplate(e.target.value)}
            disabled={isSubmitting}
            helperText={t('adminTeams.editDialog.sprintNameTemplateHelper')}
            sx={{ mb: 2 }}
          />

          {onTransfer && members && currentAdminId && (
            <Box sx={{ mt: 4 }}>
              <Divider sx={{ mb: 2 }} />
              <Typography variant="h6" color="error" gutterBottom>
                {t('adminTeams.editDialog.dangerZone')}
              </Typography>
              <Typography variant="body2" gutterBottom>
                {t('adminTeams.editDialog.transferWarning')}
              </Typography>

              {transferError && (
                <Alert severity="error" sx={{ mb: 2 }}>
                  {transferError}
                </Alert>
              )}

              {availableMembers.length > 0 ? (
                <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mt: 2 }}>
                  <FormControl fullWidth size="small">
                    <InputLabel>{t('adminTeams.editDialog.selectNewAdmin')}</InputLabel>
                    <Select
                      value={selectedAdminId}
                      label={t('adminTeams.editDialog.selectNewAdmin')}
                      onChange={(e) => setSelectedAdminId(e.target.value)}
                      disabled={isSubmitting}
                    >
                      {availableMembers.map((member) => (
                        <MenuItem key={member.id} value={member.id}>
                          {member.name || member.email || 'Unknown User'}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                  <Button
                    variant="outlined"
                    color="error"
                    onClick={handleTransfer}
                    disabled={isSubmitting || !selectedAdminId}
                  >
                    {t('adminTeams.editDialog.transferButton')}
                  </Button>
                </Box>
              ) : (
                <Alert severity="info" sx={{ mt: 2 }}>
                  {t('adminTeams.editDialog.noMembersToTransfer')}
                </Alert>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSubmitting}>
            {t('common.cancel')}
          </Button>
          <Button
            type="submit"
            color="primary"
            variant="contained"
            disabled={isSubmitting || !name.trim() || !hasChanges}
          >
            {t('common.save')}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};

export default EditTeamDialog;
