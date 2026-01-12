import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import './index.css';
import App from './App.tsx';
import { AuthProvider } from './context/AuthContext.tsx'; // Import AuthProvider
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs from 'dayjs';
import 'dayjs/locale/fr';

const userLocale = navigator.language.startsWith('fr') ? 'fr' : 'en';
dayjs.locale(userLocale);

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <AuthProvider>
        {' '}
        {/* Wrap App with AuthProvider */}
        <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale={userLocale}>
          <App />
        </LocalizationProvider>
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
);
