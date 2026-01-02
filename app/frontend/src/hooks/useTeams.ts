import useSWR from 'swr';
import { getTeams } from '../services/teamService';

export const useTeams = () => {
    // The key '/teams' is used to cache the data.
    // SWR will call the 'getTeams' function to fetch the data.
    const { data, error, isLoading, mutate } = useSWR('/teams', getTeams);

    return {
        teams: data,
        isLoading,
        isError: error,
        mutateTeams: mutate
    };
};
