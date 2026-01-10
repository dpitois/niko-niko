import { createTheme } from '@mui/material/styles';
import type { PaletteMode } from '@mui/material';

export const getTheme = (mode: PaletteMode) => {
  return createTheme({
    palette: {
      mode,
      primary: {
        main: mode === 'dark' ? '#90caf9' : '#007bff',
      },
      secondary: {
        main: mode === 'dark' ? '#ce93d8' : '#6c757d',
      },
      ...(mode === 'dark' && {
        background: {
          default: '#121212',
          paper: '#1e1e1e',
        },
        text: {
          primary: '#ffffff',
          secondary: 'rgba(255, 255, 255, 0.7)',
        },
      }),
    },
    typography: {
      fontFamily: 'Arial, Helvetica, sans-serif',
      fontSize: 14,
      h1: {
        fontSize: '2rem',
        fontWeight: 500,
        color: mode === 'dark' ? '#90caf9' : '#007bff',
        marginBottom: '1rem',
      },
      h2: {
        fontSize: '1.5rem',
        fontWeight: 500,
        color: mode === 'dark' ? '#64b5f6' : '#0056b3',
        marginBottom: '1rem',
      },
      h3: {
        fontSize: '1.2rem',
        fontWeight: 500,
        color: mode === 'dark' ? '#42a5f5' : '#004085',
        marginBottom: '0.8rem',
      },
    },
    components: {
      MuiButton: {
        styleOverrides: {
          root: {
            textTransform: 'none',
            borderRadius: 4,
            padding: '0.8em 1.2em',
          },
        },
      },
      MuiTextField: {
        defaultProps: {
          variant: 'outlined',
          size: 'small',
        },
      },
      MuiSelect: {
        defaultProps: {
          variant: 'outlined',
          size: 'small',
        },
      },
      MuiInputLabel: {
        styleOverrides: {
          root: {
            fontWeight: 'bold',
          },
        },
      },
    },
  });
};
