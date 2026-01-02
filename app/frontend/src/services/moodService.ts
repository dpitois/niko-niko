import api from './api';
import type { Mood } from '../models/Mood';
import type { CreateMood } from '../models/CreateMood';

export const createMoodEntry = async (moodEntry: CreateMood): Promise<Mood> => {
  const { data } = await api.post('/moodentries', moodEntry);
  return data;
};

export const getMoodEntriesBySprint = async (sprintId: string, userId?: string, date?: string): Promise<Mood[]> => {
  let url = `/moodentries/bysprint/${sprintId}`;
  const params = new URLSearchParams();
  if (userId) {
    params.append('userId', userId);
  }
  if (date) {
    params.append('date', date);
  }
  if (params.toString()) {
    url += `?${params.toString()}`;
  }
  const { data } = await api.get(url);
  return data;
};
