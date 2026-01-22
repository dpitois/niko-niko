import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  TextField,
  Typography,
} from '@mui/material';
import dayjs from 'dayjs';
import { isAxiosError } from 'axios';

import type { Sprint } from '@/models/Sprint';
import type { UpdateSprint } from '@/models/UpdateSprint';
import { updateSprint } from '@/services/sprintService';

interface AdminEditSprintDialogProps {
  sprint: Sprint | null;
  open: boolean;
  onClose: () => void;
  onSprintUpdated: () => void;
}

const AdminEditSprintDialog: React.FC<AdminEditSprintDialogProps> = ({
  sprint,
  open,
  onClose,
  onSprintUpdated,
}) => {
  const { t } = useTranslation();
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (sprint && open) {
      setName(sprint.name);

      setStartDate(dayjs(sprint.startDate).format('YYYY-MM-DD'));
      setEndDate(dayjs(sprint.endDate).format('YYYY-MM-DD'));
      setError(null);
    }
  }, [sprint, open]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!sprint) return;

    setError(null);
    setIsSubmitting(true);

    if (!name || !startDate || !endDate) {
      setError(t('adminSprints.createForm.errorFill'));
      setIsSubmitting(false);
      return;
    }

    if (dayjs(startDate).isSameOrAfter(dayjs(endDate), 'day')) {
      setError(t('adminSprints.createForm.errorDate'));
      setIsSubmitting(false);
      return;
    }

    const updatedSprint: UpdateSprint = {
      name,
      startDate,
      endDate,
    };

    try {
      await updateSprint(sprint.id, updatedSprint);
      onSprintUpdated();
      onClose();
    } catch (err) {
      // Check if it's a validation error from API (400 BadRequest)
      if (isAxiosError(err) && err.response?.status === 400) {
        setError(t('adminSprints.editDialog.errorFail'));
      } else {
        setError(t('common.error'));
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>{t('adminSprints.editDialog.title')}</DialogTitle>
      <Box component="form" onSubmit={handleSubmit}>
        <DialogContent>
          {error && (
            <Typography color="error" variant="body2" sx={{ mb: 2 }}>
              {error}
            </Typography>
          )}
          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <TextField
                id="edit-sprint-name"
                label={t('adminSprints.createForm.nameLabel')}
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                fullWidth
                required
                disabled={isSubmitting}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                id="edit-start-date"
                label={t('adminSprints.createForm.startDateLabel')}
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                InputLabelProps={{
                  shrink: true,
                }}
                fullWidth
                required
                disabled={isSubmitting}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                id="edit-end-date"
                label={t('adminSprints.createForm.endDateLabel')}
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                InputLabelProps={{
                  shrink: true,
                }}
                fullWidth
                required
                disabled={isSubmitting}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSubmitting}>
            {t('common.cancel')}
          </Button>
          <Button type="submit" variant="contained" color="primary" disabled={isSubmitting}>
            {isSubmitting ? t('common.loading') : t('common.save')}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
};

export default AdminEditSprintDialog;
