import dayjs from 'dayjs';

import type { CreateMood } from '@/models/CreateMood';
import type { Mood } from '@/models/Mood';
import type { PagedResult } from '@/models/PagedResult';

import api from './api';

export const createMoodEntry = async (moodEntry: CreateMood): Promise<Mood> => {
  const payload = {
    ...moodEntry,
    timezoneOffset: dayjs().utcOffset(),
  };
  const { data } = await api.post('/moodentries', payload);
  return data;
};

export const updateMoodEntry = async (moodEntry: CreateMood): Promise<Mood> => {
  // The backend uses a POST for both create and update (upsert)
  const { data } = await api.post('/moodentries', moodEntry);
  return data;
};

export const getMoodEntriesBySprint = async (
  sprintId: string,
  userId?: string,
  date?: string,
): Promise<Mood[]> => {
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

export const getMyMoodHistory = async (page = 1, pageSize = 20): Promise<PagedResult<Mood>> => {
  const { data } = await api.get(`/moodentries/me?page=${page}&pageSize=${pageSize}`);
  return data;
};
