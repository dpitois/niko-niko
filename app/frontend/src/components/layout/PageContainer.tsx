import React from 'react';
import { Box, Typography } from '@mui/material';

interface PageContainerProps {
  title: string;
  icon?: React.ReactNode;
  action?: React.ReactNode;
  children: React.ReactNode;
}

const PageContainer: React.FC<PageContainerProps> = ({ title, icon, action, children }) => {
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          {icon && (
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'text.secondary',
                '& svg': { fontSize: 32 }, // Slightly larger icon for H4 title
              }}
            >
              {icon}
            </Box>
          )}
          <Typography variant="h4" component="h1">
            {title}
          </Typography>
        </Box>
        {action && <Box>{action}</Box>}
      </Box>
      {children}
    </Box>
  );
};

export default PageContainer;
