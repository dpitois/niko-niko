import type { User } from '@/models/User';

import api from './api';

export const getUsers = async (): Promise<User[]> => {
  const { data } = await api.get('/users');
  return data;
};

export const getUser = async (userId: string): Promise<User> => {
  const { data } = await api.get(`/users/${userId}`);
  return data;
};

export const deleteUser = async (userId: string): Promise<void> => {
  await api.delete(`/users/${userId}`);
};

export const deleteMe = async (): Promise<void> => {
  await api.delete('/users/me');
};
