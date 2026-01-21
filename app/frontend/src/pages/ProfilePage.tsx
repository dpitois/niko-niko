import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import DeleteIcon from '@mui/icons-material/Delete';
import DownloadIcon from '@mui/icons-material/Download';
import SentimentDissatisfiedIcon from '@mui/icons-material/SentimentDissatisfied';
import SentimentSatisfiedIcon from '@mui/icons-material/SentimentSatisfied';
import SentimentSatisfiedAltIcon from '@mui/icons-material/SentimentSatisfiedAlt';
import {
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
  Grid,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Paper,
  Tab,
  TablePagination,
  Tabs,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import dayjs from 'dayjs';
import { useSnackbar } from 'notistack';
import useSWR from 'swr';

import { useAuth } from '@/context/AuthContext';
import { MoodValues } from '@/models/MoodType';
import { getMyMoodHistory } from '@/services/moodService';
import { deleteMe, exportData, getUser } from '@/services/userService';

import PageContainer from '@/components/layout/PageContainer';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function CustomTabPanel(props: TabPanelProps) {
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

const ProfilePage = () => {
  const { t } = useTranslation();
  const { user, logout } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();
  const [value, setValue] = useState(0);
  const [openDeleteDialog, setOpenDeleteDialog] = useState(false);
  const [isExporting, setIsExporting] = useState(false);

  // Pagination state
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);

  const {
    data: historyData,
    isLoading,
    error: historyError,
  } = useSWR(user ? ['/moodentries/me', page, pageSize] : null, () =>
    getMyMoodHistory(page + 1, pageSize),
  );

  const { data: userDetails } = useSWR(user ? `/users/${user.sub}` : null, () =>
    getUser(user!.sub),
  );

  const handleChange = (_event: React.SyntheticEvent, newValue: number) => {
    setValue(newValue);
  };

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangePageSize = (event: React.ChangeEvent<HTMLInputElement>) => {
    setPageSize(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleDeleteAccount = async () => {
    try {
      await deleteMe();
      enqueueSnackbar(t('profile.deleteDialog.success'), { variant: 'success' });
      logout();
      navigate('/login');
    } catch (error) {
      console.error('Failed to delete account:', error);
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const errorMessage = (error as any).response?.data || t('profile.deleteDialog.error');
      enqueueSnackbar(errorMessage, { variant: 'error' });
      setOpenDeleteDialog(false);
    }
  };

  const handleExport = async () => {
    setIsExporting(true);
    try {
      const blob = await exportData();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `nikoniko-export-${new Date().toISOString().split('T')[0]}.json`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (error) {
      console.error('Export failed', error);
      enqueueSnackbar(t('common.error'), { variant: 'error' });
    } finally {
      setIsExporting(false);
    }
  };

  const getMoodIcon = (moodType: number) => {
    switch (moodType) {
      case MoodValues.Happy:
        return <SentimentSatisfiedAltIcon color="success" />;
      case MoodValues.Neutral:
        return <SentimentSatisfiedIcon color="info" />;
      case MoodValues.Sad:
        return <SentimentDissatisfiedIcon color="error" />;
      default:
        return null;
    }
  };

  const getMoodLabel = (moodType: number) => {
    switch (moodType) {
      case MoodValues.Happy:
        return t('mood.happy');
      case MoodValues.Neutral:
        return t('mood.neutral');
      case MoodValues.Sad:
        return t('mood.sad');
      default:
        return '';
    }
  };

  if (!user) {
    return null;
  }

  return (
    <PageContainer title={t('profile.title')} icon={<AccountCircleIcon />}>
      <Paper sx={{ width: '100%' }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={value} onChange={handleChange} aria-label="profile tabs">
            <Tab label={t('profile.tabs.general')} {...a11yProps(0)} />
            <Tab label={t('profile.tabs.moodHistory')} {...a11yProps(1)} />
          </Tabs>
        </Box>
        <CustomTabPanel value={value} index={0}>
          <Grid container spacing={4}>
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="h6" gutterBottom>
                {t('profile.general.accountInfo')}
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                <Avatar
                  src={user.avatar_url}
                  alt={user.name}
                  sx={{ width: 80, height: 80, mr: 2 }}
                />
                <Box>
                  <Typography variant="h6">{user.name}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {userDetails?.provider || '...'}
                  </Typography>
                </Box>
              </Box>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <TextField
                  label={t('adminUsers.table.name')}
                  value={user.name}
                  slotProps={{
                    input: {
                      readOnly: true,
                    },
                  }}
                  fullWidth
                />
                <TextField
                  label={t('adminUsers.table.email')}
                  value={user.email || ''}
                  slotProps={{
                    input: {
                      readOnly: true,
                    },
                  }}
                  fullWidth
                />
                <TextField
                  label={t('adminUsers.table.createdAt')}
                  value={userDetails?.createdAt ? dayjs(userDetails.createdAt).format('L') : '...'}
                  slotProps={{
                    input: {
                      readOnly: true,
                    },
                  }}
                  fullWidth
                />
              </Box>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="h6" gutterBottom color="error">
                {t('profile.general.privacyZone')}
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <Box>
                  <Typography variant="subtitle1" gutterBottom>
                    {t('profile.general.exportData')}
                  </Typography>
                  <Tooltip title={t('profile.general.exportTooltip')}>
                    <span>
                      <Button
                        variant="outlined"
                        startIcon={isExporting ? <CircularProgress size={20} /> : <DownloadIcon />}
                        disabled={isExporting}
                        onClick={handleExport}
                        fullWidth
                        sx={{ justifyContent: 'flex-start' }}
                      >
                        {isExporting ? t('common.loading') : t('profile.general.exportData')}
                      </Button>
                    </span>
                  </Tooltip>
                </Box>

                <Box sx={{ mt: 2 }}>
                  <Typography variant="subtitle1" color="error" gutterBottom>
                    {t('profile.general.deleteAccount')}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    {t('profile.general.deleteWarning')}
                  </Typography>
                  <Button
                    variant="contained"
                    color="error"
                    startIcon={<DeleteIcon />}
                    onClick={() => setOpenDeleteDialog(true)}
                    fullWidth
                    sx={{ justifyContent: 'flex-start' }}
                  >
                    {t('profile.general.deleteAccount')}
                  </Button>
                </Box>
              </Box>
            </Grid>
          </Grid>
        </CustomTabPanel>
        <CustomTabPanel value={value} index={1}>
          {isLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : historyError ? (
            <Typography color="error">{t('common.error')}</Typography>
          ) : historyData && historyData.items.length > 0 ? (
            <>
              <List>
                {historyData.items.map((entry) => (
                  <ListItem key={entry.id} divider>
                    <ListItemIcon>{getMoodIcon(entry.mood)}</ListItemIcon>
                    <ListItemText
                      primary={getMoodLabel(entry.mood)}
                      secondary={dayjs(entry.date).format('dddd, D MMMM YYYY')}
                    />
                  </ListItem>
                ))}
              </List>
              <TablePagination
                component="div"
                count={historyData.totalCount}
                page={page}
                onPageChange={handleChangePage}
                rowsPerPage={pageSize}
                onRowsPerPageChange={handleChangePageSize}
                rowsPerPageOptions={[5, 10, 25, 50]}
              />
            </>
          ) : (
            <Typography sx={{ p: 2, textAlign: 'center', color: 'text.secondary' }}>
              {t('profile.history.noData')}
            </Typography>
          )}
        </CustomTabPanel>
      </Paper>

      <Dialog
        open={openDeleteDialog}
        onClose={() => setOpenDeleteDialog(false)}
        aria-labelledby="delete-dialog-title"
        aria-describedby="delete-dialog-description"
      >
        <DialogTitle id="delete-dialog-title">{t('profile.deleteDialog.title')}</DialogTitle>
        <DialogContent>
          <DialogContentText id="delete-dialog-description">
            {t('profile.deleteDialog.content')}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDeleteDialog(false)}>{t('common.cancel')}</Button>
          <Button onClick={handleDeleteAccount} color="error" autoFocus>
            {t('profile.deleteDialog.confirm')}
          </Button>
        </DialogActions>
      </Dialog>
    </PageContainer>
  );
};

export default ProfilePage;
