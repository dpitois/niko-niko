import React from 'react';
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
import LoginIcon from '@mui/icons-material/Login';
import LogoutIcon from '@mui/icons-material/Logout';
import PeopleIcon from '@mui/icons-material/People';
import TimelineIcon from '@mui/icons-material/Timeline';
import {
  Avatar,
  Box,
  Collapse,
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
} from '@mui/material';
import { styled } from '@mui/material/styles';

import { useAuth } from '@/context/AuthContext';
import { useColorMode } from '@/context/ColorModeContext';

const DrawerHeader = styled('div')(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  padding: theme.spacing(0, 1),
  ...theme.mixins.toolbar,
  justifyContent: 'flex-end',
}));

interface SidebarProps {
  open: boolean;
  handleDrawerClose: () => void;
  handleDrawerOpen: () => void; // Added for completeness, though not used to open from within sidebar
}

const Sidebar: React.FC<SidebarProps> = ({ open, handleDrawerClose, handleDrawerOpen }) => {
  const { user, logout, isSuperAdmin, userTeamRoles } = useAuth();
  const { toggleColorMode, mode } = useColorMode();
  const navigate = useNavigate();

  const repoUrl = import.meta.env.VITE_GITHUB_REPO_URL;

  const isAnyTeamAdmin = Object.values(userTeamRoles).some((role) => role.isAdmin);
  const showAdminMenu = isSuperAdmin || isAnyTeamAdmin;

  const [openAdminMenu, setOpenAdminMenu] = React.useState(false);
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const handleAdminMenuClick = () => {
    setOpenAdminMenu(!openAdminMenu);
  };

  const handleUserMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleUserMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    handleUserMenuClose();
    logout();
    navigate('/login');
    window.location.reload();
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
            <ListItemText primary="Home" sx={{ opacity: open ? 1 : 0 }} />
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
            <ListItemText primary="Past Sprints" sx={{ opacity: open ? 1 : 0 }} />
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
                <ListItemText primary="Admin" sx={{ opacity: open ? 1 : 0 }} />
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
                        <ListItemText primary="Teams" sx={{ opacity: open ? 1 : 0 }} />
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
                      <ListItemText primary="Users" sx={{ opacity: open ? 1 : 0 }} />
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
                      <ListItemText primary="Sprints" sx={{ opacity: open ? 1 : 0 }} />
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
          <ListItemText primary="Login" sx={{ opacity: open ? 1 : 0 }} />
        </ListItemButton>
      </ListItem>
    </List>
  );

  return (
    <>
      <DrawerHeader>
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
                    secondary={user.email}
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
                Report a Bug
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
              {mode === 'dark' ? 'Light Mode' : 'Dark Mode'}
            </MenuItem>
            <Divider />
            <MenuItem onClick={handleLogout}>
              <ListItemIcon>
                <LogoutIcon fontSize="small" />
              </ListItemIcon>
              Logout
            </MenuItem>
          </Menu>
        </>
      )}
    </>
  );
};

export default Sidebar;
