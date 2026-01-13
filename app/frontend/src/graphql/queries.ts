import { gql } from '@apollo/client';

export const GET_MY_TEAMS_DASHBOARD = gql`
  query GetMyTeamsDashboard {
    myTeams {
      id
      name
      adminId
      admin {
        id
        name
      }
      members {
        id
        name
        email
        avatarUrl
      }
      sprints(order: { startDate: DESC }) {
        id
        name
        startDate
        endDate
        moodEntries {
          id
          mood
          date
          userId
        }
      }
    }
  }
`;
