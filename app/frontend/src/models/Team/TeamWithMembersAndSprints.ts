import type { TeamDto } from '../Team';
import type { Sprint } from '../Sprint'; // Assuming Sprint interface exists
import type { User } from '../User';

export interface TeamWithMembersAndSprints extends TeamDto {
  sprints: Sprint[];
  members: User[];
}
