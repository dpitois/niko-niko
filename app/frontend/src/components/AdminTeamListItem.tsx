import React from 'react';
import { Box, Typography, Button, Paper, Avatar, List, ListItem, ListItemAvatar, ListItemText } from '@mui/material';
import { Delete as DeleteIcon, Group as GroupIcon } from '@mui/icons-material';
import type { TeamWithMembersAndSprints } from '../models/Team/TeamWithMembersAndSprints';

interface AdminTeamListItemProps {
  team: TeamWithMembersAndSprints;
  onDelete: (teamId: string) => void;
}

const AdminTeamListItem: React.FC<AdminTeamListItemProps> = ({ team, onDelete }) => {
  return (
    <Paper elevation={2} sx={{ p: 2, mb: 3, display: 'flex', flexDirection: 'column' }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6" component="div">
          {team.name}
        </Typography>
        <Button
          variant="outlined"
          color="error"
          startIcon={<DeleteIcon />}
          onClick={() => onDelete(team.id)}
          size="small"
        >
          Delete
        </Button>
      </Box>
      <Box>
        <Typography variant="subtitle2" sx={{ mb: 1, display: 'flex', alignItems: 'center' }}>
          <GroupIcon sx={{ mr: 0.5 }} fontSize="small" /> Members ({team.members.length})
        </Typography>
        <List dense>
          {team.members.map((member) => (
            <ListItem key={member.id} disablePadding>
              <ListItemAvatar>
                <Avatar sx={{ width: 24, height: 24, fontSize: '0.75rem' }}>
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
