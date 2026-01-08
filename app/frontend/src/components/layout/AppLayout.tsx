import React from 'react';
import { Box, CssBaseline } from '@mui/material';
import { styled } from '@mui/material/styles';
import Sidebar from './Sidebar'; // We will create this next
import NotificationListener from '../NotificationListener';

const drawerWidth = 240;

const Main = styled('main', { shouldForwardProp: (prop) => prop !== 'open' })<{
  open?: boolean;
}>(({ theme, open }) => ({
  flexGrow: 1,
  padding: theme.spacing(3),
  transition: theme.transitions.create('margin', {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.leavingScreen,
  }),
  marginLeft: `-${drawerWidth}px`,
  ...(open && {
    transition: theme.transitions.create('margin', {
      easing: theme.transitions.easing.easeOut,
      duration: theme.transitions.duration.enteringScreen,
    }),
    marginLeft: 0,
  }),
}));

const DrawerHeader = styled('div')(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  padding: theme.spacing(0, 1),
  // necessary for content to be below app bar
  ...theme.mixins.toolbar,
  justifyContent: 'flex-end',
}));

interface AppLayoutProps {
  children: React.ReactNode;
}

const AppLayout: React.FC<AppLayoutProps> = ({ children }) => {
  const [open, setOpen] = React.useState(true); // Sidebar open by default

  const handleDrawerOpen = () => {
    setOpen(true);
  };

  const handleDrawerClose = () => {
    setOpen(false);
  };

  return (
    <Box sx={{ display: 'flex' }}>
      <CssBaseline />
      <NotificationListener /> {/* Render NotificationListener outside the main content flow for toasts etc. */}

      <Sidebar
        drawerWidth={drawerWidth}
        open={open}
        handleDrawerClose={handleDrawerClose}
        handleDrawerOpen={handleDrawerOpen} // Pass handleDrawerOpen if we need an app bar to open it
      />
      <Main open={open}>
        <DrawerHeader /> {/* This pushes content below the AppBar space if one existed, but serves to offset content when drawer is closed */}
        {children}
      </Main>
    </Box>
  );
};

export default AppLayout;
