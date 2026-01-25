import type { Sprint } from '@/models/Sprint';
import type { User } from '@/models/User';

export interface TeamWithSprintsDto {
  id: string;
  name: string;
  adminId: string;
  createdAt: string;
  defaultSprintDuration?: number;
  sprints: Sprint[];
  members: User[];
}
