import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import SentimentDissatisfiedIcon from '@mui/icons-material/SentimentDissatisfied';
import SentimentNeutralIcon from '@mui/icons-material/SentimentNeutral';
import SentimentSatisfiedIcon from '@mui/icons-material/SentimentSatisfied';
import SentimentVeryDissatisfiedIcon from '@mui/icons-material/SentimentVeryDissatisfied';
import SentimentVerySatisfiedIcon from '@mui/icons-material/SentimentVerySatisfied';
import {
  alpha,
  Box,
  Card,
  CardContent,
  CircularProgress,
  IconButton,
  Stack,
  Tooltip,
  Typography,
  useTheme,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import dayjs, { Dayjs } from 'dayjs';
import { useSnackbar } from 'notistack';

import { useAuth } from '@/context/AuthContext';
import { useMoodMutation } from '@/hooks/useMoodMutation';
import { useMoods } from '@/hooks/useMoods';
import type { MoodType } from '@/models/MoodType';
import { MoodValues } from '@/models/MoodType';

interface DailyMoodWidgetProps {
  teamId: string;
  sprintId: string;
  sprintStartDate: string;
  sprintEndDate: string;
}

const DailyMoodWidget: React.FC<DailyMoodWidgetProps> = ({
  teamId,
  sprintId,
  sprintStartDate,
  sprintEndDate,
}) => {
  const { t } = useTranslation();
  const { user, userTeamRoles } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const theme = useTheme();

  const canPost = userTeamRoles[teamId]?.isMember || userTeamRoles[teamId]?.isAdmin;

  const [selectedDate, setSelectedDate] = useState<Dayjs>(dayjs());

  // Data fetching
  const { moods, isLoading } = useMoods(sprintId);
  // Mutation logic
  const { saveMood, isUpdating } = useMoodMutation(sprintId);

  const serverMood = moods?.find(
    (m) =>
      m.userId === user?.sub && dayjs(m.date).startOf('day').isSame(selectedDate.startOf('day')),
  );

  const currentMoodValue = serverMood?.mood;

  const handleMoodSelect = async (moodType: MoodType) => {
    if (!user || isUpdating) return;

    try {
      await saveMood(selectedDate, moodType, user.sub);
      enqueueSnackbar(t('common.success'), { variant: 'success' });
    } catch {
      enqueueSnackbar(t('common.error'), { variant: 'error' });
    }
  };

  const moodButtons = [
    {
      type: MoodValues.VerySad,
      icon: <SentimentVeryDissatisfiedIcon />,
      color: theme.palette.error.dark,
      label: t('mood.verySad', 'Very Sad'),
    },
    {
      type: MoodValues.Sad,
      icon: <SentimentDissatisfiedIcon />,
      color: theme.palette.error.main,
      label: t('mood.sad', 'Sad'),
    },
    {
      type: MoodValues.Neutral,
      icon: <SentimentNeutralIcon />,
      color: theme.palette.warning.main,
      label: t('mood.neutral', 'Neutral'),
    },
    {
      type: MoodValues.Happy,
      icon: <SentimentSatisfiedIcon />,
      color: theme.palette.success.light,
      label: t('mood.happy', 'Happy'),
    },
    {
      type: MoodValues.VeryHappy,
      icon: <SentimentVerySatisfiedIcon />,
      color: theme.palette.success.main,
      label: t('mood.veryHappy', 'Very Happy'),
    },
  ];

  return (
    <Card sx={{ height: '100%', boxShadow: 3, borderRadius: 2 }}>
      <CardContent>
        <Typography variant="h6" gutterBottom align="center">
          {t('dashboard.howAreYouToday', 'Comment allez-vous ?')}
        </Typography>

        <Box sx={{ mt: 2, display: 'flex', justifyContent: 'center' }}>
          <DatePicker
            label={t('dashboard.selectDate', 'Sélectionner une date')}
            value={selectedDate}
            onChange={(newValue) => newValue && setSelectedDate(newValue)}
            minDate={dayjs(sprintStartDate)}
            maxDate={dayjs(sprintEndDate).isBefore(dayjs()) ? dayjs(sprintEndDate) : dayjs()}
            slotProps={{ textField: { size: 'small', fullWidth: true } }}
          />
        </Box>

        <Stack
          direction="row"
          spacing={1}
          justifyContent="center"
          alignItems="center"
          sx={{ mt: 4, mb: 2, minHeight: 90 }}
        >
          {isLoading ? (
            <CircularProgress size={40} />
          ) : !canPost ? (
            <Box
              sx={{
                p: 2,
                borderRadius: 2,
                bgcolor: alpha(theme.palette.info.main, 0.1),
                border: `1px dashed ${theme.palette.info.main}`,
                width: '100%',
                textAlign: 'center',
              }}
            >
              <Typography variant="body2" color="info.main">
                {t('dashboard.observerMode', 'Vous visualisez cette équipe en mode observateur.')}
              </Typography>
            </Box>
          ) : (
            moodButtons.map((button) => {
              const isSelected = currentMoodValue === button.type;
              return (
                <Tooltip key={button.type} title={button.label}>
                  <IconButton
                    onClick={() => handleMoodSelect(button.type)}
                    disabled={isUpdating}
                    sx={{
                      color: isSelected ? '#fff' : theme.palette.text.disabled,
                      backgroundColor: isSelected ? button.color : 'transparent',
                      border: `2px solid ${isSelected ? button.color : 'transparent'}`,
                      transition: 'all 0.2s',
                      '&:hover': {
                        backgroundColor: isSelected ? button.color : alpha(button.color, 0.2),
                        color: isSelected ? '#fff' : button.color,
                        transform: 'scale(1.1)',
                      },
                      p: 1.5,
                      width: 75,
                      height: 75,
                      '& svg': {
                        fontSize: 50,
                      },
                    }}
                  >
                    {button.icon}
                  </IconButton>
                </Tooltip>
              );
            })
          )}
        </Stack>

        {canPost && currentMoodValue !== undefined && (
          <Typography variant="body2" color="text.secondary" align="center" sx={{ mt: 1 }}>
            {t('dashboard.moodRecorded', 'Humeur enregistrée pour ce jour')}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
};

export default DailyMoodWidget;
