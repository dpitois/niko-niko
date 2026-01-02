import type { MoodType } from './MoodType';

export interface Mood {
  id: string;
  userId: string;
  sprintId: string;
  date: string;
  mood: MoodType;
}
