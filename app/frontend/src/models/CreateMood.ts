import type { MoodType } from './MoodType';

export interface CreateMood {
  userId: string;
  sprintId: string;
  mood: MoodType;
  date?: string; // Optional date for the mood entry
}
