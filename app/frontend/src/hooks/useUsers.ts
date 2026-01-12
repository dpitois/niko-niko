import useSWR from 'swr';
import { getUsers } from '../services/userService';

export const useUsers = () => {
  // The key '/users' is used to cache the data.
  // SWR will call the 'getUsers' function to fetch the data.
  const { data, error, isLoading, mutate } = useSWR('/users', getUsers);

  return {
    users: data,
    isLoading,
    isError: error,
    mutate,
  };
};
