import React from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import {
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
  People as PeopleIcon,
  GroupWork as GroupWorkIcon,
  Timeline as TimelineIcon,
  Logout as LogoutIcon,
  ChevronLeft as ChevronLeftIcon,
  ChevronRight as ChevronRightIcon,
  Login as LoginIcon,
  ExpandMore
} from '@mui/icons-material';
import { styled } from '@mui/material/styles';
import MuiAccordion from '@mui/material/Accordion';
import MuiAccordionSummary from '@mui/material/AccordionSummary';
import MuiAccordionDetails from '@mui/material/AccordionDetails';

const Accordion = styled(MuiAccordion)(() => ({
  '&:not(:last-child)': {
    borderBottom: 0,
  },
  '&::before': {
    display: 'none',
  },
}));

const AccordionSummary = styled(MuiAccordionSummary)(({ theme }) => ({
  backgroundColor:
    theme.palette.mode === 'dark'
      ? 'rgba(255, 255, 255, .05)'
      : 'rgba(0, 0, 0, .03)',
  flexDirection: 'row-reverse',
  '& .MuiAccordionSummary-expandIconWrapper.Mui-expanded': {
    transform: 'rotate(90deg)',
  },
  '& .MuiAccordionSummary-content': {
    marginLeft: theme.spacing(1),
  },
}));

const AccordionDetails = styled(MuiAccordionDetails)(({ theme }) => ({
  padding: theme.spacing(2),
  borderTop: '1px solid rgba(0, 0, 0, .125)',
}));


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
  const { user, logout, isSuperAdmin } = useAuth();
  const navigate = useNavigate();

  const [expanded, setExpanded] = React.useState<string | false>(false);

  const handleChange =
    (panel: string) => (_event: React.SyntheticEvent, isExpanded: boolean) => {
      setExpanded(isExpanded ? panel : false);
    };


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
      </List>

      {isSuperAdmin && (
        <>
          <Divider />
          <Accordion expanded={expanded === 'panel1'} onChange={handleChange('panel1')} sx={{ width: '100%' }}>
            <AccordionSummary
              expandIcon={<ExpandMore sx={{ fontSize: '1.2rem' }} />}
              aria-controls="panel1d-content"
              id="panel1d-header"
              sx={{
                minHeight: 48,
                justifyContent: open ? 'initial' : 'center',
                px: 2.5,
                '& .MuiAccordionSummary-content': {
                  justifyContent: open ? 'initial' : 'center',
                }
              }}
            >
              <ListItemIcon
                sx={{
                  minWidth: 0,
                  mr: open ? 3 : 'auto',
                  justifyContent: 'center',
                }}
              >
                <PeopleIcon /> {/* Using PeopleIcon for Admin group */}
              </ListItemIcon>
              <ListItemText primary="Admin" sx={{ opacity: open ? 1 : 0 }} />
            </AccordionSummary>
            <AccordionDetails sx={{ padding: 0 }}>
              <List component="div" disablePadding>
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
            </AccordionDetails>
          </Accordion>
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
        <Box sx={{ p: 2, display: 'flex', alignItems: 'center', justifyContent: open ? 'flex-start' : 'center' }}>
          <Avatar sx={{ mr: open ? 2 : 0 }}>{user.name ? user.name[0].toUpperCase() : '?'}</Avatar>
          {open && (
            <Tooltip title={user.email} arrow>
              <Typography variant="body1" noWrap>{user.name}</Typography>
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
    </>
  );
};

export default Sidebar;
