import type { Sprint } from '../Sprint';
import type { User } from '../User';

export interface TeamWithSprintsDto {
  id: string;
  name: string;
  adminId: string;
  createdAt: string;
  sprints: Sprint[];
  members: User[];
}
