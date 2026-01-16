import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import SentimentDissatisfiedIcon from '@mui/icons-material/SentimentDissatisfied';
import SentimentNeutralIcon from '@mui/icons-material/SentimentNeutral';
import SentimentSatisfiedAltIcon from '@mui/icons-material/SentimentSatisfiedAlt';
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
  sprintId: string;
  sprintStartDate: string;
  sprintEndDate: string;
}

const DailyMoodWidget: React.FC<DailyMoodWidgetProps> = ({
  sprintId,
  sprintStartDate,
  sprintEndDate,
}) => {
  const { t } = useTranslation();
  const { user } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const theme = useTheme();
  
  const [selectedDate, setSelectedDate] = useState<Dayjs>(dayjs());

  // Data fetching
  const { moods, isLoading } = useMoods(sprintId);
  // Mutation logic
  const { saveMood, isUpdating } = useMoodMutation(sprintId);

  const serverMood = moods?.find(
    (m) => m.userId === user?.sub && dayjs(m.date).startOf('day').isSame(selectedDate.startOf('day'))
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
      type: MoodValues.Sad,
      icon: <SentimentDissatisfiedIcon sx={{ fontSize: 80 }} />,
      color: theme.palette.error.main,
      label: t('mood.sad', 'Sad'),
    },
    {
      type: MoodValues.Neutral,
      icon: <SentimentNeutralIcon sx={{ fontSize: 80 }} />,
      color: theme.palette.warning.main,
      label: t('mood.neutral', 'Neutral'),
    },
    {
      type: MoodValues.Happy,
      icon: <SentimentSatisfiedAltIcon sx={{ fontSize: 80 }} />,
      color: theme.palette.success.main,
      label: t('mood.happy', 'Happy'),
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
          spacing={3}
          justifyContent="center"
          alignItems="center"
          sx={{ mt: 4, mb: 2 }}
        >
          {isLoading ? (
            <CircularProgress size={40} />
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
                      p: 2,
                      width: 110,
                      height: 110,
                      '& svg': {
                        fontSize: 80,
                      }
                    }}
                  >
                    {button.icon}
                  </IconButton>
                </Tooltip>
              );
            })
          )}
        </Stack>

        {currentMoodValue !== undefined && (
          <Typography variant="body2" color="text.secondary" align="center" sx={{ mt: 1 }}>
            {t('dashboard.moodRecorded', 'Humeur enregistrée pour ce jour')}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
};

export default DailyMoodWidget;