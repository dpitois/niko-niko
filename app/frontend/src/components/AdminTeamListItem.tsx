import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import FaceIcon from '@mui/icons-material/Face';
import GroupIcon from '@mui/icons-material/Group';
import {
  Avatar,
  Box,
  Button,
  Chip,
  IconButton,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Paper,
  Typography,
} from '@mui/material';
import { useSnackbar } from 'notistack';

import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';
import { transferTeamAdmin,updateTeam } from '@/services/teamService';

import EditTeamDialog from './EditTeamDialog';

interface AdminTeamListItemProps {
  team: TeamWithMembersAndSprints;
  onDelete: (teamId: string) => void;
  onUpdate?: () => void;
}

const AdminTeamListItem: React.FC<AdminTeamListItemProps> = ({ team, onDelete, onUpdate }) => {
  const { t } = useTranslation();
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const { enqueueSnackbar } = useSnackbar();

  const handleUpdateName = async (newName: string) => {
    try {
      await updateTeam(team.id, newName);
      enqueueSnackbar(t('dashboard.teamUpdated'), { variant: 'success' });
      if (onUpdate) onUpdate();
    } catch {
      enqueueSnackbar(t('dashboard.teamUpdateFailed'), { variant: 'error' });
      throw new Error('Update failed');
    }
  };

  const handleTransferAdmin = async (newAdminId: string) => {
    try {
      await transferTeamAdmin(team.id, newAdminId);
      enqueueSnackbar(t('adminTeams.teamAdminTransferred'), { variant: 'success' });
      if (onUpdate) onUpdate();
    } catch {
      enqueueSnackbar(t('adminTeams.teamAdminTransferFailed'), { variant: 'error' });
      throw new Error('Transfer failed');
    }
  };

  return (
    <Paper elevation={2} sx={{ p: 2, mb: 3, display: 'flex', flexDirection: 'column' }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Typography variant="h6" component="div">
            {team.name}
          </Typography>
          <IconButton
            size="small"
            onClick={() => setIsEditDialogOpen(true)}
            title={t('adminTeams.listItem.editNameTitle')}
            sx={{ color: 'text.secondary' }}
          >
            <EditIcon fontSize="small" />
          </IconButton>
          <Chip
            icon={<FaceIcon />}
            label={t('dashboard.owner', { name: team.adminName })}
            variant="outlined"
            size="small"
            color="primary"
          />
        </Box>
        <Button
          variant="outlined"
          color="error"
          startIcon={<DeleteIcon />}
          onClick={() => onDelete(team.id)}
          size="small"
        >
          {t('common.delete')}
        </Button>
      </Box>

      <EditTeamDialog
        open={isEditDialogOpen}
        onClose={() => setIsEditDialogOpen(false)}
        onUpdate={handleUpdateName}
        currentName={team.name}
        members={team.members}
        currentAdminId={team.adminId}
        onTransfer={handleTransferAdmin}
      />
      <Box>
        <Typography variant="subtitle2" sx={{ mb: 1, display: 'flex', alignItems: 'center' }}>
          <GroupIcon sx={{ mr: 0.5 }} fontSize="small" />{' '}
          {t('adminTeams.listItem.membersCount', { count: team.members.length })}
        </Typography>
        <List dense>
          {team.members.map((member) => (
            <ListItem key={member.id} disablePadding>
              <ListItemAvatar>
                <Avatar
                  src={member.avatarUrl}
                  alt={member.name || member.email}
                  sx={{ width: 24, height: 24, fontSize: '0.75rem' }}
                >
                  {member.name ? member.name[0].toUpperCase() : member.email[0].toUpperCase()}
                </Avatar>
              </ListItemAvatar>
              <ListItemText
                primary={member.name || member.email}
                primaryTypographyProps={{ variant: 'body2' }}
              />
            </ListItem>
          ))}
        </List>
      </Box>
    </Paper>
  );
};

export default AdminTeamListItem;
