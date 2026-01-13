import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs from 'dayjs';

import { AuthProvider } from '@/context/AuthContext.tsx';

import App from './App.tsx';

import './index.css';
import 'dayjs/locale/fr';

const userLocale = navigator.language.startsWith('fr') ? 'fr' : 'en';
dayjs.locale(userLocale);

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale={userLocale}>
          <App />
        </LocalizationProvider>
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
);
