export interface CreateTeamInvitation {
  teamId: string;
  expirationInDays?: number; // Optional, backend defaults to 7
}
