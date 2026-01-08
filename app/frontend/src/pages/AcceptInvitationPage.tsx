import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Box, Typography, CircularProgress, Alert, Button } from '@mui/material';
import { teamInvitationService } from '../services/teamInvitationService';
import { useAuth } from '../context/AuthContext';
import useTeams from '../hooks/useTeams';
import axios from 'axios';

const AcceptInvitationPage: React.FC = () => {
  const { token } = useParams<{ token: string }>();
  const navigate = useNavigate();
  const { user, isLoading: isLoadingAuth } = useAuth(); // Récupérer isLoading du useAuth
  const { mutate } = useTeams();
  const [loading, setLoading] = useState<boolean>(true);
  const [message, setMessage] = useState<string>('');
  const [severity, setSeverity] = useState<'success' | 'error' | 'info'>('info');

  useEffect(() => {
    // Si l'authentification est toujours en cours de chargement, ne rien faire
    if (isLoadingAuth) {
      return;
    }
    


    const handleAcceptInvitation = async () => {
      if (!token) {
        setMessage('Invalid invitation link.');
        setSeverity('error');
        setLoading(false);
        return;
      }

      if (!user) { // Cette vérification ne sera faite que si isLoadingAuth est faux
        setMessage('You need to be logged in to accept this invitation. Redirecting to login...');
        setSeverity('info');
        localStorage.setItem('invitationToken', token);
        setTimeout(() => navigate('/login'), 3000);
        setLoading(false);
        return;
      }



      try {
        const acceptedInvitation = await teamInvitationService.acceptTeamInvitation(token);
        setMessage(`Invitation to team "${acceptedInvitation.teamName}" accepted successfully!`);
        setSeverity('success');
        localStorage.removeItem('invitationToken');
        mutate();
        setTimeout(() => navigate(`/my-teams`), 3000);
      } catch (err: unknown) {
        console.error('Error accepting invitation:', err); // Log l'erreur complète
        let errorMessage = 'Failed to accept invitation.';
        if (axios.isAxiosError(err) && err.response?.data?.message) {
          errorMessage = err.response.data.message;
        } else if (err instanceof Error) {
          errorMessage = err.message;
        }
        setMessage(errorMessage);
        setSeverity('error');
      } finally {
        setLoading(false);
      }
    };

    handleAcceptInvitation();
  }, [token, navigate, user, mutate, isLoadingAuth]); // Ajouter isLoadingAuth aux dépendances

  // Rendu : Afficher un indicateur de chargement si isLoadingAuth est vrai
  if (isLoadingAuth) {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '80vh' }}>
        <CircularProgress />
        <Typography variant="h6" sx={{ mt: 2 }}>Checking authentication status...</Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '80vh' }}>
      {loading ? (
        <>
          <CircularProgress />
          <Typography variant="h6" sx={{ mt: 2 }}>Accepting Invitation...</Typography>
        </>
      ) : (
        <Alert severity={severity} sx={{ mb: 2, width: '100%', maxWidth: 400 }}>
          {message}
        </Alert>
      )}
      {!loading && severity === 'success' && (
        <Button variant="contained" onClick={() => navigate('/my-teams')} sx={{ mt: 2 }}>
          Go to My Teams
        </Button>
      )}
      {!loading && severity === 'error' && (
        <Button variant="contained" onClick={() => navigate('/')} sx={{ mt: 2 }}>
          Go to Dashboard
        </Button>
      )}
    </Box>
  );
};

export default AcceptInvitationPage;
