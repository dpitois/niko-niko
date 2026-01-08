import React from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import {
  Drawer,
  Box,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Typography,
  Divider,
  Avatar,
  IconButton,
  Tooltip
} from '@mui/material';
import {
  Dashboard as DashboardIcon,
  History as HistoryIcon,
  MailOutline as MailOutlineIcon,
  People as PeopleIcon,
  GroupWork as GroupWorkIcon,
  Timeline as TimelineIcon,
  Logout as LogoutIcon,
  ChevronLeft as ChevronLeftIcon,
  ChevronRight as ChevronRightIcon,
  Login as LoginIcon
} from '@mui/icons-material';
import { styled, useTheme } from '@mui/material/styles';

const DrawerHeader = styled('div')(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  padding: theme.spacing(0, 1),
  ...theme.mixins.toolbar,
  justifyContent: 'flex-end',
}));

interface SidebarProps {
  drawerWidth: number;
  open: boolean;
  handleDrawerClose: () => void;
  handleDrawerOpen: () => void; // Added for completeness, though not used to open from within sidebar
}

const Sidebar: React.FC<SidebarProps> = ({ drawerWidth, open, handleDrawerClose }) => {
  const { user, logout, isSuperAdmin } = useAuth();
  const navigate = useNavigate();
  const theme = useTheme();

  const handleLogout = () => {
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
            <ListItemText primary="Current Dashboard" sx={{ opacity: open ? 1 : 0 }} />
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

        <ListItem disablePadding sx={{ display: 'block' }}>
          <ListItemButton
            component={NavLink}
            to="/team-invitations"
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
              <MailOutlineIcon />
            </ListItemIcon>
            <ListItemText primary="Invitations" sx={{ opacity: open ? 1 : 0 }} />
          </ListItemButton>
        </ListItem>
      </List>

      {isSuperAdmin && (
        <>
          <Divider />
          <List>
            <ListItem disablePadding sx={{ display: 'block' }}>
              <ListItemButton
                component={NavLink}
                to="/admin/teams"
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
                  <GroupWorkIcon />
                </ListItemIcon>
                <ListItemText primary="Admin Teams" sx={{ opacity: open ? 1 : 0 }} />
              </ListItemButton>
            </ListItem>

            <ListItem disablePadding sx={{ display: 'block' }}>
              <ListItemButton
                component={NavLink}
                to="/admin/users"
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
                  <PeopleIcon />
                </ListItemIcon>
                <ListItemText primary="Admin Users" sx={{ opacity: open ? 1 : 0 }} />
              </ListItemButton>
            </ListItem>

            <ListItem disablePadding sx={{ display: 'block' }}>
              <ListItemButton
                component={NavLink}
                to="/admin/sprints"
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
                <ListItemText primary="Admin Sprints" sx={{ opacity: open ? 1 : 0 }} />
              </ListItemButton>
            </ListItem>
          </List>
        </>
      )}
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
    <Drawer
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: drawerWidth,
          boxSizing: 'border-box',
        },
      }}
      variant="persistent"
      anchor="left"
      open={open}
    >
      <DrawerHeader>
        <IconButton onClick={handleDrawerClose}>
          {theme.direction === 'ltr' ? <ChevronLeftIcon /> : <ChevronRightIcon />}
        </IconButton>
      </DrawerHeader>
      <Divider />
      {navItems}
      <Box sx={{ flexGrow: 1 }} /> {/* Pushes user info to the bottom */}
      <Divider />
      {user && (
        <Box sx={{ p: 2, display: 'flex', alignItems: 'center', justifyContent: open ? 'flex-start' : 'center' }}>
          <Avatar sx={{ mr: open ? 2 : 0 }}>{user.email ? user.email[0].toUpperCase() : '?'}</Avatar>
          {open && (
            <Tooltip title={user.email} arrow>
              <Typography variant="body1" noWrap>{user.email}</Typography>
            </Tooltip>
          )}
        </Box>
      )}
      {user && (
        <List>
          <ListItem disablePadding sx={{ display: 'block' }}>
            <ListItemButton onClick={handleLogout}
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
                <LogoutIcon />
              </ListItemIcon>
              <ListItemText primary="Logout" sx={{ opacity: open ? 1 : 0 }} />
            </ListItemButton>
          </ListItem>
        </List>
      )}
    </Drawer>
  );
};

export default Sidebar;
