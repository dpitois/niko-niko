import { describe, it, expect } from 'vitest';
import { calculateMoodTrend } from './moodTrendUtils';
import { MoodType } from '@/models/Mood';

// Mock simple de Mood pour les tests
const createMood = (date: string, moodValue: number, userId: string = 'user1') => ({
  id: '1',
  userId,
  sprintId: 'sprint1',
  date,
  mood: moodValue as MoodType,
});

describe('calculateMoodTrend', () => {
  it('should return empty array if no moods provided', () => {
    const result = calculateMoodTrend(undefined, '2026-01-01', '2026-01-14');
    expect(result).toEqual([]);
  });

  it('should generate points for each day in range up to today', () => {
    // Si la fin du sprint est dans le futur, on s'arrête à aujourd'hui (simulé ici via la logique interne)
    // Pour simplifier le test sans mocker dayjs().startOf('day'), prenons un sprint passé
    const result = calculateMoodTrend([], '2025-01-01', '2025-01-05');
    
    expect(result).toHaveLength(5);
    expect(result[0].date).toBe('2025-01-01');
    expect(result[4].date).toBe('2025-01-05');
  });

  it('should calculate average correctly', () => {
    const moods = [
      createMood('2025-01-01T10:00:00Z', 4), // Value 5 (index + 1)
      createMood('2025-01-01T12:00:00Z', 0), // Value 1
    ];

    const result = calculateMoodTrend(moods, '2025-01-01', '2025-01-01');
    
    // (5 + 1) / 2 = 3
    expect(result[0].average).toBe(3);
  });

  it('should calculate user average specifically when userId provided', () => {
    const userId = 'targetUser';
    const moods = [
      createMood('2025-01-01T10:00:00Z', 4, userId), // User's mood: 5
      createMood('2025-01-01T12:00:00Z', 0, 'otherUser'), // Other's mood: 1
    ];

    const result = calculateMoodTrend(moods, '2025-01-01', '2025-01-01', userId);
    
    // Global avg: (5 + 1) / 2 = 3
    expect(result[0].average).toBe(3);
    // User avg: 5 / 1 = 5
    expect(result[0].userAverage).toBe(5);
  });

  it('should return null average for days without moods', () => {
    const result = calculateMoodTrend([], '2025-01-01', '2025-01-01');
    expect(result[0].average).toBeNull();
  });
});
