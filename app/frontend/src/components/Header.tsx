import React from 'react';
import { NavLink, useNavigate } from 'react-router-dom'; // Corrected import
import { useAuth } from '../context/AuthContext';

// Material UI Imports
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Box from '@mui/material/Box';
import Avatar from '@mui/material/Avatar';

const Header: React.FC = () => {
  const { user, logout, isSuperAdmin } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
    window.location.reload();
  };

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          Niko Niko Calendar
        </Typography>

        {user && (
          <Button color="inherit" component={NavLink} to="/dashboard">
            Dashboard
          </Button>
        )}

        {user && (
          <Box sx={{ display: 'flex', alignItems: 'center', ml: 2 }}>
            {isSuperAdmin && (
              <Button color="inherit" component={NavLink} to="/admin">
                Admin
              </Button>
            )}
            <Button color="inherit" component={NavLink} to="/sprint/create">
              Create Sprint
            </Button>
            
            <Box sx={{ display: 'flex', alignItems: 'center', ml: 2, mr: 1 }}>
              <Avatar 
                alt={user.name || user.email} 
                src={user.avatar_url} 
                sx={{ width: 32, height: 32, mr: 1 }} 
              />
              <Typography variant="body2">
                {user.name || user.email}
              </Typography>
            </Box>

            <Button color="inherit" onClick={handleLogout}>
              Logout
            </Button>
          </Box>
        )}

        {!user && (
          <Button color="inherit" component={NavLink} to="/login">
            Login
          </Button>
        )}
      </Toolbar>
    </AppBar>
  );
};

export default Header;
