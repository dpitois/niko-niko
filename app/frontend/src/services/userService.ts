import api from './api';
import type { User } from '../models/User';

export const getUsers = async (): Promise<User[]> => {
  const { data } = await api.get('/users');
  return data;
};

export const deleteUser = async (userId: string): Promise<void> => {
  await api.delete(`/users/${userId}`);
};
