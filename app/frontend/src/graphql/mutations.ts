import { gql } from '@apollo/client';

export const ADD_MOOD_ENTRY = gql`
  mutation AddMoodEntry($input: CreateMoodEntryDtoInput!) {
    addMoodEntry(input: $input) {
      id
      mood
      date
      userId
      sprintId
    }
  }
`;