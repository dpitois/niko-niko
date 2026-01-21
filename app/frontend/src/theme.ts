import type { PaletteMode } from '@mui/material';
import { createTheme } from '@mui/material/styles';

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
      background: {
        default: mode === 'dark' ? '#121212' : '#f5f7f9',
        paper: mode === 'dark' ? '#1e1e1e' : '#ffffff',
      },
      ...(mode === 'dark' && {
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
      MuiAppBar: {
        styleOverrides: {
          root: {
            backgroundColor: mode === 'dark' ? '#1e1e1e' : '#ffffff',
            color: mode === 'dark' ? '#ffffff' : '#000000',
            boxShadow: mode === 'dark' ? 'none' : '0px 1px 3px rgba(0, 0, 0, 0.1)',
            borderBottom: mode === 'dark' ? '1px solid rgba(255, 255, 255, 0.12)' : 'none',
          },
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: {
            backgroundImage: 'none',
          },
          elevation1: {
            boxShadow: mode === 'dark' ? 'none' : '0px 1px 3px rgba(0, 0, 0, 0.05)',
            border:
              mode === 'dark'
                ? '1px solid rgba(255, 255, 255, 0.12)'
                : '1px solid rgba(0, 0, 0, 0.08)',
          },
        },
      },
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
