import useSWR from 'swr';
import { getTeams } from '../services/teamService';

const useTeams = () => {
  const { data, error, mutate } = useSWR('teams', getTeams);

  return {
    teams: data,
    isLoading: !error && !data,
    isError: error,
    mutate,
  };
};

export default useTeams;