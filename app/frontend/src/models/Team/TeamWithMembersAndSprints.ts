import type { Sprint } from '../Sprint';
import type { TeamDto } from '../Team';
import type { User } from '../User';

export interface TeamWithMembersAndSprints extends TeamDto {
  sprints: Sprint[];
  members: User[];
}
