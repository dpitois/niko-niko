import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, fireEvent } from '@testing-library/react';
import { render } from '../../test/test-utils';
import DailyMoodWidget from './DailyMoodWidget';
import { MoodValues } from '@/models/MoodType';
import dayjs from 'dayjs';

// Mocks des hooks
const mockSaveMood = vi.fn();
const mockUser = { sub: 'user123', name: 'Test User' };

vi.mock('@/context/AuthContext', () => ({
  useAuth: () => ({
    user: mockUser,
    userTeamRoles: {
      'team1': { isMember: true, isAdmin: false }
    }
  }),
}));

vi.mock('@/hooks/useMoods', () => ({
  useMoods: () => ({
    moods: [],
    isLoading: false
  }),
}));

vi.mock('@/hooks/useMoodMutation', () => ({
  useMoodMutation: () => ({
    saveMood: mockSaveMood,
    isUpdating: false
  }),
}));

// Mock de useTranslation pour éviter les clés de traduction brutes
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, defaultValue: string) => defaultValue || key,
  }),
}));

describe('DailyMoodWidget', () => {
  const defaultProps = {
    teamId: 'team1',
    sprintId: 'sprint1',
    sprintStartDate: '2025-01-01',
    sprintEndDate: '2025-01-14',
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders the widget title', () => {
    render(<DailyMoodWidget {...defaultProps} />);
    expect(screen.getByText('Comment allez-vous ?')).toBeInTheDocument();
  });

  it('renders all mood buttons', () => {
    render(<DailyMoodWidget {...defaultProps} />);
    // Il y a 5 boutons d'humeur
    const buttons = screen.getAllByRole('button');
    // Note: Le DatePicker a aussi un bouton pour le calendrier, donc on filtre ou on compte
    // Les boutons d'humeur ont des tooltips, c'est un bon moyen de les repérer si besoin, 
    // ou simplement vérifier qu'on a bien les icones.
    // Ici on vérifie simplement qu'on a au moins les 5 boutons d'action + le datepicker
    expect(buttons.length).toBeGreaterThanOrEqual(5); 
  });

  it('calls saveMood when a mood button is clicked', () => {
    render(<DailyMoodWidget {...defaultProps} />);
    
    // On cible un bouton spécifique via son label (tooltip)
    // "Very Happy" est le label par défaut dans le mock useTranslation
    const happyButton = screen.getByRole('button', { name: 'Very Happy' });
    
    fireEvent.click(happyButton);

    expect(mockSaveMood).toHaveBeenCalledTimes(1);
    // Vérifie que saveMood est appelé avec la bonne humeur (VeryHappy = 4)
    expect(mockSaveMood).toHaveBeenCalledWith(
      expect.any(Object), // Date object (dayjs)
      MoodValues.VeryHappy, 
      mockUser.sub
    );
  });
});
