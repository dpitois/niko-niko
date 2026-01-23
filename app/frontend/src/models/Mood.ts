import type { MoodType } from './MoodType';

export interface Mood {
  id: string;
  userId: string | null;
  sprintId: string;
  date: string;
  mood: MoodType;
}
