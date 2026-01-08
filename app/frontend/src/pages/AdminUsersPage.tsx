import React, { useState, useEffect } from 'react';
import { Box, Typography, Tabs, Tab, CircularProgress, Alert, TableContainer, Table, TableHead, TableRow, TableCell, TableBody, Avatar, Paper, IconButton, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Button, Divider, List, ListItem, Grid } from '@mui/material';
import { Delete as DeleteIcon } from '@mui/icons-material';
import { useUsers } from '../hooks/useUsers';
import { useSnackbar } from 'notistack';
import { deleteUser } from '../services/userService';
import useTeams from '../hooks/useTeams'; // For team selection in invitations
import CreateTeamInvitationForm from '../components/CreateTeamInvitationForm'; // For creating invitations
import { teamInvitationService } from '../services/teamInvitationService'; // For fetching/deleting invitations
import useSWR from 'swr'; // For invitations
import type { TeamInvitation } from '../models/Team/Invitation/TeamInvitation';
import type { TeamWithMembersAndSprints } from '../models/Team/TeamWithMembersAndSprints';
import axios from 'axios'; // For error handling

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

function a11yProps(index: number) {
  return {
    id: `simple-tab-${index}`,
    'aria-controls': `simple-tabpanel-${index}`,
  };
}

const AdminUsersPage: React.FC = () => {
  const [value, setValue] = useState(0);
  const { users, isLoading: isLoadingUsers, isError: isErrorUsers, mutate } = useUsers();
  const { enqueueSnackbar } = useSnackbar();

  // User Deletion State
  const [openConfirmUserDialog, setOpenConfirmUserDialog] = useState(false);
  const [userToDeleteId, setUserToDeleteId] = useState<string | null>(null);
  const [userToDeleteName, setUserToDeleteName] = useState<string | null>(null);

  // Invitation Management State
  const { teams, isLoading: isLoadingTeams, isError: isErrorTeams } = useTeams();
  const [selectedTeamId, setSelectedTeamId] = useState<string | null>(null);
  const [invitationToDeleteId, setInvitationToDeleteId] = useState<string | null>(null);
  const [openConfirmInvitationDialog, setOpenConfirmInvitationDialog] = useState(false);


  // Set first team as default for invitation management
  useEffect(() => {
    if (teams && teams.length > 0 && !selectedTeamId) {
      setSelectedTeamId(teams[0].id);
    }
  }, [teams, selectedTeamId]);

  // Fetch invitations for selected team
  const { data: invitations, isLoading: isLoadingInvitations, error: invitationsError, mutate: mutateTeamInvitations } = useSWR<TeamInvitation[]>(
    selectedTeamId ? `/teams/${selectedTeamId}/invitations` : null,
    () => teamInvitationService.getTeamInvitations(selectedTeamId!)
  );

  const handleChange = (_event: React.SyntheticEvent, newValue: number) => {
    setValue(newValue);
  };

  // --- User Deletion Handlers ---
  const handleDeleteUserClick = (userId: string, userName: string) => {
    setUserToDeleteId(userId);
    setUserToDeleteName(userName);
    setOpenConfirmUserDialog(true);
  };

  const handleConfirmDeleteUser = async () => {
    if (userToDeleteId) {
      try {
        await deleteUser(userToDeleteId);
        enqueueSnackbar(`User ${userToDeleteName} deleted successfully!`, { variant: 'success' });
        mutate();
      } catch (error) {
        console.error('Failed to delete user:', error);
        enqueueSnackbar(`Failed to delete user ${userToDeleteName}.`, { variant: 'error' });
      } finally {
        setOpenConfirmUserDialog(false);
        setUserToDeleteId(null);
        setUserToDeleteName(null);
      }
    }
  };

  const handleCancelDeleteUser = () => {
    setOpenConfirmUserDialog(false);
    setUserToDeleteId(null);
    setUserToDeleteName(null);
  };

  // --- Invitation Management Handlers ---
  const handleInvitationCreated = () => {
    mutateTeamInvitations();
    enqueueSnackbar('Invitation created successfully!', { variant: 'success' });
  };

  const handleDeleteInvitationClick = (invitationId: string) => {
    setInvitationToDeleteId(invitationId);
    setOpenConfirmInvitationDialog(true);
  };

  const handleConfirmDeleteInvitation = async () => {
    if (invitationToDeleteId) {
      try {
        await teamInvitationService.deleteTeamInvitation(invitationToDeleteId);
        enqueueSnackbar('Invitation deleted successfully!', { variant: 'success' });
        mutateTeamInvitations();
      } catch (error: unknown) {
        let errorMessage = 'Failed to delete invitation.';
        if (axios.isAxiosError(error) && error.response?.data?.message) {
          errorMessage = error.response.data.message;
        } else if (error instanceof Error) {
          errorMessage = error.message;
        }
        enqueueSnackbar(errorMessage, { variant: 'error' });
      } finally {
        setOpenConfirmInvitationDialog(false);
        setInvitationToDeleteId(null);
      }
    }
  };

  const handleCancelDeleteInvitation = () => {
    setOpenConfirmInvitationDialog(false);
    setInvitationToDeleteId(null);
  };

  return (
    <Box sx={{ width: '100%' }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Admin Users
      </Typography>

      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs value={value} onChange={handleChange} aria-label="admin users tabs">
          <Tab label="User Management" {...a11yProps(0)} />
          <Tab label="Invitations" {...a11yProps(1)} />
        </Tabs>
      </Box>
      <TabPanel value={value} index={0}>
        {isLoadingUsers && <CircularProgress />}
        {isErrorUsers && <Alert severity="error">Failed to load users.</Alert>}

        {users && users.length > 0 ? (
          <TableContainer component={Paper}>
            <Table sx={{ minWidth: 650 }} aria-label="users table">
              <TableHead>
                <TableRow>
                  <TableCell>Avatar</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Created At</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {users.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell>
                      <Avatar src={user.avatarUrl} alt={user.name || user.email}>
                        {user.name ? user.name[0].toUpperCase() : user.email[0].toUpperCase()}
                      </Avatar>
                    </TableCell>
                    <TableCell>{user.name}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>{new Date(user.createdAt).toLocaleDateString()}</TableCell>
                    <TableCell>
                      <IconButton
                        aria-label="delete user"
                        onClick={() => handleDeleteUserClick(user.id, user.name || user.email)}
                        color="error"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        ) : (!isLoadingUsers && !isErrorUsers && <Typography>No users found.</Typography>)}
      </TabPanel>
      <TabPanel value={value} index={1}>
        {isLoadingTeams && <CircularProgress />}
        {isErrorTeams && <Alert severity="error">Failed to load teams for invitations.</Alert>}

        {teams && teams.length > 0 ? (
          <>
            <Box sx={{ mb: 3 }}>
              <Typography variant="h6" component="h2" gutterBottom>
                Select a Team to Manage Invitations
              </Typography>
              <select
                value={selectedTeamId || ''}
                onChange={(e) => setSelectedTeamId(e.target.value)}
                style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ccc', marginBottom: '16px' }}
              >
                <option value="">Select a team</option>
                {teams.map((team: TeamWithMembersAndSprints) => (
                  <option key={team.id} value={team.id}>{team.name}</option>
                ))}
              </select>
            </Box>

            {selectedTeamId && (
              <>
                <Box sx={{ mb: 3 }}>
                  <CreateTeamInvitationForm teamId={selectedTeamId} onInvitationCreated={handleInvitationCreated} />
                </Box>
                <Divider sx={{ my: 3 }} />
                <Typography variant="h6" gutterBottom>Existing Team Invitations</Typography>
                {isLoadingInvitations && <CircularProgress />}
                {invitationsError && <Alert severity="error">{invitationsError?.message || 'Failed to load invitations.'}</Alert>}
                {!isLoadingInvitations && invitations && invitations.length > 0 ? (
                  <List>
                    {invitations.map((invitation) => (
                      <ListItem key={invitation.id} secondaryAction={
                        <IconButton edge="end" aria-label="delete" onClick={() => handleDeleteInvitationClick(invitation.id)}>
                          <DeleteIcon />
                        </IconButton>
                      }>
                        <Grid container spacing={2} alignItems="center">
                          <Grid size={{ xs: 12, sm: 6 }}>
                            <Typography variant="body1">Token: {invitation.token.substring(0, 10)}...</Typography>
                            <Typography variant="body2" color="text.secondary">Expires: {new Date(invitation.expirationDate).toLocaleDateString()}</Typography>
                          </Grid>
                          <Grid size={{ xs: 12, sm: 6 }}>
                            <Typography variant="body2" color={invitation.status === 'Accepted' ? 'success.main' : invitation.status === 'Expired' ? 'error.main' : 'info.main'}>
                              Status: {invitation.status}
                            </Typography>
                            <Button variant="outlined" size="small" onClick={() => navigator.clipboard.writeText(`${window.location.origin}/accept-invitation/${invitation.token}`)} sx={{ mt: 1 }}>
                              Copy Link
                            </Button>
                          </Grid>
                        </Grid>
                      </ListItem>
                    ))}
                  </List>
                ) : (!isLoadingInvitations && !invitationsError && (
                  <Typography>No invitations found for this team.</Typography>
                ))}
              </>
            )}
          </>
        ) : (!isLoadingTeams && !isErrorTeams && <Typography>No teams available to manage invitations.</Typography>)}
      </TabPanel>

      <Dialog
        open={openConfirmUserDialog}
        onClose={handleCancelDeleteUser}
        aria-labelledby="confirm-delete-user-dialog-title"
        aria-describedby="confirm-delete-user-dialog-description"
      >
        <DialogTitle id="confirm-delete-user-dialog-title">{"Confirm Delete User"}</DialogTitle>
        <DialogContent>
          <DialogContentText id="confirm-delete-user-dialog-description">
            Are you sure you want to delete user "{userToDeleteName}"? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCancelDeleteUser}>Cancel</Button>
          <Button onClick={handleConfirmDeleteUser} color="error" autoFocus>
            Delete
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={openConfirmInvitationDialog}
        onClose={handleCancelDeleteInvitation}
        aria-labelledby="confirm-delete-invitation-dialog-title"
        aria-describedby="confirm-delete-invitation-dialog-description"
      >
        <DialogTitle id="confirm-delete-invitation-dialog-title">{"Confirm Delete Invitation"}</DialogTitle>
        <DialogContent>
          <DialogContentText id="confirm-delete-invitation-dialog-description">
            Are you sure you want to delete this invitation? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCancelDeleteInvitation}>Cancel</Button>
          <Button onClick={handleConfirmDeleteInvitation} color="error" autoFocus>
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default AdminUsersPage;
