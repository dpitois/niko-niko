import React, { useState } from 'react';
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from '@mui/material';

interface EditTeamDialogProps {
  open: boolean;
  onClose: () => void;
  onUpdate: (newName: string) => Promise<void>;
  currentName: string;
}

const EditTeamDialog: React.FC<EditTeamDialogProps> = ({
  open,
  onClose,
  onUpdate,
  currentName,
}) => {
  const [name, setName] = useState(currentName);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || name === currentName) {
      onClose();
      return;
    }

    setIsSubmitting(true);
    try {
      await onUpdate(name);
      onClose();
    } catch (error) {
      console.error('Failed to update team name:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Edit Team Name</DialogTitle>
      <form onSubmit={handleSubmit}>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Team Name"
            type="text"
            fullWidth
            variant="outlined"
            value={name}
            onChange={(e) => setName(e.target.value)}
            disabled={isSubmitting}
            required
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button
            type="submit"
            color="primary"
            variant="contained"
            disabled={isSubmitting || !name.trim() || name === currentName}
          >
            Save
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};

export default EditTeamDialog;
