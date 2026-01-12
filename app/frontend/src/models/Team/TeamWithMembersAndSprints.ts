import type { Sprint } from '@/models/Sprint';
import type { TeamDto } from '@/models/Team';
import type { User } from '@/models/User';

export interface TeamWithMembersAndSprints extends TeamDto {
  sprints: Sprint[];
  members: User[];
}
