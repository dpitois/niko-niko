import type { User } from '../User';
import type { Sprint } from '../Sprint';

export interface TeamWithSprintsDto {
  id: string;
  name: string;
  adminId: string;
  createdAt: string;
  sprints: Sprint[];
  members: User[];
}
