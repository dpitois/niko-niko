import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import DeleteIcon from '@mui/icons-material/Delete';
import PeopleIcon from '@mui/icons-material/People';
import {
  Alert,
  Avatar,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Divider,
  FormControl,
  Grid,
  IconButton,
  InputLabel,
  List,
  ListItem,
  MenuItem,
  Paper,
  Select,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  Typography,
} from '@mui/material';
import axios from 'axios';
import { useSnackbar } from 'notistack';
import useSWR from 'swr';

import { useAuth } from '@/context/AuthContext';
import useTeams from '@/hooks/useTeams';
import { useUsers } from '@/hooks/useUsers';
import type { TeamInvitation } from '@/models/Team/Invitation/TeamInvitation';
import type { TeamWithMembersAndSprints } from '@/models/Team/TeamWithMembersAndSprints';
import { teamInvitationService } from '@/services/teamInvitationService';
import { deleteUser } from '@/services/userService';

import CreateTeamInvitationForm from '@/components/CreateTeamInvitationForm';
import PageContainer from '@/components/layout/PageContainer';

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
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
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
  const { t } = useTranslation();
  const [value, setValue] = useState(0);
  const { user: currentUser } = useAuth();
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
  const {
    data: invitations,
    isLoading: isLoadingInvitations,
    error: invitationsError,
    mutate: mutateTeamInvitations,
  } = useSWR<TeamInvitation[]>(selectedTeamId ? `/teams/${selectedTeamId}/invitations` : null, () =>
    teamInvitationService.getTeamInvitations(selectedTeamId!),
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
        enqueueSnackbar(t('adminUsers.deleteDialog.success', { name: userToDeleteName }), {
          variant: 'success',
        });
        mutate();
      } catch {
        enqueueSnackbar(t('adminUsers.deleteDialog.fail', { name: userToDeleteName }), {
          variant: 'error',
        });
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
    enqueueSnackbar(t('adminUsers.invitations.createForm.success'), { variant: 'success' });
  };

  const handleDeleteInvitationClick = (invitationId: string) => {
    setInvitationToDeleteId(invitationId);
    setOpenConfirmInvitationDialog(true);
  };

  const handleConfirmDeleteInvitation = async () => {
    if (invitationToDeleteId) {
      try {
        await teamInvitationService.deleteTeamInvitation(invitationToDeleteId);
        enqueueSnackbar(t('adminUsers.invitations.deleteDialog.success'), { variant: 'success' });
        mutateTeamInvitations();
      } catch (error: unknown) {
        let errorMessage = t('adminUsers.invitations.deleteDialog.fail');
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
    <PageContainer title={t('adminUsers.title')} icon={<PeopleIcon />}>
      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs value={value} onChange={handleChange} aria-label="admin users tabs">
          <Tab label={t('adminUsers.tabs.management')} {...a11yProps(0)} />
          <Tab label={t('adminUsers.tabs.invitations')} {...a11yProps(1)} />
        </Tabs>
      </Box>
      <TabPanel value={value} index={0}>
        {isLoadingUsers && <CircularProgress />}
        {isErrorUsers && <Alert severity="error">{t('common.error')}</Alert>}

        {users && users.length > 0 ? (
          <TableContainer component={Paper}>
            <Table sx={{ minWidth: 650 }} aria-label="users table">
              <TableHead>
                <TableRow>
                  <TableCell>{t('adminUsers.table.avatar')}</TableCell>
                  <TableCell>{t('adminUsers.table.name')}</TableCell>
                  <TableCell>{t('adminUsers.table.email')}</TableCell>
                  <TableCell>{t('adminUsers.table.createdAt')}</TableCell>
                  <TableCell>{t('adminUsers.table.actions')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {users.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell>
                      <Avatar src={user.avatarUrl} alt={user.name || user.email || 'User'}>
                        {user.name
                          ? user.name[0].toUpperCase()
                          : user.email
                            ? user.email[0].toUpperCase()
                            : '?'}
                      </Avatar>
                    </TableCell>
                    <TableCell>{user.name}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>
                      {user.createdAt ? new Date(user.createdAt).toLocaleDateString() : '-'}
                    </TableCell>
                    <TableCell>
                      <IconButton
                        aria-label="delete user"
                        onClick={() =>
                          handleDeleteUserClick(user.id, user.name || user.email || 'Unknown User')
                        }
                        color="error"
                        disabled={currentUser?.sub === user.id}
                      >
                        <DeleteIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        ) : (
          !isLoadingUsers &&
          !isErrorUsers && <Typography>{t('adminUsers.table.noUsers')}</Typography>
        )}
      </TabPanel>
      <TabPanel value={value} index={1}>
        {isLoadingTeams && <CircularProgress />}
        {isErrorTeams && <Alert severity="error">{t('common.error')}</Alert>}

        {teams && teams.length > 0 ? (
          <>
            <Box sx={{ mb: 3 }}>
              <Typography variant="h6" component="h2" gutterBottom>
                {t('adminUsers.invitations.selectTeamTitle')}
              </Typography>
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel id="team-select-label">
                  {t('adminUsers.invitations.selectTeamLabel')}
                </InputLabel>
                <Select
                  labelId="team-select-label"
                  id="team-select"
                  value={selectedTeamId || ''}
                  label={t('adminUsers.invitations.selectTeamLabel')}
                  onChange={(e) => setSelectedTeamId(e.target.value)}
                >
                  {teams.map((team: TeamWithMembersAndSprints) => (
                    <MenuItem key={team.id} value={team.id}>
                      {team.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>

            {selectedTeamId && (
              <>
                <Box sx={{ mb: 3 }}>
                  <CreateTeamInvitationForm
                    teamId={selectedTeamId}
                    onInvitationCreated={handleInvitationCreated}
                  />
                </Box>
                <Divider sx={{ my: 3 }} />
                <Typography variant="h6" gutterBottom>
                  {t('adminUsers.invitations.existingTitle')}
                </Typography>
                {isLoadingInvitations && <CircularProgress />}
                {invitationsError && (
                  <Alert severity="error">{invitationsError?.message || t('common.error')}</Alert>
                )}
                {!isLoadingInvitations && invitations && invitations.length > 0 ? (
                  <List>
                    {invitations.map((invitation) => (
                      <ListItem
                        key={invitation.id}
                        secondaryAction={
                          <IconButton
                            edge="end"
                            aria-label="delete"
                            onClick={() => handleDeleteInvitationClick(invitation.id)}
                          >
                            <DeleteIcon />
                          </IconButton>
                        }
                      >
                        <Grid container spacing={2} alignItems="center">
                          <Grid size={{ xs: 12, sm: 6 }}>
                            <Typography variant="body1">
                              {t('adminUsers.invitations.token', {
                                token: invitation.token.substring(0, 10),
                              })}
                            </Typography>
                            <Typography variant="body2" color="text.secondary">
                              {t('adminUsers.invitations.expires', {
                                date: new Date(invitation.expirationDate).toLocaleDateString(),
                              })}
                            </Typography>
                          </Grid>
                          <Grid size={{ xs: 12, sm: 6 }}>
                            <Typography
                              variant="body2"
                              color={
                                invitation.status === 'Accepted'
                                  ? 'success.main'
                                  : invitation.status === 'Expired'
                                    ? 'error.main'
                                    : 'info.main'
                              }
                            >
                              {t('adminUsers.invitations.status', { status: invitation.status })}
                            </Typography>
                            <Button
                              variant="outlined"
                              size="small"
                              onClick={() =>
                                navigator.clipboard.writeText(
                                  `${window.location.origin}/accept-invitation/${invitation.token}`,
                                )
                              }
                              sx={{ mt: 1 }}
                            >
                              {t('common.copyLink')}
                            </Button>
                          </Grid>
                        </Grid>
                      </ListItem>
                    ))}
                  </List>
                ) : (
                  !isLoadingInvitations &&
                  !invitationsError && (
                    <Typography>{t('adminUsers.invitations.noInvitations')}</Typography>
                  )
                )}
              </>
            )}
          </>
        ) : (
          !isLoadingTeams &&
          !isErrorTeams && <Typography>{t('adminUsers.invitations.noTeams')}</Typography>
        )}
      </TabPanel>

      <Dialog
        open={openConfirmUserDialog}
        onClose={handleCancelDeleteUser}
        aria-labelledby="confirm-delete-user-dialog-title"
        aria-describedby="confirm-delete-user-dialog-description"
      >
        <DialogTitle id="confirm-delete-user-dialog-title">
          {t('adminUsers.deleteDialog.title')}
        </DialogTitle>
        <DialogContent>
          <DialogContentText id="confirm-delete-user-dialog-description">
            {t('adminUsers.deleteDialog.content', { name: userToDeleteName })}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCancelDeleteUser}>{t('common.cancel')}</Button>
          <Button onClick={handleConfirmDeleteUser} color="error" autoFocus>
            {t('common.delete')}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={openConfirmInvitationDialog}
        onClose={handleCancelDeleteInvitation}
        aria-labelledby="confirm-delete-invitation-dialog-title"
        aria-describedby="confirm-delete-invitation-dialog-description"
      >
        <DialogTitle id="confirm-delete-invitation-dialog-title">
          {t('adminUsers.invitations.deleteDialog.title')}
        </DialogTitle>
        <DialogContent>
          <DialogContentText id="confirm-delete-invitation-dialog-description">
            {t('adminUsers.invitations.deleteDialog.content')}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCancelDeleteInvitation}>{t('common.cancel')}</Button>
          <Button onClick={handleConfirmDeleteInvitation} color="error" autoFocus>
            {t('common.delete')}
          </Button>
        </DialogActions>
      </Dialog>
    </PageContainer>
  );
};

export default AdminUsersPage;
