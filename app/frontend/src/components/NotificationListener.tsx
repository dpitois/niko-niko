import React, { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useSnackbar } from 'notistack';

const NotificationListener: React.FC = () => {
  const { enqueueSnackbar } = useSnackbar();

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("/notificationHub", {
        accessTokenFactory: () => {
          const token = localStorage.getItem('jwt_token');
          return token ? token : '';
        },
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveNotification", (user: string, message: string) => {
      enqueueSnackbar(`${user} - ${message}`, { variant: 'info' });
    });

    connection.start()
      .then(() => console.log('SignalR Connected!'))
      .catch(err => console.error('SignalR Connection Error: ', err));

    return () => {
      connection.stop()
        .then(() => console.log('SignalR Disconnected.'))
        .catch(err => console.error('SignalR Disconnection Error: ', err));
    };
  }, [enqueueSnackbar]);

  return null; // This component doesn't render anything itself
};

export default NotificationListener;
