import type { Sprint } from './Sprint';
import type { TeamDto } from './Team';

export interface TeamWithSprints extends TeamDto {
  sprints: Sprint[];
}
