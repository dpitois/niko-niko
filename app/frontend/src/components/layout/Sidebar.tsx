import React from 'react';
import { useTranslation } from 'react-i18next';
import { NavLink, useNavigate } from 'react-router-dom';
import Brightness4Icon from '@mui/icons-material/Brightness4';
import Brightness7Icon from '@mui/icons-material/Brightness7';
import BugReportIcon from '@mui/icons-material/BugReport';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import DashboardIcon from '@mui/icons-material/Dashboard';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';
import GroupWorkIcon from '@mui/icons-material/GroupWork';
import HistoryIcon from '@mui/icons-material/History';
import LanguageIcon from '@mui/icons-material/Language';
import LoginIcon from '@mui/icons-material/Login';
import LogoutIcon from '@mui/icons-material/Logout';
import PeopleIcon from '@mui/icons-material/People';
import TimelineIcon from '@mui/icons-material/Timeline';
import {
  Avatar,
  Box,
  Button,
  Collapse,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Divider,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  Tooltip,
  Typography,
} from '@mui/material';
import { styled } from '@mui/material/styles';

import { useAuth } from '@/context/AuthContext';
import { useColorMode } from '@/context/ColorModeContext';

const DrawerHeader = styled('div')(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  padding: theme.spacing(0, 1),
  ...theme.mixins.toolbar,
  justifyContent: 'space-between', // Changed from flex-end to space-between
}));

interface SidebarProps {
  open: boolean;
  handleDrawerClose: () => void;
  handleDrawerOpen: () => void; // Added for completeness, though not used to open from within sidebar
}

const Sidebar: React.FC<SidebarProps> = ({ open, handleDrawerClose, handleDrawerOpen }) => {
  const { user, logout, isSuperAdmin, userTeamRoles } = useAuth();
  const { toggleColorMode, mode } = useColorMode();
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();

  const repoUrl = import.meta.env.VITE_GITHUB_REPO_URL;

  const isAnyTeamAdmin = Object.values(userTeamRoles).some((role) => role.isAdmin);
  const showAdminMenu = isSuperAdmin || isAnyTeamAdmin;

  const [openAdminMenu, setOpenAdminMenu] = React.useState(false);
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const [openLogoutDialog, setOpenLogoutDialog] = React.useState(false);

  const handleAdminMenuClick = () => {
    setOpenAdminMenu(!openAdminMenu);
  };

  const handleUserMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleUserMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLanguageSwitch = () => {
    const newLang = i18n.language === 'fr' ? 'en' : 'fr';
    i18n.changeLanguage(newLang);
  };

  const handleLogoutClick = () => {
    handleUserMenuClose();
    setOpenLogoutDialog(true);
  };

  const handleConfirmLogout = () => {
    setOpenLogoutDialog(false);
    logout();
    navigate('/login');
    window.location.reload();
  };

  const handleCancelLogout = () => {
    setOpenLogoutDialog(false);
  };

  const navItems = user ? (
    <>
      <List>
        <ListItem disablePadding sx={{ display: 'block' }}>
          <ListItemButton
            component={NavLink}
            to="/my-teams"
            sx={{
              minHeight: 48,
              justifyContent: open ? 'initial' : 'center',
              px: 2.5,
            }}
          >
            <ListItemIcon
              sx={{
                minWidth: 0,
                mr: open ? 3 : 'auto',
                justifyContent: 'center',
              }}
            >
              <DashboardIcon />
            </ListItemIcon>
            <ListItemText primary={t('sidebar.home')} sx={{ opacity: open ? 1 : 0 }} />
          </ListItemButton>
        </ListItem>

        <ListItem disablePadding sx={{ display: 'block' }}>
          <ListItemButton
            component={NavLink}
            to="/current-sprints"
            sx={{
              minHeight: 48,
              justifyContent: open ? 'initial' : 'center',
              px: 2.5,
            }}
          >
            <ListItemIcon
              sx={{
                minWidth: 0,
                mr: open ? 3 : 'auto',
                justifyContent: 'center',
              }}
            >
              <TimelineIcon />
            </ListItemIcon>
            <ListItemText primary={t('sidebar.currentSprint')} sx={{ opacity: open ? 1 : 0 }} />
          </ListItemButton>
        </ListItem>

        <ListItem disablePadding sx={{ display: 'block' }}>
          <ListItemButton
            component={NavLink}
            to="/past-sprints"
            sx={{
              minHeight: 48,
              justifyContent: open ? 'initial' : 'center',
              px: 2.5,
            }}
          >
            <ListItemIcon
              sx={{
                minWidth: 0,
                mr: open ? 3 : 'auto',
                justifyContent: 'center',
              }}
            >
              <HistoryIcon />
            </ListItemIcon>
            <ListItemText primary={t('sidebar.pastSprints')} sx={{ opacity: open ? 1 : 0 }} />
          </ListItemButton>
        </ListItem>

        {showAdminMenu && (
          <>
            <Divider />
            <ListItem disablePadding sx={{ display: 'block' }}>
              <ListItemButton
                onClick={handleAdminMenuClick}
                sx={{
                  justifyContent: open ? 'initial' : 'center',
                  px: 2.5,
                  py: 0.5, // Réduire le padding vertical
                  flexGrow: 0, // Annuler le flex-grow: 1
                }}
              >
                <ListItemIcon
                  sx={{
                    minWidth: 0,
                    mr: open ? 3 : 'auto',
                    justifyContent: 'center',
                  }}
                >
                  <PeopleIcon />
                </ListItemIcon>
                <ListItemText primary={t('sidebar.admin')} sx={{ opacity: open ? 1 : 0 }} />
                {openAdminMenu ? <ExpandLess /> : <ExpandMore />}
              </ListItemButton>
              <Collapse in={openAdminMenu} timeout="auto" unmountOnExit>
                <List component="div" disablePadding>
                  {isSuperAdmin && (
                    <ListItem disablePadding sx={{ display: 'block' }}>
                      <ListItemButton
                        component={NavLink}
                        to="/admin/teams"
                        sx={{
                          minHeight: 48,
                          justifyContent: open ? 'initial' : 'center',
                          px: open ? 4.5 : 2.5, // Indent for sub-items
                        }}
                      >
                        <ListItemIcon
                          sx={{
                            minWidth: 0,
                            mr: open ? 3 : 'auto',
                            justifyContent: 'center',
                          }}
                        >
                          <GroupWorkIcon />
                        </ListItemIcon>
                        <ListItemText primary={t('sidebar.teams')} sx={{ opacity: open ? 1 : 0 }} />
                      </ListItemButton>
                    </ListItem>
                  )}

                  <ListItem disablePadding sx={{ display: 'block' }}>
                    <ListItemButton
                      component={NavLink}
                      to="/admin/users"
                      sx={{
                        minHeight: 48,
                        justifyContent: open ? 'initial' : 'center',
                        px: open ? 4.5 : 2.5, // Indent for sub-items
                      }}
                    >
                      <ListItemIcon
                        sx={{
                          minWidth: 0,
                          mr: open ? 3 : 'auto',
                          justifyContent: 'center',
                        }}
                      >
                        <PeopleIcon />
                      </ListItemIcon>
                      <ListItemText primary={t('sidebar.users')} sx={{ opacity: open ? 1 : 0 }} />
                    </ListItemButton>
                  </ListItem>

                  <ListItem disablePadding sx={{ display: 'block' }}>
                    <ListItemButton
                      component={NavLink}
                      to="/admin/sprints"
                      sx={{
                        minHeight: 48,
                        justifyContent: open ? 'initial' : 'center',
                        px: open ? 4.5 : 2.5, // Indent for sub-items
                      }}
                    >
                      <ListItemIcon
                        sx={{
                          minWidth: 0,
                          mr: open ? 3 : 'auto',
                          justifyContent: 'center',
                        }}
                      >
                        <TimelineIcon />
                      </ListItemIcon>
                      <ListItemText primary={t('sidebar.sprints')} sx={{ opacity: open ? 1 : 0 }} />
                    </ListItemButton>
                  </ListItem>
                </List>
              </Collapse>
            </ListItem>
          </>
        )}
      </List>
    </>
  ) : (
    <List>
      <ListItem disablePadding sx={{ display: 'block' }}>
        <ListItemButton
          component={NavLink}
          to="/login"
          sx={{
            minHeight: 48,
            justifyContent: open ? 'initial' : 'center',
            px: 2.5,
          }}
        >
          <ListItemIcon
            sx={{
              minWidth: 0,
              mr: open ? 3 : 'auto',
              justifyContent: 'center',
            }}
          >
            <LoginIcon />
          </ListItemIcon>
          <ListItemText primary={t('sidebar.login')} sx={{ opacity: open ? 1 : 0 }} />
        </ListItemButton>
      </ListItem>
    </List>
  );

  return (
    <>
      <DrawerHeader>
        <Typography
          variant="h6"
          noWrap
          component="div"
          sx={{
            ml: 2,
            opacity: open ? 1 : 0,
            transition: 'opacity 0.2s',
            fontWeight: 'bold',
            flexGrow: 1,
          }}
        >
          {t('common.appName')}
        </Typography>
        <IconButton onClick={open ? handleDrawerClose : handleDrawerOpen}>
          {open ? <ChevronLeftIcon /> : <ChevronRightIcon />}
        </IconButton>
      </DrawerHeader>
      <Divider />
      {navItems}
      <Box sx={{ flexGrow: 1 }} /> {/* Pushes user info to the bottom */}
      <Divider />
      {user && (
        <>
          <List>
            <ListItem disablePadding sx={{ display: 'block' }}>
              <Tooltip title={!open ? user.name : ''} placement="right" arrow>
                <ListItemButton
                  onClick={handleUserMenuOpen}
                  sx={{
                    minHeight: 48,
                    justifyContent: open ? 'initial' : 'center',
                    px: 2.5,
                  }}
                >
                  <ListItemIcon
                    sx={{
                      minWidth: 0,
                      mr: open ? 2 : 'auto',
                      justifyContent: 'center',
                    }}
                  >
                    <Avatar src={user.avatar_url} alt={user.name} sx={{ width: 32, height: 32 }}>
                      {user.name ? user.name[0].toUpperCase() : '?'}
                    </Avatar>
                  </ListItemIcon>
                  <ListItemText
                    primary={user.name}
                    secondary={user.email || ''}
                    sx={{
                      opacity: open ? 1 : 0,
                      '& .MuiListItemText-secondary': {
                        fontSize: '0.7rem',
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                        whiteSpace: 'nowrap',
                      },
                    }}
                  />
                </ListItemButton>
              </Tooltip>
            </ListItem>
          </List>

          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleUserMenuClose}
            onClick={handleUserMenuClose}
            transformOrigin={{ horizontal: 'left', vertical: 'bottom' }}
            anchorOrigin={{ horizontal: 'left', vertical: 'top' }}
            slotProps={{
              paper: {
                elevation: 3,
                sx: {
                  mb: 1,
                  minWidth: 180,
                  '& .MuiAvatar-root': {
                    width: 32,
                    height: 32,
                    ml: -0.5,
                    mr: 1,
                  },
                },
              },
            }}
          >
            {repoUrl && (
              <MenuItem component="a" href={repoUrl} target="_blank" rel="noopener noreferrer">
                <ListItemIcon>
                  <BugReportIcon fontSize="small" />
                </ListItemIcon>
                {t('sidebar.reportBug')}
              </MenuItem>
            )}
            <MenuItem
              onClick={(e) => {
                e.stopPropagation();
                toggleColorMode();
              }}
            >
              <ListItemIcon>
                {mode === 'dark' ? (
                  <Brightness7Icon fontSize="small" />
                ) : (
                  <Brightness4Icon fontSize="small" />
                )}
              </ListItemIcon>
              {t('sidebar.mode', { mode: mode === 'dark' ? 'Light' : 'Dark' })}
            </MenuItem>
            <MenuItem
              onClick={(e) => {
                e.stopPropagation();
                handleLanguageSwitch();
              }}
            >
              <ListItemIcon>
                <LanguageIcon fontSize="small" />
              </ListItemIcon>
              {i18n.language === 'fr' ? 'English' : 'Français'}
            </MenuItem>
            <Divider />
            <MenuItem onClick={handleLogoutClick}>
              <ListItemIcon>
                <LogoutIcon fontSize="small" />
              </ListItemIcon>
              {t('common.logout')}
            </MenuItem>
          </Menu>

          <Dialog
            open={openLogoutDialog}
            onClose={handleCancelLogout}
            aria-labelledby="logout-dialog-title"
            aria-describedby="logout-dialog-description"
          >
            <DialogTitle id="logout-dialog-title">{t('common.logoutDialog.title')}</DialogTitle>
            <DialogContent>
              <DialogContentText id="logout-dialog-description">
                {t('common.logoutDialog.content')}
              </DialogContentText>
            </DialogContent>
            <DialogActions>
              <Button onClick={handleCancelLogout}>{t('common.cancel')}</Button>
              <Button onClick={handleConfirmLogout} color="error" autoFocus>
                {t('common.logoutDialog.confirm')}
              </Button>
            </DialogActions>
          </Dialog>
        </>
      )}
    </>
  );
};

export default Sidebar;
